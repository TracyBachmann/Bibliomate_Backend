using System.Security.Claims;
using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using BackendBiblioMate.Models.Enums;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="ReportsController"/>.
    /// Verifies authorization, CRUD behavior, and ownership rules.
    /// </summary>
    public class ReportsControllerTest
    {
        private readonly Mock<IReportService> _serviceMock;
        private readonly ReportsController    _controller;

        public ReportsControllerTest()
        {
            _serviceMock = new Mock<IReportService>();
            _controller  = new ReportsController(_serviceMock.Object);
        }

        /// <summary>
        /// Sets up HttpContext.User with specified user ID and optional role.
        /// </summary>
        private void SetUser(int userId, string? role = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            if (role != null)
                claims.Add(new Claim(ClaimTypes.Role, role));
            var identity = new ClaimsIdentity(claims, "test");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        /// <summary>
        /// Admin or Librarian fetching all reports should return 200 OK with the list.
        /// </summary>
        [Theory]
        [InlineData(UserRoles.Admin)]
        [InlineData(UserRoles.Librarian)]
        public async Task GetReports_AsPrivileged_ReturnsOk(string role)
        {
            // Arrange
            SetUser(1, role);
            var list = new List<ReportReadDto>
            {
                new ReportReadDto { ReportId = 10, UserId = 1, Title = "R1" },
                new ReportReadDto { ReportId = 20, UserId = 2, Title = "R2" }
            };
            _serviceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            // Act
            var action = await _controller.GetReports(CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// A non-privileged user fetching all reports should still return 200 OK.
        /// </summary>
        [Fact]
        public async Task GetReports_AsUser_ReturnsOk()
        {
            // Arrange
            SetUser(3, UserRoles.User);
            var list = new List<ReportReadDto>();
            _serviceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            // Act
            var action = await _controller.GetReports(CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// Retrieving a missing report should yield 404 NotFound.
        /// </summary>
        [Fact]
        public async Task GetReport_NotFound_Returns404()
        {
            // Arrange
            SetUser(5, UserRoles.User);
            _serviceMock
                .Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReportReadDto?)null);

            // Act
            var action = await _controller.GetReport(99, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action.Result);
        }

        /// <summary>
        /// Owner retrieving own report should get 200 OK with the DTO.
        /// </summary>
        [Fact]
        public async Task GetReport_Owner_ReturnsOk()
        {
            // Arrange
            var dto = new ReportReadDto { ReportId = 7, UserId = 7, Title = "My Report" };
            SetUser(7, UserRoles.User);
            _serviceMock
                .Setup(s => s.GetByIdAsync(7, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // Act
            var action = await _controller.GetReport(7, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Non-owner non-admin retrieving another user's report should get 403 Forbidden.
        /// </summary>
        [Fact]
        public async Task GetReport_NonOwnerNonAdmin_ReturnsForbid()
        {
            // Arrange
            var dto = new ReportReadDto { ReportId = 8, UserId = 2, Title = "Other" };
            SetUser(1, UserRoles.User);
            _serviceMock
                .Setup(s => s.GetByIdAsync(8, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // Act
            var action = await _controller.GetReport(8, CancellationToken.None);

            // Assert
            Assert.IsType<ForbidResult>(action.Result);
        }

        /// <summary>
        /// An admin retrieving any report should get 200 OK with the DTO.
        /// </summary>
        [Fact]
        public async Task GetReport_Admin_ReturnsOk()
        {
            // Arrange
            var dto = new ReportReadDto { ReportId = 9, UserId = 2, Title = "Other" };
            SetUser(1, UserRoles.Admin);
            _serviceMock
                .Setup(s => s.GetByIdAsync(9, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // Act
            var action = await _controller.GetReport(9, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Creating a report should return 201 Created with a location header and the DTO.
        /// </summary>
        [Fact]
        public async Task CreateReport_ReturnsCreated()
        {
            // Arrange
            var createDto = new ReportCreateDto { Title = "New" };
            var created   = new ReportReadDto { ReportId = 11, UserId = 3, Title = "New" };
            SetUser(3, UserRoles.User);
            _serviceMock
                .Setup(s => s.CreateAsync(createDto, 3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

            // Act
            var action = await _controller.CreateReport(createDto, CancellationToken.None);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(action.Result);
            Assert.Equal(nameof(ReportsController.GetReport), createdAt.ActionName);
            Assert.Equal(created.ReportId, createdAt.RouteValues!["id"]);
            Assert.Equal(created, createdAt.Value);
        }

        /// <summary>
        /// Updating with mismatched ID should yield 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task UpdateReport_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            SetUser(4, UserRoles.User);
            var dto = new ReportUpdateDto { ReportId = 5, Title = "X" };

            // Act
            var action = await _controller.UpdateReport(9, dto, CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestResult>(action);
        }

        /// <summary>
        /// Updating a non-existent report should yield 404 NotFound.
        /// </summary>
        [Fact]
        public async Task UpdateReport_NotFound_Returns404()
        {
            // Arrange
            SetUser(4, UserRoles.User);
            _serviceMock
                .Setup(s => s.GetByIdAsync(12, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReportReadDto?)null);
            var dto = new ReportUpdateDto { ReportId = 12, Title = "X" };

            // Act
            var action = await _controller.UpdateReport(12, dto, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Non-owner non-admin updating another user's report should yield 403 Forbidden.
        /// </summary>
        [Fact]
        public async Task UpdateReport_NonOwnerNonAdmin_ReturnsForbid()
        {
            // Arrange
            var existing = new ReportReadDto { ReportId = 13, UserId = 2, Title = "Other" };
            SetUser(1, UserRoles.User);
            _serviceMock
                .Setup(s => s.GetByIdAsync(13, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);
            var dto = new ReportUpdateDto { ReportId = 13, Title = "X" };

            // Act
            var action = await _controller.UpdateReport(13, dto, CancellationToken.None);

            // Assert
            Assert.IsType<ForbidResult>(action);
        }

        /// <summary>
        /// Owner updating their report successfully should yield 204 NoContent.
        /// </summary>
        [Fact]
        public async Task UpdateReport_OwnerSuccess_ReturnsNoContent()
        {
            // Arrange
            var existing = new ReportReadDto { ReportId = 14, UserId = 7, Title = "Mine" };
            SetUser(7, UserRoles.User);
            _serviceMock
                .Setup(s => s.GetByIdAsync(14, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);
            _serviceMock
                .Setup(s => s.UpdateAsync(It.Is<ReportUpdateDto>(d => d.ReportId == 14), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            var dto = new ReportUpdateDto { ReportId = 14, Title = "Updated" };

            // Act
            var action = await _controller.UpdateReport(14, dto, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(action);
        }

        /// <summary>
        /// Owner updating but service returns false should yield 404 NotFound.
        /// </summary>
        [Fact]
        public async Task UpdateReport_OwnerFailure_ReturnsNotFound()
        {
            // Arrange
            var existing = new ReportReadDto { ReportId = 15, UserId = 7, Title = "Mine" };
            SetUser(7, UserRoles.User);
            _serviceMock
                .Setup(s => s.GetByIdAsync(15, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);
            _serviceMock
                .Setup(s => s.UpdateAsync(It.IsAny<ReportUpdateDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            var dto = new ReportUpdateDto { ReportId = 15, Title = "Nope" };

            // Act
            var action = await _controller.UpdateReport(15, dto, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Non-owner non-admin deleting another user's report should yield 403 Forbidden.
        /// </summary>
        [Fact]
        public async Task DeleteReport_NonOwnerNonAdmin_ReturnsForbid()
        {
            // Arrange
            var existing = new ReportReadDto { ReportId = 21, UserId = 5, Title = "Other" };
            SetUser(1, UserRoles.User);
            _serviceMock
                .Setup(s => s.GetByIdAsync(21, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            // Act
            var action = await _controller.DeleteReport(21, CancellationToken.None);

            // Assert
            Assert.IsType<ForbidResult>(action);
        }

        /// <summary>
        /// Owner deleting their own report successfully should yield 204 NoContent.
        /// </summary>
        [Fact]
        public async Task DeleteReport_OwnerSuccess_ReturnsNoContent()
        {
            // Arrange
            var existing = new ReportReadDto { ReportId = 22, UserId = 8, Title = "Mine" };
            SetUser(8, UserRoles.User);
            _serviceMock
                .Setup(s => s.GetByIdAsync(22, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);
            _serviceMock
                .Setup(s => s.DeleteAsync(22, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var action = await _controller.DeleteReport(22, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(action);
        }

        /// <summary>
        /// Owner deleting but service returns false should yield 404 NotFound.
        /// </summary>
        [Fact]
        public async Task DeleteReport_OwnerFailure_ReturnsNotFound()
        {
            // Arrange
            var existing = new ReportReadDto { ReportId = 23, UserId = 9, Title = "Mine" };
            SetUser(9, UserRoles.User);
            _serviceMock
                .Setup(s => s.GetByIdAsync(23, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);
            _serviceMock
                .Setup(s => s.DeleteAsync(23, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var action = await _controller.DeleteReport(23, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action);
        }
    }
}