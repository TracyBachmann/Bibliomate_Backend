using System.Security.Claims;
using System.Runtime.Serialization;
using BackendBiblioMate.Controllers;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="LoansController"/>.
    /// Uses EF Core InMemory database for <see cref="BiblioMateDbContext"/>
    /// and mocks for <see cref="ILoanService"/> and other external dependencies.
    /// Verifies all endpoints: creation, return, retrieval, and deletion of loans.
    /// </summary>
    public class LoansControllerTest
    {
        private readonly Mock<ILoanService> _serviceMock;
        private readonly Mock<ILogger<LoansController>> _loggerMock;
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly BiblioMateDbContext _dbContext;
        private readonly LoansController _controller;

        /// <summary>
        /// Initializes the test class with mocked services and
        /// an in-memory EF Core database context.
        /// </summary>
        public LoansControllerTest()
        {
            _serviceMock = new Mock<ILoanService>();
            _loggerMock  = new Mock<ILogger<LoansController>>();
            _envMock     = new Mock<IWebHostEnvironment>();

            // Configure EF Core InMemory database
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // Instantiate EncryptionService without calling its constructor
            // (encryption not required in these unit tests).
            var encryptionService =
                (EncryptionService)FormatterServices.GetUninitializedObject(typeof(EncryptionService));

            _dbContext = new BiblioMateDbContext(options, encryptionService);

            _controller = new LoansController(
                _serviceMock.Object,
                _loggerMock.Object,
                _envMock.Object,
                _dbContext
            );
        }

        /// <summary>
        /// Ensures that loan creation returns 200 OK when the service succeeds.
        /// </summary>
        [Fact]
        public async Task CreateLoan_ShouldReturnOkWhenSuccess()
        {
            // Arrange
            var dto     = new LoanCreateDto { UserId = 1, BookId = 2 };
            var dueDate = DateTime.UtcNow.AddDays(14);
            var resultOk = Result<LoanCreatedResult, string>.Ok(
                new LoanCreatedResult { DueDate = dueDate });

            _serviceMock
                .Setup(s => s.CreateAsync(It.IsAny<LoanCreateDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultOk);

            // Simulate authenticated user (required to avoid Unauthorized)
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "TestAuth"))
                }
            };

            // Act
            var action = await _controller.CreateLoan(dto, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action);
            dynamic body = ok.Value!;
            Assert.Equal("Loan created successfully.", (string)body.message);
            Assert.Equal(dueDate, (DateTime)body.dueDate);
        }

        /// <summary>
        /// Ensures that loan creation returns 400 BadRequest when the service fails.
        /// </summary>
        [Fact]
        public async Task CreateLoan_ShouldReturnBadRequestWhenError()
        {
            // Arrange
            var dto = new LoanCreateDto { UserId = 1, BookId = 2 };
            var resultFail = Result<LoanCreatedResult, string>.Fail("Invalid loan data");

            _serviceMock
                .Setup(s => s.CreateAsync(It.IsAny<LoanCreateDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultFail);

            // Simulate authenticated user
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "TestAuth"))
                }
            };

            // Act
            var action = await _controller.CreateLoan(dto, CancellationToken.None);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(action);
            dynamic body = bad.Value!;
            Assert.Equal("Invalid loan data", (string)body.error);
        }

        /// <summary>
        /// Ensures that loan return returns 200 OK with fine information when successful.
        /// </summary>
        [Fact]
        public async Task ReturnLoan_ShouldReturnOkWhenSuccess()
        {
            // Arrange
            var loanId = 10;
            var expectedFine = 2.5m;
            var returnedDto = new LoanReturnedResult { ReservationNotified = false, Fine = expectedFine };
            var resultOk = Result<LoanReturnedResult, string>.Ok(returnedDto);

            _serviceMock
                .Setup(s => s.ReturnAsync(loanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultOk);

            // Act
            var action = await _controller.ReturnLoan(loanId, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action);
            dynamic body = ok.Value!;
            Assert.Equal(expectedFine, (decimal)body.fine);
        }

        /// <summary>
        /// Ensures that loan return returns 400 BadRequest when the service fails.
        /// </summary>
        [Fact]
        public async Task ReturnLoan_ShouldReturnBadRequestWhenError()
        {
            // Arrange
            var loanId = 99;
            var resultFail = Result<LoanReturnedResult, string>.Fail("Loan not found");

            _serviceMock
                .Setup(s => s.ReturnAsync(loanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultFail);

            // Act
            var action = await _controller.ReturnLoan(loanId, CancellationToken.None);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(action);
            dynamic body = bad.Value!;
            Assert.Equal("Loan not found", (string)body.error);
        }

        /// <summary>
        /// Ensures that fetching all loans returns 200 OK when the service succeeds.
        /// </summary>
        [Fact]
        public async Task GetAll_ShouldReturnOkWhenSuccess()
        {
            // Arrange
            var loans = new List<Loan> { new Loan(), new Loan() };
            var resultOk = Result<IEnumerable<Loan>, string>.Ok(loans);

            _serviceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultOk);

            // Act
            var action = await _controller.GetAll(CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action);
            Assert.IsAssignableFrom<IEnumerable<LoanReadDto>>(ok.Value);
        }

        /// <summary>
        /// Ensures that fetching all loans returns 400 BadRequest when the service fails.
        /// </summary>
        [Fact]
        public async Task GetAll_ShouldReturnBadRequestWhenError()
        {
            // Arrange
            var resultFail = Result<IEnumerable<Loan>, string>.Fail("Database error");

            _serviceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultFail);

            // Act
            var action = await _controller.GetAll(CancellationToken.None);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(action);
            dynamic body = bad.Value!;
            Assert.Equal("Database error", (string)body.error);
        }

        /// <summary>
        /// Ensures that loan deletion returns 200 OK with a success message when successful.
        /// </summary>
        [Fact]
        public async Task DeleteLoan_ShouldReturnOkWhenSuccess()
        {
            // Arrange
            var resultOk = Result<bool, string>.Ok(true);

            _serviceMock
                .Setup(s => s.DeleteAsync(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultOk);

            // Act
            var action = await _controller.DeleteLoan(3, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action);
            dynamic body = ok.Value!;
            Assert.Equal("Loan deleted successfully.", (string)body.message);
        }

        /// <summary>
        /// Ensures that loan deletion returns 400 BadRequest when the service fails.
        /// </summary>
        [Fact]
        public async Task DeleteLoan_ShouldReturnBadRequestWhenError()
        {
            // Arrange
            var resultFail = Result<bool, string>.Fail("Cannot delete loan");

            _serviceMock
                .Setup(s => s.DeleteAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultFail);

            // Act
            var action = await _controller.DeleteLoan(99, CancellationToken.None);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(action);
            dynamic body = bad.Value!;
            Assert.Equal("Cannot delete loan", (string)body.error);
        }
    }
}

