using Moq;
using System.Net;
using TaskApi.Functions.Models;
using Xunit;

namespace TestTaskApi
{

    public partial class TasksFunctionTests
    {
        [Fact]
        public async Task UpdateTask_WithValidData_ReturnsOk()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var json = "{\"title\":\"Updated Task\",\"status\":1}";

            var mockRequest = CreateMockHttpRequestDataWithBody(json);
            var mockContext = CreateMockFunctionContext(userId);

            var existingTask = new TaskItem 
            { 
                Id = taskId, 
                Title = "Original Task", 
                UserId = userId,
                Status = TaskApi.Functions.Models.TaskStatus.Pending
            };

            _mockRepo.Setup(r => r.GetAsync(taskId)).ReturnsAsync(existingTask);
            _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>())).Returns(Task.CompletedTask);

            // Act
            var response = await _function.UpdateTask(mockRequest.Object, taskId.ToString(), mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _mockRepo.Verify(r => r.UpdateAsync(It.Is<TaskItem>(t => 
                t.Title == "Updated Task" && t.Status == TaskApi.Functions.Models.TaskStatus.InProgress)), Times.Once);
        }

        [Fact]
        public async Task UpdateTask_WithoutAuthorization_ReturnsUnauthorized()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var json = "{\"title\":\"Updated Task\"}";

            var mockRequest = CreateMockHttpRequestDataWithBody(json);
            var mockContext = CreateMockFunctionContextWithoutUser();

            var existingTask = new TaskItem 
            { 
                Id = taskId, 
                Title = "Original Task", 
                UserId = Guid.NewGuid()
            };

            _mockRepo.Setup(r => r.GetAsync(taskId)).ReturnsAsync(existingTask);

            // Act
            var response = await _function.UpdateTask(mockRequest.Object, taskId.ToString(), mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
        }


        [Fact]
        public async Task UpdateTask_WithInvalidGuid_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var json = "{\"title\":\"Updated Task\"}";

            var mockRequest = CreateMockHttpRequestDataWithBody(json);
            var mockContext = CreateMockFunctionContext(userId);

            // Act
            var response = await _function.UpdateTask(mockRequest.Object, "invalid-guid", mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


    }
}
