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

        public LoansControllerTest()
        {
            _serviceMock = new Mock<ILoanService>();
            _controller = new LoansController(_serviceMock.Object);
        }

        [Fact]
        public async Task CreateLoan_ShouldReturnOkWhenSuccess()
        {
            // Arrange
            var dto = new LoanCreateDto { UserId = 1, BookId = 2 };
            var dueDate = DateTime.UtcNow.AddDays(14);
            var resultOk = Result<LoanCreatedResult, string>
                .Ok(new LoanCreatedResult { DueDate = dueDate });

            _serviceMock
                .Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultOk);

            // Act
            var action = await _controller.CreateLoan(dto, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action);
            dynamic body = ok.Value!;
            Assert.Equal("Loan created successfully.", (string)body.message);
            Assert.Equal(dueDate,   (DateTime)body.dueDate);
        }

        [Fact]
        public async Task CreateLoan_ShouldReturnBadRequestWhenError()
        {
            // Arrange
            var dto = new LoanCreateDto { UserId = 1, BookId = 2 };
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

        [Fact]
        public async Task ReturnLoan_ShouldReturnFineInResponse()
        {
            // Arrange
            var loanId = 10;
            var expectedFine = 2.5m;
            var returnedDto = new BackendBiblioMate.DTOs.LoanReturnedResult
            {
                ReservationNotified = false,
                Fine = expectedFine
            };
            var resultOk = Result<BackendBiblioMate.DTOs.LoanReturnedResult, string>
                .Ok(returnedDto);

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

        [Fact]
        public async Task ReturnLoan_ShouldReturnBadRequestWhenError()
        {
            // Arrange
            var loanId = 99;
            var resultFail = Result<BackendBiblioMate.DTOs.LoanReturnedResult, string>
                .Fail("Loan not found");

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

        [Fact]
        public async Task GetAll_ShouldReturnOkWithLoans()
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
            Assert.Equal(loans, ok.Value);
        }

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

        [Fact]
        public async Task GetById_ShouldReturnOkWhenFound()
        {
            // Arrange
            var loan = new Loan { LoanId = 7 };
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

        [Fact]
        public async Task UpdateLoan_ShouldReturnOkWhenSuccess()
        {
            // Arrange
            var dto = new LoanUpdateDto { DueDate = DateTime.UtcNow };
            var updatedLoan = new Loan { LoanId = 5 };
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

        [Fact]
        public async Task UpdateLoan_ShouldReturnBadRequestWhenError()
        {
            // Arrange
            var dto = new LoanUpdateDto { DueDate = DateTime.UtcNow };
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