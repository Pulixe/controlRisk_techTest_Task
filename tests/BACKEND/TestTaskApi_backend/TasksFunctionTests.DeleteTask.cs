using Moq;
using System.Net;
using TaskApi.Functions.Models;
using Xunit;

namespace TestTaskApi
{
    /// <summary>
    /// Unit tests for TasksFunction.DeleteTask endpoint
    /// </summary>
    public partial class TasksFunctionTests
    {
        [Fact]
        public async Task DeleteTask_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            var mockRequest = CreateMockHttpRequestData();
            var mockContext = CreateMockFunctionContext(userId);

            var existingTask = new TaskItem 
            { 
                Id = taskId, 
                Title = "Task to Delete", 
                UserId = userId
            };

            _mockRepo.Setup(r => r.GetAsync(taskId)).ReturnsAsync(existingTask);
            _mockRepo.Setup(r => r.DeleteAsync(taskId)).Returns(Task.CompletedTask);

            // Act
            var response = await _function.DeleteTask(mockRequest.Object, taskId.ToString(), mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            _mockRepo.Verify(r => r.DeleteAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task DeleteTask_WithNonExistentId_ReturnsNoContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            var mockRequest = CreateMockHttpRequestData();
            var mockContext = CreateMockFunctionContext(userId);

            _mockRepo.Setup(r => r.GetAsync(taskId)).ReturnsAsync((TaskItem?)null);

            // Act
            var response = await _function.DeleteTask(mockRequest.Object, taskId.ToString(), mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteTask_WithoutAuthorization_ReturnsUnauthorized()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            var mockRequest = CreateMockHttpRequestData();
            var mockContext = CreateMockFunctionContextWithoutUser();

            var existingTask = new TaskItem 
            { 
                Id = taskId, 
                Title = "Task to Delete", 
                UserId = Guid.NewGuid()
            };

            _mockRepo.Setup(r => r.GetAsync(taskId)).ReturnsAsync(existingTask);

            // Act
            var response = await _function.DeleteTask(mockRequest.Object, taskId.ToString(), mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteTask_WithDifferentUser_ReturnsForbidden()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            var mockRequest = CreateMockHttpRequestData();
            var mockContext = CreateMockFunctionContext(userId);

            var existingTask = new TaskItem 
            { 
                Id = taskId, 
                Title = "Task to Delete", 
                UserId = differentUserId
            };

            _mockRepo.Setup(r => r.GetAsync(taskId)).ReturnsAsync(existingTask);

            // Act
            var response = await _function.DeleteTask(mockRequest.Object, taskId.ToString(), mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteTask_WithInvalidGuid_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var mockRequest = CreateMockHttpRequestData();
            var mockContext = CreateMockFunctionContext(userId);

            // Act
            var response = await _function.DeleteTask(mockRequest.Object, "invalid-guid", mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteTask_WhenExceptionOccurs_ReturnsInternalServerError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            var mockRequest = CreateMockHttpRequestData();
            var mockContext = CreateMockFunctionContext(userId);

            var existingTask = new TaskItem 
            { 
                Id = taskId, 
                Title = "Task to Delete", 
                UserId = userId
            };

            _mockRepo.Setup(r => r.GetAsync(taskId)).ReturnsAsync(existingTask);
            _mockRepo.Setup(r => r.DeleteAsync(taskId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var response = await _function.DeleteTask(mockRequest.Object, taskId.ToString(), mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }
    }
}
