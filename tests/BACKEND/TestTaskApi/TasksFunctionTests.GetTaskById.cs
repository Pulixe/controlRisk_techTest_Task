using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Net;
using TaskApi.Functions.Models;
using Xunit;

namespace TestTaskApi
{
   
    public partial class TasksFunctionTests
    {
        [Fact]
        public async Task GetTaskById_WithValidIdAndAuthorizedUser_ReturnsOkWithTask()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var task = new TaskItem { Id = taskId, Title = "Test Task", UserId = userId };

            var mockRequest = CreateMockHttpRequestData();
            var mockContext = CreateMockFunctionContext(userId);

            _mockRepo.Setup(r => r.GetAsync(taskId)).ReturnsAsync(task);

            // Act
            var response = await _function.GetTaskById(mockRequest.Object, taskId.ToString(), mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _mockRepo.Verify(r => r.GetAsync(taskId), Times.Once);
        }


        [Fact]
        public async Task GetTaskById_WithoutAuthorization_ReturnsUnauthorized()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task = new TaskItem { Id = taskId, Title = "Test Task", UserId = Guid.NewGuid() };

            var mockRequest = CreateMockHttpRequestData();
            var mockContext = CreateMockFunctionContextWithoutUser();

            _mockRepo.Setup(r => r.GetAsync(taskId)).ReturnsAsync(task);

            // Act
            var response = await _function.GetTaskById(mockRequest.Object, taskId.ToString(), mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }


        [Fact]
        public async Task GetTaskById_WithInvalidGuid_ReturnsBadRequest()
        {
            // Arrange
            var mockRequest = CreateMockHttpRequestData();
            var mockContext = CreateMockFunctionContext(Guid.NewGuid());

            // Act
            var response = await _function.GetTaskById(mockRequest.Object, "invalid-guid", mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

    }
}
