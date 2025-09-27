using System.Security.Claims;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    public class LoansControllerTest
    {
        private readonly Mock<ILoanService> _serviceMock;
        private readonly Mock<ILogger<LoansController>> _loggerMock;
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly BiblioMateDbContext _dbContext;
        private readonly LoansController _controller;

        public LoansControllerTest()
        {
            _serviceMock = new Mock<ILoanService>();
            _loggerMock  = new Mock<ILogger<LoansController>>();
            _envMock     = new Mock<IWebHostEnvironment>();

            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // EncryptionService via IConfiguration (évite les APIs obsolètes)
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = Convert.ToBase64String(
                        System.Text.Encoding.UTF8.GetBytes("12345678901234567890123456789012"))
                })
                .Build();
            var encryptionService = new EncryptionService(config);

            _dbContext = new BiblioMateDbContext(options, encryptionService);

            _controller = new LoansController(
                _serviceMock.Object,
                _loggerMock.Object,
                _envMock.Object,
                _dbContext
            );
        }

        [Fact]
        public async Task CreateLoan_ShouldReturnOkWhenSuccess()
        {
            var dto     = new LoanCreateDto { UserId = 1, BookId = 2 };
            var dueDate = DateTime.UtcNow.AddDays(14);
            var resultOk = Result<LoanCreatedResult, string>.Ok(new LoanCreatedResult { DueDate = dueDate });

            _serviceMock
                .Setup(s => s.CreateAsync(It.IsAny<LoanCreateDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultOk);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "TestAuth"))
                }
            };

            var action = await _controller.CreateLoan(dto, CancellationToken.None);
            var ok = Assert.IsType<OkObjectResult>(action);

            // Accepte LoanCreatedResult ou objet anonyme { dueDate, message } ou string
            switch (ok.Value)
            {
                case LoanCreatedResult payload:
                    Assert.Equal(dueDate, payload.DueDate);
                    break;
                case not null:
                    // essaie de lire via reflection un champ "dueDate" si c'est un type anonyme
                    var prop = ok.Value.GetType().GetProperty("dueDate")
                               ?? ok.Value.GetType().GetProperty("DueDate");
                    Assert.NotNull(prop);
                    Assert.Equal(dueDate, (DateTime)prop!.GetValue(ok.Value)!);
                    break;
                default:
                    Assert.Fail("Unexpected null OkObjectResult.Value");
                    break;
            }
        }

        [Fact]
        public async Task CreateLoan_ShouldReturnBadRequestWhenError()
        {
            var dto = new LoanCreateDto { UserId = 1, BookId = 2 };
            var resultFail = Result<LoanCreatedResult, string>.Fail("Invalid loan data");

            _serviceMock
                .Setup(s => s.CreateAsync(It.IsAny<LoanCreateDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultFail);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "TestAuth"))
                }
            };

            var action = await _controller.CreateLoan(dto, CancellationToken.None);
            var bad = Assert.IsType<BadRequestObjectResult>(action);

            // Tolère string, ProblemDetails ou objet anonyme { error }
            Assert.True(
                bad.Value is string s && s.Contains("Invalid loan data", StringComparison.OrdinalIgnoreCase)
                || TryGetAnonString(bad.Value, "error", out var err) && err.Contains("Invalid loan data", StringComparison.OrdinalIgnoreCase)
                || bad.Value is ProblemDetails pd && (pd.Detail?.Contains("Invalid loan data", StringComparison.OrdinalIgnoreCase) == true),
                $"Unexpected bad request payload: {bad.Value}"
            );
        }

        [Fact]
        public async Task ReturnLoan_ShouldReturnOkWhenSuccess()
        {
            var loanId = 10;
            var expectedFine = 2.5m;
            var returnedDto = new LoanReturnedResult { ReservationNotified = false, Fine = expectedFine };
            var resultOk = Result<LoanReturnedResult, string>.Ok(returnedDto);

            _serviceMock
                .Setup(s => s.ReturnAsync(loanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultOk);

            var action = await _controller.ReturnLoan(loanId, CancellationToken.None);
            var ok = Assert.IsType<OkObjectResult>(action);

            // Accepte LoanReturnedResult ou objet anonyme { fine }
            if (ok.Value is LoanReturnedResult payload)
            {
                Assert.Equal(expectedFine, payload.Fine);
            }
            else
            {
                Assert.True(
                    TryGetAnon<decimal>(ok.Value, "fine", out var fine) && fine == expectedFine,
                    $"Unexpected ok payload for Return: {ok.Value}"
                );
            }
        }

        [Fact]
        public async Task ReturnLoan_ShouldReturnBadRequestWhenError()
        {
            var loanId = 99;
            var resultFail = Result<LoanReturnedResult, string>.Fail("Loan not found");

            _serviceMock
                .Setup(s => s.ReturnAsync(loanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultFail);

            var action = await _controller.ReturnLoan(loanId, CancellationToken.None);
            var bad = Assert.IsType<BadRequestObjectResult>(action);

            Assert.True(
                bad.Value is string s && s.Contains("Loan not found", StringComparison.OrdinalIgnoreCase)
                || TryGetAnonString(bad.Value, "error", out var err) && err.Contains("Loan not found", StringComparison.OrdinalIgnoreCase)
                || bad.Value is ProblemDetails pd && (pd.Detail?.Contains("Loan not found", StringComparison.OrdinalIgnoreCase) == true),
                $"Unexpected bad request payload: {bad.Value}"
            );
        }

        [Fact]
        public async Task GetAll_ShouldReturnOkWhenSuccess()
        {
            var loans = new List<Loan> { new Loan(), new Loan() };
            var resultOk = Result<IEnumerable<Loan>, string>.Ok(loans);

            _serviceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultOk);

            var action = await _controller.GetAll(CancellationToken.None);
            var ok = Assert.IsType<OkObjectResult>(action);
            Assert.IsAssignableFrom<IEnumerable<LoanReadDto>>(ok.Value);
        }

        [Fact]
        public async Task GetAll_ShouldReturnBadRequestWhenError()
        {
            var resultFail = Result<IEnumerable<Loan>, string>.Fail("Database error");

            _serviceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultFail);

            var action = await _controller.GetAll(CancellationToken.None);
            var bad = Assert.IsType<BadRequestObjectResult>(action);

            Assert.True(
                bad.Value is string s && s.Contains("Database error", StringComparison.OrdinalIgnoreCase)
                || TryGetAnonString(bad.Value, "error", out var err) && err.Contains("Database error", StringComparison.OrdinalIgnoreCase)
                || bad.Value is ProblemDetails pd && (pd.Detail?.Contains("Database error", StringComparison.OrdinalIgnoreCase) == true),
                $"Unexpected bad request payload: {bad.Value}"
            );
        }

        [Fact]
        public async Task DeleteLoan_ShouldReturnOkWhenSuccess()
        {
            var resultOk = Result<bool, string>.Ok(true);

            _serviceMock
                .Setup(s => s.DeleteAsync(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultOk);

            var action = await _controller.DeleteLoan(3, CancellationToken.None);
            var ok = Assert.IsType<OkObjectResult>(action);

            // Le contrôleur peut renvoyer une string, un bool, ou { message = ... }
            Assert.True(
                ok.Value is string s && s.Contains("deleted", StringComparison.OrdinalIgnoreCase)
                || ok.Value is bool b && b
                || TryGetAnonString(ok.Value, "message", out var msg) && msg.Contains("deleted", StringComparison.OrdinalIgnoreCase),
                $"Unexpected ok payload for Delete: {ok.Value}"
            );
        }

        [Fact]
        public async Task DeleteLoan_ShouldReturnBadRequestWhenError()
        {
            var resultFail = Result<bool, string>.Fail("Cannot delete loan");

            _serviceMock
                .Setup(s => s.DeleteAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultFail);

            var action = await _controller.DeleteLoan(99, CancellationToken.None);
            var bad = Assert.IsType<BadRequestObjectResult>(action);

            Assert.True(
                bad.Value is string s && s.Contains("Cannot delete loan", StringComparison.OrdinalIgnoreCase)
                || TryGetAnonString(bad.Value, "error", out var err) && err.Contains("Cannot delete loan", StringComparison.OrdinalIgnoreCase)
                || bad.Value is ProblemDetails pd && (pd.Detail?.Contains("Cannot delete loan", StringComparison.OrdinalIgnoreCase) == true),
                $"Unexpected bad request payload: {bad.Value}"
            );
        }

        // ---------------- helpers ----------------

        private static bool TryGetAnonString(object? value, string propName, out string str)
        {
            str = string.Empty;
            if (value is null) return false;
            var p = value.GetType().GetProperty(propName) ?? value.GetType().GetProperty(ToPascal(propName));
            if (p == null) return false;
            var v = p.GetValue(value) as string;
            if (v == null) return false;
            str = v;
            return true;
        }

        private static bool TryGetAnon<T>(object? value, string propName, out T result)
        {
            result = default!;
            if (value is null) return false;
            var p = value.GetType().GetProperty(propName) ?? value.GetType().GetProperty(ToPascal(propName));
            if (p == null) return false;
            var v = p.GetValue(value);
            if (v is T cast)
            {
                result = cast;
                return true;
            }
            return false;
        }

        private static string ToPascal(string name)
            => string.IsNullOrEmpty(name) ? name : char.ToUpperInvariant(name[0]) + name.Substring(1);
    }
}
