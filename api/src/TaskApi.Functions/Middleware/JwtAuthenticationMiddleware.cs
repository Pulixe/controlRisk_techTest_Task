using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using TaskApi.Functions.Services;
using System.Linq;

namespace TaskApi.Functions.Middleware
{
    public class JwtAuthenticationMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<JwtAuthenticationMiddleware> _logger;
        private readonly IConfiguration _config;
        private readonly IUserService _userService;
        private Microsoft.IdentityModel.Protocols.IConfigurationManager<OpenIdConnectConfiguration>? _configurationManager;

        public const string ContextUserKey = "__auth:user";
        public const string ContextTokenKey = "__auth:token";

        public JwtAuthenticationMiddleware(ILogger<JwtAuthenticationMiddleware> logger, IConfiguration config, IUserService userService)
        {
            _logger = logger;
            _config = config;
            _userService = userService;

            // Defer metadata manager initialization to first request when authority is known
            _configurationManager = null;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var req = await context.GetHttpRequestDataAsync();
            if (req == null)
            {
                await next(context);
                return;
            }
            _logger.LogInformation("Auth middleware invoked for {method} {path}", req.Method, req.Url.AbsolutePath);

            // Extract bearer token
            if (!req.Headers.TryGetValues("Authorization", out var authHeaders))
            {
                await WriteUnauthorizedAsync(context, req, "Missing Authorization header");
                return;
            }

            var authHeader = System.Linq.Enumerable.FirstOrDefault(authHeaders);
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                await WriteUnauthorizedAsync(context, req, "Invalid Authorization header");
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrWhiteSpace(token))
            {
                await WriteUnauthorizedAsync(context, req, "Bearer token empty");
                return;
            }

            try
            {
                var authority = _config["Oidc:Authority"] ?? _config["OIDC__Authority"]
                    ?? throw new InvalidOperationException("OIDC authority not configured.");
                var audience = _config["Oidc:Audience"] ?? _config["OIDC__Audience"]
                    ?? throw new InvalidOperationException("OIDC audience not configured.");

                //_logger.LogInformation("Configured OIDC issuer: {issuer}, audience: {aud}", authority, audience);

                if (_configurationManager == null)
                {
                    var metadataAddress = authority.TrimEnd('/') + "/.well-known/openid-configuration";
                    _logger.LogInformation("Fetching OIDC metadata from {metadata}", metadataAddress);
                    _configurationManager = new Microsoft.IdentityModel.Protocols.ConfigurationManager<OpenIdConnectConfiguration>(
                        metadataAddress,
                        new OpenIdConnectConfigurationRetriever(),
                        new Microsoft.IdentityModel.Protocols.HttpDocumentRetriever { RequireHttps = true }
                    );
                }
                var oidcConfig = await _configurationManager!.GetConfigurationAsync(default);
                //_logger.LogInformation("Loaded {keyCount} signing keys from issuer metadata", oidcConfig.SigningKeys?.Count ?? 0);

                // Optional debug logging of token (controlled by config)
                var logJwtFlag = _config["Debug:LogJwt"] ?? _config["Debug__LogJwt"] ?? _config["DEBUG__LOG_JWT"];
                bool logJwt = string.Equals(logJwtFlag, "true", StringComparison.OrdinalIgnoreCase);

                if (logJwt)
                {
                    _logger.LogWarning("DEBUG raw JWT: {token}", token);
                }

                // Pre-parse token to log basic claims before validation
                try
                {
                    var pre = new JwtSecurityTokenHandler().ReadJwtToken(token);
                    if (logJwt)
                    {
                        var hdr = string.Join(", ", pre.Header.Select(kv => $"{kv.Key}={kv.Value}"));
                        var cls = string.Join(", ", pre.Claims.Select(c => $"{c.Type}={c.Value}"));
                        _logger.LogWarning("DEBUG JWT header: {header}", hdr);
                        _logger.LogWarning("DEBUG JWT claims (unvalidated): {claims}", cls);
                        _logger.LogWarning("DEBUG JWT iss: {iss}, aud: {aud}", pre.Issuer, pre.Audiences.FirstOrDefault());
                    }
                }
                catch { /* ignore parse errors for logging */ }

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuers = new[]
                    {
                        authority.TrimEnd('/'),
                        $"{authority.TrimEnd('/')}/v2.0"
                    },
                    
                    ValidateAudience = true,
                        ValidAudiences = new[]
                         {
                        audience,
                         $"api://{audience}"
                    },
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = oidcConfig.SigningKeys,
                    ClockSkew = TimeSpan.FromMinutes(2)
                };

                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, validationParameters, out var validated);
                var jwt = (JwtSecurityToken)validated;

                // Successful validation: optional debug of key identifiers and common claims
                if (logJwt)
                {
                    var aud = jwt.Audiences.FirstOrDefault();
                    var sub = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                    var email = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == "email")?.Value;
                    _logger.LogWarning("DEBUG Validated JWT iss: {iss}, aud: {aud}, sub: {sub}, email: {email}", jwt.Issuer, aud, sub, email);
                }

                // Ensure user exists and attach to context
                var user = await _userService.EnsureUserAsync(jwt);
                context.Items[ContextUserKey] = user;
                context.Items[ContextTokenKey] = jwt;

                await next(context);
            }
            catch (SecurityTokenException ex)
            {
                try
                {
                    var pre = new JwtSecurityTokenHandler().ReadJwtToken(token);
                    var kid = pre.Header.TryGetValue("kid", out var k) ? k?.ToString() : null;
                    var alg = pre.Header.Alg;
                    //_logger.LogWarning(ex, "JWT validation failed. iss={iss}, aud={aud}, alg={alg}, kid={kid}", pre.Issuer, pre.Audiences.FirstOrDefault(), alg, kid);
                }
                catch
                {
                    _logger.LogWarning(ex, "JWT validation failed (token parse unavailable)");
                }
                await WriteUnauthorizedAsync(context, req, "Invalid token");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication error");
                var resp = req.CreateResponse(HttpStatusCode.InternalServerError);
                await resp.WriteStringAsync("Authentication processing error");
                context.GetInvocationResult().Value = resp;
            }
        }

        private static async Task WriteUnauthorizedAsync(FunctionContext ctx, HttpRequestData req, string message)
        {
            var resp = req.CreateResponse(HttpStatusCode.Unauthorized);
            resp.Headers.Add("WWW-Authenticate", "Bearer");
            await resp.WriteStringAsync(message);
            ctx.GetInvocationResult().Value = resp;
        }
    }
}
