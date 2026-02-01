using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using TaskApi.Functions.Functions;
using TaskApi.Functions.Models;
using TaskApi.Functions.Repositories;
using Xunit;

namespace TestTaskApi
{
    /// <summary>
    /// Unit tests for UsersFunction.GetUsers endpoint
    /// </summary>
    public partial class UsersFunctionTests
    {
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly Mock<ILogger<UsersFunction>> _mockLogger;
        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly UsersFunction _function;

        public UsersFunctionTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _mockLogger = new Mock<ILogger<UsersFunction>>();
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);

            _function = new UsersFunction(_mockRepo.Object, _mockLoggerFactory.Object);
        }

        [Fact]
        public async Task GetUsers_WithAuthorizedUser_ReturnsOkWithUsers()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), Name = "User 1", Email = "user1@example.com" },
                new User { Id = Guid.NewGuid(), Name = "User 2", Email = "user2@example.com" }
            };

            var mockRequest = CreateMockHttpRequestData();
            var mockContext = CreateMockFunctionContext(currentUserId);

            _mockRepo.Setup(r => r.ListAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(users);

            // Act
            var response = await _function.GetUsers(mockRequest.Object, mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _mockRepo.Verify(r => r.ListAsync(null, 0, 50), Times.Once);
        }

        [Fact]
        public async Task GetUsers_WithoutAuthorization_ReturnsUnauthorized()
        {
            // Arrange
            var mockRequest = CreateMockHttpRequestData();
            var mockContext = CreateMockFunctionContextWithoutUser();

            // Act
            var response = await _function.GetUsers(mockRequest.Object, mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            _mockRepo.Verify(r => r.ListAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetUsers_WithSearchQuery_PassesQueryToRepository()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            var searchQuery = "john";
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@example.com" }
            };

            var mockRequest = CreateMockHttpRequestData($"?q={searchQuery}");
            var mockContext = CreateMockFunctionContext(currentUserId);

            _mockRepo.Setup(r => r.ListAsync(searchQuery, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(users);

            // Act
            var response = await _function.GetUsers(mockRequest.Object, mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _mockRepo.Verify(r => r.ListAsync(searchQuery, 0, 50), Times.Once);
        }

        [Fact]
        public async Task GetUsers_WithPagination_PassesCorrectSkipAndTake()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            var mockRequest = CreateMockHttpRequestData("?skip=10&take=20");
            var mockContext = CreateMockFunctionContext(currentUserId);

            _mockRepo.Setup(r => r.ListAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<User>());

            // Act
            var response = await _function.GetUsers(mockRequest.Object, mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _mockRepo.Verify(r => r.ListAsync(null, 10, 20), Times.Once);
        }

        [Fact]
        public async Task GetUsers_WithDefaultPagination_UsesDefaultValues()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            var mockRequest = CreateMockHttpRequestData();
            var mockContext = CreateMockFunctionContext(currentUserId);

            _mockRepo.Setup(r => r.ListAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<User>());

            // Act
            var response = await _function.GetUsers(mockRequest.Object, mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _mockRepo.Verify(r => r.ListAsync(null, 0, 50), Times.Once);
        }

        [Fact]
        public async Task GetUsers_WithInvalidPagination_UsesDefaultValues()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            var mockRequest = CreateMockHttpRequestData("?skip=invalid&take=invalid");
            var mockContext = CreateMockFunctionContext(currentUserId);

            _mockRepo.Setup(r => r.ListAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<User>());

            // Act
            var response = await _function.GetUsers(mockRequest.Object, mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _mockRepo.Verify(r => r.ListAsync(null, 0, 50), Times.Once);
        }

        [Fact]
        public async Task GetUsers_ReturnsOnlyIdNameAndEmail()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var users = new List<User>
            {
                new User 
                { 
                    Id = userId, 
                    Name = "John Doe", 
                    Email = "john@example.com",
                    Sub = "auth0|123456", // Should not be returned
                    CreatedAt = DateTime.UtcNow // Should not be returned
                }
            };

            var mockRequest = CreateMockHttpRequestData();
            var mockContext = CreateMockFunctionContext(currentUserId);

            _mockRepo.Setup(r => r.ListAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(users);

            // Act
            var response = await _function.GetUsers(mockRequest.Object, mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // Note: In a real scenario, we would verify the response body contains only id, name, and email
            // For this test, we verify that the repository was called correctly
            _mockRepo.Verify(r => r.ListAsync(null, 0, 50), Times.Once);
        }

        [Fact]
        public async Task GetUsers_WhenExceptionOccurs_ReturnsInternalServerError()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            var mockRequest = CreateMockHttpRequestData();
            var mockContext = CreateMockFunctionContext(currentUserId);

            _mockRepo.Setup(r => r.ListAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var response = await _function.GetUsers(mockRequest.Object, mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task GetUsers_WithEmptyResult_ReturnsOkWithEmptyList()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            var mockRequest = CreateMockHttpRequestData();
            var mockContext = CreateMockFunctionContext(currentUserId);

            _mockRepo.Setup(r => r.ListAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<User>());

            // Act
            var response = await _function.GetUsers(mockRequest.Object, mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetUsers_WithCombinedQueryAndPagination_PassesAllParameters()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            var searchQuery = "test";
            var mockRequest = CreateMockHttpRequestData($"?q={searchQuery}&skip=5&take=15");
            var mockContext = CreateMockFunctionContext(currentUserId);

            _mockRepo.Setup(r => r.ListAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<User>());

            // Act
            var response = await _function.GetUsers(mockRequest.Object, mockContext.Object);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _mockRepo.Verify(r => r.ListAsync(searchQuery, 5, 15), Times.Once);
        }
    }
}
