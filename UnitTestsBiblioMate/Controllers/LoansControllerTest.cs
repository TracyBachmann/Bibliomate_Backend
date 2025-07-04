using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="LoansController"/>.
    /// Verifies behavior of all endpoints with mocked <see cref="ILoanService"/>.
    /// </summary>
    public class LoansControllerTest
    {
        private readonly Mock<ILoanService> _serviceMock;
        private readonly LoansController _controller;

        /// <summary>
        /// Initializes mocks and controller for testing.
        /// </summary>
        public LoansControllerTest()
        {
            _serviceMock = new Mock<ILoanService>();
            _controller = new LoansController(_serviceMock.Object);
        }

        /// <summary>
        /// Ensures CreateLoan returns 200 OK with message and dueDate on success.
        /// </summary>
        [Fact]
        public async Task CreateLoan_ShouldReturnOkWhenSuccess()
        {
            // Arrange
            var dto = new LoanCreateDto { /* fill required props */ };
            var dueDate = DateTime.UtcNow.AddDays(14);
            var resultOk = Result<LoanCreatedResult, string>.Ok(new LoanCreatedResult { DueDate = dueDate });

            _serviceMock
                .Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultOk);

            // Act
            var action = await _controller.CreateLoan(dto, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action);
            dynamic body = ok.Value!;
            Assert.Equal("Loan created successfully.", (string)body.message);
            Assert.Equal(dueDate, (DateTime)body.dueDate);
        }

        /// <summary>
        /// Ensures CreateLoan returns 400 BadRequest with error on failure.
        /// </summary>
        [Fact]
        public async Task CreateLoan_ShouldReturnBadRequestWhenError()
        {
            // Arrange
            var dto = new LoanCreateDto { /* fill required props */ };
            var resultFail = Result<LoanCreatedResult, string>.Fail("Invalid loan data");

            _serviceMock
                .Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultFail);

            // Act
            var action = await _controller.CreateLoan(dto, CancellationToken.None);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(action);
            dynamic body = bad.Value!;
            Assert.Equal("Invalid loan data", (string)body.error);
        }

        /// <summary>
        /// Ensures ReturnLoan returns 200 OK with message and reservationNotified on success.
        /// </summary>
        [Fact]
        public async Task ReturnLoan_ShouldReturnOkWhenSuccess()
        {
            // Arrange
            var loanId = 42;
            var resultOk = Result<LoanReturnedResult, string>.Ok(new LoanReturnedResult { ReservationNotified = true });

            _serviceMock
                .Setup(s => s.ReturnAsync(loanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultOk);

            // Act
            var action = await _controller.ReturnLoan(loanId, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action);
            dynamic body = ok.Value!;
            Assert.Equal("Book returned successfully.", (string)body.message);
            Assert.True((bool)body.reservationNotified);
        }

        /// <summary>
        /// Ensures ReturnLoan returns 400 BadRequest with error on failure.
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
        /// Ensures GetAll returns 200 OK with a list of loans on success.
        /// </summary>
        [Fact]
        public async Task GetAll_ShouldReturnOkWithLoans()
        {
            // Arrange
            var loans = new List<Loan>
            {
                new Loan { /* fill props */ },
                new Loan { /* fill props */ }
            };
            var resultOk = Result<IEnumerable<Loan>, string>.Ok(loans);

            _serviceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultOk);

            // Act
            var action = await _controller.GetAll(CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action);
            Assert.Equal(loans, ok.Value);
        }

        /// <summary>
        /// Ensures GetAll returns 400 BadRequest with error on failure.
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
        /// Ensures GetById returns 200 OK with the loan on success.
        /// </summary>
        [Fact]
        public async Task GetById_ShouldReturnOkWhenFound()
        {
            // Arrange
            var loan = new Loan { /* fill props */ };
            var resultOk = Result<Loan, string>.Ok(loan);

            _serviceMock
                .Setup(s => s.GetByIdAsync(7, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultOk);

            // Act
            var action = await _controller.GetById(7, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action);
            Assert.Equal(loan, ok.Value);
        }

        /// <summary>
        /// Ensures GetById returns 400 BadRequest with error on failure.
        /// </summary>
        [Fact]
        public async Task GetById_ShouldReturnBadRequestWhenError()
        {
            // Arrange
            var resultFail = Result<Loan, string>.Fail("Loan not found");

            _serviceMock
                .Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultFail);

            // Act
            var action = await _controller.GetById(99, CancellationToken.None);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(action);
            dynamic body = bad.Value!;
            Assert.Equal("Loan not found", (string)body.error);
        }

        /// <summary>
        /// Ensures UpdateLoan returns 200 OK with the updated loan on success.
        /// </summary>
        [Fact]
        public async Task UpdateLoan_ShouldReturnOkWhenSuccess()
        {
            // Arrange
            var dto = new LoanUpdateDto { /* fill props */ };
            var updatedLoan = new Loan { /* fill props */ };
            var resultOk = Result<Loan, string>.Ok(updatedLoan);

            _serviceMock
                .Setup(s => s.UpdateAsync(5, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultOk);

            // Act
            var action = await _controller.UpdateLoan(5, dto, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action);
            Assert.Equal(updatedLoan, ok.Value);
        }

        /// <summary>
        /// Ensures UpdateLoan returns 400 BadRequest with error on failure.
        /// </summary>
        [Fact]
        public async Task UpdateLoan_ShouldReturnBadRequestWhenError()
        {
            // Arrange
            var dto = new LoanUpdateDto { /* fill props */ };
            var resultFail = Result<Loan, string>.Fail("Cannot update loan");

            _serviceMock
                .Setup(s => s.UpdateAsync(99, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultFail);

            // Act
            var action = await _controller.UpdateLoan(99, dto, CancellationToken.None);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(action);
            dynamic body = bad.Value!;
            Assert.Equal("Cannot update loan", (string)body.error);
        }

        /// <summary>
        /// Ensures DeleteLoan returns 200 OK with message on success.
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
        /// Ensures DeleteLoan returns 400 BadRequest with error on failure.
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