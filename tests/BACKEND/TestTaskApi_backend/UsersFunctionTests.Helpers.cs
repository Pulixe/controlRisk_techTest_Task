using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Net;
using System.Text;
using TaskApi.Functions.Models;

namespace TestTaskApi
{
    /// <summary>
    /// Helper methods shared across all UsersFunction tests
    /// </summary>
    public partial class UsersFunctionTests
    {
        protected static Mock<HttpRequestData> CreateMockHttpRequestData(string queryString = "")
        {
            var mockRequest = new Mock<HttpRequestData>(MockBehavior.Strict, new Mock<FunctionContext>().Object);
            var url = new Uri($"https://localhost:7071/api/users{queryString}");
            mockRequest.Setup(r => r.Url).Returns(url);
            mockRequest.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(MockBehavior.Strict, new Mock<FunctionContext>().Object);
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.Setup(r => r.WriteStringAsync(It.IsAny<string>())).Returns(ValueTask.CompletedTask);
                response.Setup(r => r.WriteAsJsonAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                    .Returns(ValueTask.CompletedTask);
                return response.Object;
            });

            return mockRequest;
        }

        protected static Mock<FunctionContext> CreateMockFunctionContext(Guid userId, string? email = null, string? name = null)
        {
            var mockContext = new Mock<FunctionContext>();
            var items = new Dictionary<object, object>
            {
                ["CurrentUser"] = new User 
                { 
                    Id = userId, 
                    Email = email ?? "test@example.com",
                    Name = name ?? "Test User"
                }
            };
            mockContext.Setup(c => c.Items).Returns(items);
            return mockContext;
        }

        protected static Mock<FunctionContext> CreateMockFunctionContextWithoutUser()
        {
            var mockContext = new Mock<FunctionContext>();
            var items = new Dictionary<object, object>();
            mockContext.Setup(c => c.Items).Returns(items);
            return mockContext;
        }
    }
}
