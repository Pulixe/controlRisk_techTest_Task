using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TaskApi.Functions.Repositories;
using TaskApi.Functions.Factories;
using System;
using System.IO;
using System.Net;
using TaskApi.Functions.Models;
using TaskApi.Functions.Extensions;

namespace TaskApi.Functions.Functions
{
    public class TasksFunction
    {
        private readonly ITaskRepository _repo;
        private readonly ITaskFactory _factory;
        private readonly ILogger _logger;

        public TasksFunction(ITaskRepository repo, ITaskFactory factory, ILoggerFactory loggerFactory)
        {
            _repo = repo;
            _factory = factory;
            _logger = loggerFactory.CreateLogger<TasksFunction>();
        }

        [Function("GetTasks")]
        public async Task<HttpResponseData> GetTasks([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tasks")] HttpRequestData req, FunctionContext context)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            string? status = query["status"];
            string? assigned = query["assignedTo"];
            string? sortBy = query["sortBy"];
            string? title = query["title"];
            string? search = query["q"];
            string? dueDateStr = query["dueDate"];
            string? dueFromStr = query["dueFrom"];
            string? dueToStr = query["dueTo"];
            bool desc = query["desc"] == "true";
            int skip = int.TryParse(query["skip"], out var s) ? s : 0;
            int take = int.TryParse(query["take"], out var t) ? t : 50;
            DateTime? dueFrom = null;
            DateTime? dueTo = null;

            // Interpret dueDate as a whole-day window [date, date+1)
            if (!string.IsNullOrWhiteSpace(dueDateStr) && DateTime.TryParse(dueDateStr, out var dd))
            {
                var day = dd.Date;
                dueFrom = day;
                dueTo = day.AddDays(1);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(dueFromStr) && DateTime.TryParse(dueFromStr, out var df))
                    dueFrom = df;
                if (!string.IsNullOrWhiteSpace(dueToStr) && DateTime.TryParse(dueToStr, out var dt2))
                    dueTo = dt2;
            }

            try
            {
                var user = context.GetCurrentUser();
                if (user == null)
                {
                    var unauth = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauth.WriteStringAsync("Unauthorized");
                    return unauth;
                }

                var list = await _repo.ListAsync(status, assigned, null, sortBy, desc, skip, take, user.Id, title, search, dueFrom, dueTo);
                var resp = req.CreateResponse(HttpStatusCode.OK);
                await resp.WriteAsJsonAsync(list);
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing tasks");
                var r = req.CreateResponse(HttpStatusCode.InternalServerError);
                await r.WriteStringAsync("Internal server error");
                return r;
            }
        }

        [Function("GetTaskById")]
        public async Task<HttpResponseData> GetTaskById([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tasks/{id:guid}")] HttpRequestData req, string id, FunctionContext context)
        {
            try
            {
                var guid = Guid.Parse(id);
                var t = await _repo.GetAsync(guid);
                if (t == null) return req.CreateResponse(HttpStatusCode.NotFound);
                var user = context.GetCurrentUser();
                if (user == null)
                {
                    var unauth = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauth.WriteStringAsync("Unauthorized");
                    return unauth;
                }
                if (t.UserId != user.Id)
                {
                    var forb = req.CreateResponse(HttpStatusCode.Forbidden);
                    await forb.WriteStringAsync("Forbidden");
                    return forb;
                }
                var r = req.CreateResponse(HttpStatusCode.OK);
                await r.WriteAsJsonAsync(t);
                return r;
            }
            catch (FormatException)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting task");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("CreateTask")]
        public async Task<HttpResponseData> CreateTask([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tasks")] HttpRequestData req, FunctionContext context)
        {
            try
            {
                using var sr = new StreamReader(req.Body);
                var body = await sr.ReadToEndAsync();
                var dto = JsonSerializer.Deserialize<CreateTaskDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
                    return req.CreateResponse(HttpStatusCode.BadRequest);

                var user = context.GetCurrentUser();
                if (user == null)
                {
                    var unauth = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauth.WriteStringAsync("Unauthorized");
                    return unauth;
                }

                // Keep CreatedBy for backward compatibility, but prefer user's email/name when available
                var createdBy = !string.IsNullOrWhiteSpace(user.Email) ? user.Email! : (user.Name ?? user.Id.ToString());
                var item = _factory.Create(dto.Title, dto.Description ?? "", dto.DueDate, createdBy, dto.AssignedTo);
                item.UserId = user.Id;
                await _repo.CreateAsync(item);

                var resp = req.CreateResponse(HttpStatusCode.Created);
                await resp.WriteAsJsonAsync(item);
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("UpdateTask")]
        public async Task<HttpResponseData> UpdateTask(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "tasks/{id:guid}")] HttpRequestData req, string id, FunctionContext context)
        {
            try
            {
                var guid = Guid.Parse(id);
                using var sr = new StreamReader(req.Body);
                var body = await sr.ReadToEndAsync();
                var dto = JsonSerializer.Deserialize<UpdateTaskDto>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true) }
                }); if (dto == null) return req.CreateResponse(HttpStatusCode.BadRequest);

                var existing = await _repo.GetAsync(guid);
                if (existing == null) return req.CreateResponse(HttpStatusCode.NotFound);
                var user = context.GetCurrentUser();
                if (user == null)
                {
                    var unauth = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauth.WriteStringAsync("Unauthorized");
                    return unauth;
                }
                if (existing.UserId != user.Id)
                {
                    var forb = req.CreateResponse(HttpStatusCode.Forbidden);
                    await forb.WriteStringAsync("Forbidden");
                    return forb;
                }

                existing.Title = dto.Title ?? existing.Title;
                existing.Description = dto.Description ?? existing.Description;
                existing.DueDate = dto.DueDate ?? existing.DueDate;
                if (dto.Status.HasValue) existing.Status = dto.Status.Value;
                existing.AssignedTo = dto.AssignedTo ?? existing.AssignedTo;

                await _repo.UpdateAsync(existing);

                var resp = req.CreateResponse(HttpStatusCode.OK);
                await resp.WriteAsJsonAsync(existing);
                return resp;
            }
            catch (FormatException)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("DeleteTask")]
        public async Task<HttpResponseData> DeleteTask([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "tasks/{id:guid}")] HttpRequestData req, string id, FunctionContext context)
        {
            try
            {
                var guid = Guid.Parse(id);
                var existing = await _repo.GetAsync(guid);
                if (existing == null) return req.CreateResponse(HttpStatusCode.NoContent);
                var user = context.GetCurrentUser();
                if (user == null)
                {
                    var unauth = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauth.WriteStringAsync("Unauthorized");
                    return unauth;
                }
                if (existing.UserId != user.Id)
                {
                    var forb = req.CreateResponse(HttpStatusCode.Forbidden);
                    await forb.WriteStringAsync("Forbidden");
                    return forb;
                }
                await _repo.DeleteAsync(guid);
                return req.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (FormatException)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        private record CreateTaskDto(string Title, string? Description, DateTime? DueDate, string CreatedBy, string? AssignedTo);
        private record UpdateTaskDto(string? Title, string? Description, DateTime? DueDate, Models.TaskStatus? Status, string? AssignedTo);
    }
}
