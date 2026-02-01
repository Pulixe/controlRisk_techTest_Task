using Moq;
using System.Net;
using TaskApi.Functions.Models;
using Xunit;

namespace TestTaskApi
{
    /// <summary>
    /// Unit tests for TasksFunction.CreateTask endpoint
    /// </summary>
    public partial class TasksFunctionTests
    {
        [Fact]
        public async Task CreateTask_WithValidData_ReturnsCreated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var json = "{\"title\":\"New Task\",\"description\":\"Task description\",\"dueDate\":\"2026-12-31\",\"assignedTo\":\"user@example.com\"}";

            var mockRequest = CreateMockHttpRequestDataWithBody(json);
            var mockContext = CreateMockFunctionContext(userId, "test@example.com", "Test User");

            var createdTask = new TaskItem 
            { 
                Id = taskId, 
                Title = "New Task", 
                Description = "Task description",
                UserId = userId 
            };

            _mockFactory.Setup(f => f.Create(
                "New Task", "Task description", It.IsAny<DateTime?>(), 
                It.IsAny<string>(), "user@example.com"))
                .Returns(createdTask);

            _mockRepo.Setup(r => r.CreateAsync(It.IsAny<TaskItem>())).Returns(Task.CompletedTask);

            // Act
            var response = await _function.CreateTask(mockRequest.Object, mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            _mockFactory.Verify(f => f.Create(
                "New Task", "Task description", It.IsAny<DateTime?>(), 
                "test@example.com", "user@example.com"), Times.Once);
            _mockRepo.Verify(r => r.CreateAsync(It.Is<TaskItem>(t => t.UserId == userId)), Times.Once);
        }

        [Fact]
        public async Task CreateTask_WithoutAuthorization_ReturnsUnauthorized()
        {
            // Arrange
            var json = "{\"title\":\"New Task\"}";
            var mockRequest = CreateMockHttpRequestDataWithBody(json);
            var mockContext = CreateMockFunctionContextWithoutUser();

            // Act
            var response = await _function.CreateTask(mockRequest.Object, mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            _mockRepo.Verify(r => r.CreateAsync(It.IsAny<TaskItem>()), Times.Never);
        }

        [Fact]
        public async Task CreateTask_WithEmptyTitle_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var json = "{\"title\":\"\"}";

            var mockRequest = CreateMockHttpRequestDataWithBody(json);
            var mockContext = CreateMockFunctionContext(userId);

            // Act
            var response = await _function.CreateTask(mockRequest.Object, mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.Verify(r => r.CreateAsync(It.IsAny<TaskItem>()), Times.Never);
        }

        [Fact]
        public async Task CreateTask_WithNullPayload_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var json = "{}";

            var mockRequest = CreateMockHttpRequestDataWithBody(json);
            var mockContext = CreateMockFunctionContext(userId);

            // Act
            var response = await _function.CreateTask(mockRequest.Object, mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateTask_WhenExceptionOccurs_ReturnsInternalServerError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var json = "{\"title\":\"New Task\"}";

            var mockRequest = CreateMockHttpRequestDataWithBody(json);
            var mockContext = CreateMockFunctionContext(userId);

            var createdTask = new TaskItem { Title = "New Task" };
            _mockFactory.Setup(f => f.Create(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), 
                It.IsAny<string>(), It.IsAny<string>()))
                .Returns(createdTask);

            _mockRepo.Setup(r => r.CreateAsync(It.IsAny<TaskItem>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var response = await _function.CreateTask(mockRequest.Object, mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }
    }
}
