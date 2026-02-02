using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using TaskApi.Functions.Factories;
using TaskApi.Functions.Functions;
using TaskApi.Functions.Models;
using TaskApi.Functions.Repositories;
using Xunit;

namespace TestTaskApi
{
    
    public partial class TasksFunctionTests
    {
        private readonly Mock<ITaskRepository> _mockRepo;
        private readonly Mock<ITaskFactory> _mockFactory;
        private readonly Mock<ILogger<TasksFunction>> _mockLogger;
        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly TasksFunction _function;

        public TasksFunctionTests()
        {
            _mockRepo = new Mock<ITaskRepository>();
            _mockFactory = new Mock<ITaskFactory>();
            _mockLogger = new Mock<ILogger<TasksFunction>>();
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);

            _function = new TasksFunction(_mockRepo.Object, _mockFactory.Object, _mockLoggerFactory.Object);
        }


        [Fact]
        public async Task GetTasks_WithoutAuthorization_ReturnsUnauthorized()
        {
            // Arrange
            var mockRequest = CreateMockHttpRequestData();
            var mockContext = CreateMockFunctionContextWithoutUser();

            // Act
            var response = await _function.GetTasks(mockRequest.Object, mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            _mockRepo.Verify(r => r.ListAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), 
                It.IsAny<int>(), It.IsAny<Guid?>(), It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()), Times.Never);
        }

        [Fact]
        public async Task GetTasks_WithQueryParameters_PassesCorrectFilters()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var mockRequest = CreateMockHttpRequestData("?status=Pending&assignedTo=user@example.com&sortBy=dueDate&desc=true");
            var mockContext = CreateMockFunctionContext(userId);

            _mockRepo.Setup(r => r.ListAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), 
                It.IsAny<int>(), It.IsAny<Guid?>(), It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(new List<TaskItem>());

            // Act
            var response = await _function.GetTasks(mockRequest.Object, mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _mockRepo.Verify(r => r.ListAsync(
                "Pending", "user@example.com", null, 
                "dueDate", true, 0, 50, userId, 
                It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>()), Times.Once);
        }

  

    }
}
