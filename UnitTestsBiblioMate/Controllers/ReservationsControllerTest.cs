using System.Security.Claims;
using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="ReservationsController"/>.
    /// Verifies authorization rules, CRUD behavior,
    /// user-scoped access restrictions, and the cleanup endpoint.
    /// </summary>
    public class ReservationsControllerTest
    {
        private readonly Mock<IReservationService>        _svcMock;
        private readonly Mock<IReservationCleanupService> _cleanupMock;
        private readonly ReservationsController           _controller;

        /// <summary>
        /// Initializes the test class with mocked services
        /// and a new instance of <see cref="ReservationsController"/>.
        /// </summary>
        public ReservationsControllerTest()
        {
            _svcMock     = new Mock<IReservationService>();
            _cleanupMock = new Mock<IReservationCleanupService>();
            _controller  = new ReservationsController(_svcMock.Object, _cleanupMock.Object);
        }

        /// <summary>
        /// Helper to configure <see cref="HttpContext.User"/> with the given user ID and roles.
        /// </summary>
        private void SetUser(int userId, params string[] roles)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            foreach (var r in roles)
                claims.Add(new Claim(ClaimTypes.Role, r));

            var identity = new ClaimsIdentity(claims, "test");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        /// <summary>
        /// Admin or librarian fetching all reservations should return 200 OK with the complete list.
        /// </summary>
        [Theory]
        [InlineData(UserRoles.Admin)]
        [InlineData(UserRoles.Librarian)]
        public async Task GetReservations_AsPrivileged_ReturnsOk(string role)
        {
            SetUser(1, role);
            var list = new List<ReservationReadDto>
            {
                new ReservationReadDto { ReservationId = 5, UserId = 2, BookId = 10 },
                new ReservationReadDto { ReservationId = 6, UserId = 3, BookId = 11 }
            };
            _svcMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(list);

            var action = await _controller.GetReservations(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// A normal user requesting another user’s reservations should get 403 Forbidden.
        /// </summary>
        [Fact]
        public async Task GetUserReservations_OtherUser_ReturnsForbid()
        {
            SetUser(2, UserRoles.User);

            var action = await _controller.GetUserReservations(3, CancellationToken.None);

            Assert.IsType<ForbidResult>(action.Result);
        }

        /// <summary>
        /// A user requesting their own reservations should get 200 OK with the expected list.
        /// </summary>
        [Fact]
        public async Task GetUserReservations_Self_ReturnsOk()
        {
            const int me = 4;
            SetUser(me, UserRoles.User);
            var list = new List<ReservationReadDto>
            {
                new ReservationReadDto { ReservationId = 7, UserId = me, BookId = 12 }
            };
            _svcMock.Setup(s => s.GetByUserAsync(me, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(list);

            var action = await _controller.GetUserReservations(me, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// Librarian fetching pending reservations for a specific book should return 200 OK.
        /// </summary>
        [Fact]
        public async Task GetPendingForBook_AsPrivileged_ReturnsOk()
        {
            SetUser(1, UserRoles.Librarian);
            const int bookId = 20;
            var list = new List<ReservationReadDto>
            {
                new ReservationReadDto { ReservationId = 8, UserId = 5, BookId = bookId }
            };
            _svcMock.Setup(s => s.GetPendingForBookAsync(bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(list);

            var action = await _controller.GetPendingForBook(bookId, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// Retrieving a reservation that does not exist should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task GetReservation_NotFound_Returns404()
        {
            SetUser(9, UserRoles.User);
            _svcMock.Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ReservationReadDto?)null);

            var action = await _controller.GetReservation(99, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action.Result);
        }

        /// <summary>
        /// Owner retrieving their reservation should return 200 OK with the DTO.
        /// </summary>
        [Fact]
        public async Task GetReservation_Owner_ReturnsOk()
        {
            const int me = 10;
            var dto = new ReservationReadDto { ReservationId = 15, UserId = me, BookId = 30 };
            SetUser(me, UserRoles.User);
            _svcMock.Setup(s => s.GetByIdAsync(15, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dto);

            var action = await _controller.GetReservation(15, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Admin retrieving any reservation should return 200 OK with the DTO.
        /// </summary>
        [Fact]
        public async Task GetReservation_Admin_ReturnsOk()
        {
            var dto = new ReservationReadDto { ReservationId = 16, UserId = 20, BookId = 40 };
            SetUser(1, UserRoles.Admin);
            _svcMock.Setup(s => s.GetByIdAsync(16, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dto);

            var action = await _controller.GetReservation(16, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Creating a reservation for another user should return 403 Forbidden.
        /// </summary>
        [Fact]
        public async Task CreateReservation_OtherUser_ReturnsForbid()
        {
            var dto = new ReservationCreateDto { UserId = 5, BookId = 50 };
            SetUser(6, UserRoles.User);

            var action = await _controller.CreateReservation(dto, CancellationToken.None);

            Assert.IsType<ForbidResult>(action.Result);
        }

        /// <summary>
        /// Creating a reservation for oneself should return 201 Created with the DTO.
        /// </summary>
        [Fact]
        public async Task CreateReservation_Self_ReturnsCreated()
        {
            const int me = 7;
            var dto = new ReservationCreateDto { UserId = me, BookId = 60 };
            var created = new ReservationReadDto { ReservationId = 25, UserId = me, BookId = 60 };
            SetUser(me, UserRoles.User);
            _svcMock.Setup(s => s.CreateAsync(dto, me, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(created);

            var action = await _controller.CreateReservation(dto, CancellationToken.None);

            var createdAt = Assert.IsType<CreatedAtActionResult>(action.Result);
            Assert.Equal(nameof(ReservationsController.GetReservation), createdAt.ActionName);
            Assert.Equal(created.ReservationId, createdAt.RouteValues!["id"]);
            Assert.Equal(created, createdAt.Value);
        }

        /// <summary>
        /// Updating with mismatched ID should return 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task UpdateReservation_IdMismatch_ReturnsBadRequest()
        {
            var dto = new ReservationUpdateDto { ReservationId = 99, BookId = 70 };
            SetUser(1, UserRoles.Admin);

            var action = await _controller.UpdateReservation(100, dto, CancellationToken.None);

            Assert.IsType<BadRequestResult>(action);
        }

        /// <summary>
        /// Updating a non-existent reservation should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task UpdateReservation_NotFound_Returns404()
        {
            var dto = new ReservationUpdateDto { ReservationId = 33, BookId = 80 };
            SetUser(1, UserRoles.Librarian);
            _svcMock.Setup(s => s.UpdateAsync(dto, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

            var action = await _controller.UpdateReservation(33, dto, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// A successful update should return 204 NoContent.
        /// </summary>
        [Fact]
        public async Task UpdateReservation_Success_ReturnsNoContent()
        {
            var dto = new ReservationUpdateDto { ReservationId = 44, BookId = 90 };
            SetUser(1, UserRoles.Librarian);
            _svcMock.Setup(s => s.UpdateAsync(dto, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

            var action = await _controller.UpdateReservation(44, dto, CancellationToken.None);

            Assert.IsType<NoContentResult>(action);
        }

        /// <summary>
        /// Deleting a non-existent reservation should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task DeleteReservation_NotFound_Returns404()
        {
            SetUser(2, UserRoles.User);
            _svcMock.Setup(s => s.GetByIdAsync(55, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ReservationReadDto?)null);

            var action = await _controller.DeleteReservation(55, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Non-owner, non-admin attempting to delete another user’s reservation should return 403 Forbidden.
        /// </summary>
        [Fact]
        public async Task DeleteReservation_NonOwnerNonAdmin_ReturnsForbid()
        {
            var dto = new ReservationReadDto { ReservationId = 66, UserId = 3, BookId = 100 };
            SetUser(2, UserRoles.User);
            _svcMock.Setup(s => s.GetByIdAsync(66, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dto);

            var action = await _controller.DeleteReservation(66, CancellationToken.None);

            Assert.IsType<ForbidResult>(action);
        }

        /// <summary>
        /// Owner successfully deleting their reservation should return 204 NoContent.
        /// </summary>
        [Fact]
        public async Task DeleteReservation_OwnerSuccess_ReturnsNoContent()
        {
            var dto = new ReservationReadDto { ReservationId = 77, UserId = 4, BookId = 110 };
            SetUser(4, UserRoles.User);
            _svcMock.Setup(s => s.GetByIdAsync(77, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dto);
            _svcMock.Setup(s => s.DeleteAsync(77, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

            var action = await _controller.DeleteReservation(77, CancellationToken.None);

            Assert.IsType<NoContentResult>(action);
        }

        /// <summary>
        /// Cleanup of expired reservations should return 200 OK with a message indicating count removed.
        /// </summary>
        [Fact]
        public async Task CleanupExpiredReservations_ReturnsOkMessage()
        {
            SetUser(1, UserRoles.Admin);
            _cleanupMock.Setup(c => c.CleanupExpiredReservationsAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(5);

            var action = await _controller.CleanupExpiredReservations(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action);
            dynamic body = ok.Value!;
            Assert.Equal("5 expired reservations removed.", (string)body.message);
        }
    }
}
