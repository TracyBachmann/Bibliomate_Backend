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
    /// Verifies authorization rules, CRUD behavior, and ownership logic
    /// across all controller actions.
    /// </summary>
    public class ReportsControllerTest
    {
        private readonly Mock<IReportService> _serviceMock;
        private readonly ReportsController    _controller;

        /// <summary>
        /// Initializes the test class with a mocked <see cref="IReportService"/>
        /// and a new instance of <see cref="ReportsController"/>.
        /// </summary>
        public ReportsControllerTest()
        {
            _serviceMock = new Mock<IReportService>();
            _controller  = new ReportsController(_serviceMock.Object);
        }

        /// <summary>
        /// Configures the controller's user identity and optional role
        /// by injecting a <see cref="ClaimsPrincipal"/> into the context.
        /// </summary>
        /// <param name="userId">The numeric user ID for the test identity.</param>
        /// <param name="role">Optional role (e.g., <see cref="UserRoles.Admin"/>).</param>
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
        /// Ensures that privileged roles (Admin, Librarian) can access all reports
        /// and receive a 200 OK response with the expected list.
        /// </summary>
        [Theory]
        [InlineData(UserRoles.Admin)]
        [InlineData(UserRoles.Librarian)]
        public async Task GetReports_AsPrivileged_ReturnsOk(string role)
        {
            SetUser(1, role);
            var list = new List<ReportReadDto>
            {
                new ReportReadDto { ReportId = 10, UserId = 1, Title = "R1" },
                new ReportReadDto { ReportId = 20, UserId = 2, Title = "R2" }
            };
            _serviceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var action = await _controller.GetReports(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// Ensures that a normal user can call GetReports and still receive 200 OK,
        /// even if the service returns an empty list.
        /// </summary>
        [Fact]
        public async Task GetReports_AsUser_ReturnsOk()
        {
            SetUser(3, UserRoles.User);
            var list = new List<ReportReadDto>();
            _serviceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var action = await _controller.GetReports(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// Verifies that requesting a report that does not exist
        /// results in a 404 NotFound response.
        /// </summary>
        [Fact]
        public async Task GetReport_NotFound_Returns404()
        {
            SetUser(5, UserRoles.User);
            _serviceMock
                .Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReportReadDto?)null);

            var action = await _controller.GetReport(99, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action.Result);
        }

        /// <summary>
        /// Ensures that the owner of a report can successfully retrieve it
        /// with a 200 OK response.
        /// </summary>
        [Fact]
        public async Task GetReport_Owner_ReturnsOk()
        {
            var dto = new ReportReadDto { ReportId = 7, UserId = 7, Title = "My Report" };
            SetUser(7, UserRoles.User);
            _serviceMock
                .Setup(s => s.GetByIdAsync(7, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var action = await _controller.GetReport(7, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Ensures that a user who is not the owner and not an admin
        /// is forbidden from accessing another user's report.
        /// </summary>
        [Fact]
        public async Task GetReport_NonOwnerNonAdmin_ReturnsForbid()
        {
            var dto = new ReportReadDto { ReportId = 8, UserId = 2, Title = "Other" };
            SetUser(1, UserRoles.User);
            _serviceMock
                .Setup(s => s.GetByIdAsync(8, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var action = await _controller.GetReport(8, CancellationToken.None);

            Assert.IsType<ForbidResult>(action.Result);
        }

        /// <summary>
        /// Verifies that an Admin can access another user's report
        /// and receives a 200 OK response with the DTO.
        /// </summary>
        [Fact]
        public async Task GetReport_Admin_ReturnsOk()
        {
            var dto = new ReportReadDto { ReportId = 9, UserId = 2, Title = "Other" };
            SetUser(1, UserRoles.Admin);
            _serviceMock
                .Setup(s => s.GetByIdAsync(9, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var action = await _controller.GetReport(9, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Ensures that creating a report returns a 201 CreatedAtAction
        /// with the correct route and DTO.
        /// </summary>
        [Fact]
        public async Task CreateReport_ReturnsCreated()
        {
            var createDto = new ReportCreateDto { Title = "New" };
            var created   = new ReportReadDto { ReportId = 11, UserId = 3, Title = "New" };
            SetUser(3, UserRoles.User);
            _serviceMock
                .Setup(s => s.CreateAsync(createDto, 3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

            var action = await _controller.CreateReport(createDto, CancellationToken.None);

            var createdAt = Assert.IsType<CreatedAtActionResult>(action.Result);
            Assert.Equal(nameof(ReportsController.GetReport), createdAt.ActionName);
            Assert.Equal(created.ReportId, createdAt.RouteValues!["id"]);
            Assert.Equal(created, createdAt.Value);
        }

        /// <summary>
        /// Verifies that when the route ID does not match the DTO ID,
        /// the controller responds with 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task UpdateReport_IdMismatch_ReturnsBadRequest()
        {
            SetUser(4, UserRoles.User);
            var dto = new ReportUpdateDto { ReportId = 5, Title = "X" };

            var action = await _controller.UpdateReport(9, dto, CancellationToken.None);

            Assert.IsType<BadRequestResult>(action);
        }

        /// <summary>
        /// Verifies that updating a non-existent report returns 404 NotFound.
        /// </summary>
        [Fact]
        public async Task UpdateReport_NotFound_Returns404()
        {
            SetUser(4, UserRoles.User);
            _serviceMock
                .Setup(s => s.GetByIdAsync(12, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReportReadDto?)null);
            var dto = new ReportUpdateDto { ReportId = 12, Title = "X" };

            var action = await _controller.UpdateReport(12, dto, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Ensures that a non-owner, non-admin user attempting to update another user's report
        /// receives a 403 Forbid response.
        /// </summary>
        [Fact]
        public async Task UpdateReport_NonOwnerNonAdmin_ReturnsForbid()
        {
            var existing = new ReportReadDto { ReportId = 13, UserId = 2, Title = "Other" };
            SetUser(1, UserRoles.User);
            _serviceMock
                .Setup(s => s.GetByIdAsync(13, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);
            var dto = new ReportUpdateDto { ReportId = 13, Title = "X" };

            var action = await _controller.UpdateReport(13, dto, CancellationToken.None);

            Assert.IsType<ForbidResult>(action);
        }

        /// <summary>
        /// Ensures that an owner can successfully update their own report,
        /// resulting in a 204 NoContent response.
        /// </summary>
        [Fact]
        public async Task UpdateReport_OwnerSuccess_ReturnsNoContent()
        {
            var existing = new ReportReadDto { ReportId = 14, UserId = 7, Title = "Mine" };
            SetUser(7, UserRoles.User);
            _serviceMock
                .Setup(s => s.GetByIdAsync(14, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);
            _serviceMock
                .Setup(s => s.UpdateAsync(It.Is<ReportUpdateDto>(d => d.ReportId == 14), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            var dto = new ReportUpdateDto { ReportId = 14, Title = "Updated" };

            var action = await _controller.UpdateReport(14, dto, CancellationToken.None);

            Assert.IsType<NoContentResult>(action);
        }

        /// <summary>
        /// Ensures that an owner's update attempt that fails in the service layer
        /// results in a 404 NotFound response.
        /// </summary>
        [Fact]
        public async Task UpdateReport_OwnerFailure_ReturnsNotFound()
        {
            var existing = new ReportReadDto { ReportId = 15, UserId = 7, Title = "Mine" };
            SetUser(7, UserRoles.User);
            _serviceMock
                .Setup(s => s.GetByIdAsync(15, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);
            _serviceMock
                .Setup(s => s.UpdateAsync(It.IsAny<ReportUpdateDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            var dto = new ReportUpdateDto { ReportId = 15, Title = "Nope" };

            var action = await _controller.UpdateReport(15, dto, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Ensures that a non-owner, non-admin attempting to delete a report
        /// receives a 403 Forbid response.
        /// </summary>
        [Fact]
        public async Task DeleteReport_NonOwnerNonAdmin_ReturnsForbid()
        {
            var existing = new ReportReadDto { ReportId = 21, UserId = 5, Title = "Other" };
            SetUser(1, UserRoles.User);
            _serviceMock
                .Setup(s => s.GetByIdAsync(21, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            var action = await _controller.DeleteReport(21, CancellationToken.None);

            Assert.IsType<ForbidResult>(action);
        }

        /// <summary>
        /// Ensures that an owner can successfully delete their report,
        /// resulting in a 204 NoContent response.
        /// </summary>
        [Fact]
        public async Task DeleteReport_OwnerSuccess_ReturnsNoContent()
        {
            var existing = new ReportReadDto { ReportId = 22, UserId = 8, Title = "Mine" };
            SetUser(8, UserRoles.User);
            _serviceMock
                .Setup(s => s.GetByIdAsync(22, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);
            _serviceMock
                .Setup(s => s.DeleteAsync(22, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var action = await _controller.DeleteReport(22, CancellationToken.None);

            Assert.IsType<NoContentResult>(action);
        }

        /// <summary>
        /// Ensures that if the delete operation fails for an owner,
        /// the controller responds with 404 NotFound.
        /// </summary>
        [Fact]
        public async Task DeleteReport_OwnerFailure_ReturnsNotFound()
        {
            var existing = new ReportReadDto { ReportId = 23, UserId = 9, Title = "Mine" };
            SetUser(9, UserRoles.User);
            _serviceMock
                .Setup(s => s.GetByIdAsync(23, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);
            _serviceMock
                .Setup(s => s.DeleteAsync(23, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var action = await _controller.DeleteReport(23, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action);
        }
    }
}