using System.Security.Claims;
using BackendBiblioMate.Services.Loans;
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
    /// Verifies authorization, CRUD behavior, user‐scoped access, and cleanup endpoint.
    /// </summary>
    public class ReservationsControllerTest
    {
        private readonly Mock<IReservationService>      _svcMock;
        private readonly Mock<ReservationCleanupService> _cleanupMock;
        private readonly ReservationsController         _controller;

        public ReservationsControllerTest()
        {
            _svcMock     = new Mock<IReservationService>();
            _cleanupMock = new Mock<ReservationCleanupService>(/* no args ctor */);
            _controller  = new ReservationsController(_svcMock.Object, _cleanupMock.Object);
        }

        /// <summary>
        /// Helper to set HttpContext.User with given ID and roles.
        /// </summary>
        private void SetUser(int userId, params string[] roles)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));
            var identity = new ClaimsIdentity(claims, "test");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        /// <summary>
        /// Admin fetching all reservations should return 200 OK with full list.
        /// </summary>
        [Theory]
        [InlineData(UserRoles.Admin)]
        [InlineData(UserRoles.Librarian)]
        public async Task GetReservations_AsPrivileged_ReturnsOk(string role)
        {
            // Arrange
            SetUser(1, role);
            var list = new List<ReservationReadDto>
            {
                new ReservationReadDto { ReservationId = 5, UserId = 2, BookId = 10 },
                new ReservationReadDto { ReservationId = 6, UserId = 3, BookId = 11 }
            };
            _svcMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(list);

            // Act
            var action = await _controller.GetReservations(CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// A normal user requesting another’s reservations should get 403 Forbidden.
        /// </summary>
        [Fact]
        public async Task GetUserReservations_OtherUser_ReturnsForbid()
        {
            // Arrange
            SetUser(2, UserRoles.User);
            // Act
            var action = await _controller.GetUserReservations(3, CancellationToken.None);
            // Assert
            Assert.IsType<ForbidResult>(action.Result);
        }

        /// <summary>
        /// A user requesting their own reservations should get 200 OK with list.
        /// </summary>
        [Fact]
        public async Task GetUserReservations_Self_ReturnsOk()
        {
            // Arrange
            const int me = 4;
            SetUser(me, UserRoles.User);
            var list = new List<ReservationReadDto>
            {
                new ReservationReadDto { ReservationId = 7, UserId = me, BookId = 12 }
            };
            _svcMock.Setup(s => s.GetByUserAsync(me, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(list);

            // Act
            var action = await _controller.GetUserReservations(me, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// Librarian fetching pending for a book should return 200 OK with list.
        /// </summary>
        [Fact]
        public async Task GetPendingForBook_AsPrivileged_ReturnsOk()
        {
            // Arrange
            SetUser(1, UserRoles.Librarian);
            const int bookId = 20;
            var list = new List<ReservationReadDto>
            {
                new ReservationReadDto { ReservationId = 8, UserId = 5, BookId = bookId }
            };
            _svcMock.Setup(s => s.GetPendingForBookAsync(bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(list);

            // Act
            var action = await _controller.GetPendingForBook(bookId, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// Retrieving missing reservation should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task GetReservation_NotFound_Returns404()
        {
            // Arrange
            SetUser(9, UserRoles.User);
            _svcMock.Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ReservationReadDto?)null);

            // Act
            var action = await _controller.GetReservation(99, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action.Result);
        }

        /// <summary>
        /// Owner retrieving their reservation should return 200 OK with the DTO.
        /// </summary>
        [Fact]
        public async Task GetReservation_Owner_ReturnsOk()
        {
            // Arrange
            const int me = 10;
            var dto = new ReservationReadDto { ReservationId = 15, UserId = me, BookId = 30 };
            SetUser(me, UserRoles.User);
            _svcMock.Setup(s => s.GetByIdAsync(15, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dto);

            // Act
            var action = await _controller.GetReservation(15, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Admin retrieving any reservation should return 200 OK.
        /// </summary>
        [Fact]
        public async Task GetReservation_Admin_ReturnsOk()
        {
            // Arrange
            var dto = new ReservationReadDto { ReservationId = 16, UserId = 20, BookId = 40 };
            SetUser(1, UserRoles.Admin);
            _svcMock.Setup(s => s.GetByIdAsync(16, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dto);

            // Act
            var action = await _controller.GetReservation(16, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Creating reservation for another user should return 403 Forbidden.
        /// </summary>
        [Fact]
        public async Task CreateReservation_OtherUser_ReturnsForbid()
        {
            // Arrange
            var dto = new ReservationCreateDto { UserId = 5, BookId = 50 };
            SetUser(6, UserRoles.User);

            // Act
            var action = await _controller.CreateReservation(dto, CancellationToken.None);

            // Assert
            Assert.IsType<ForbidResult>(action.Result);
        }

        /// <summary>
        /// Creating reservation for self should return 201 Created with DTO.
        /// </summary>
        [Fact]
        public async Task CreateReservation_Self_ReturnsCreated()
        {
            // Arrange
            const int me = 7;
            var dto = new ReservationCreateDto { UserId = me, BookId = 60 };
            var created = new ReservationReadDto { ReservationId = 25, UserId = me, BookId = 60 };
            SetUser(me, UserRoles.User);
            _svcMock.Setup(s => s.CreateAsync(dto, me, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(created);

            // Act
            var action = await _controller.CreateReservation(dto, CancellationToken.None);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(action.Result);
            Assert.Equal(nameof(ReservationsController.GetReservation), createdAt.ActionName);
            Assert.Equal(created.ReservationId, createdAt.RouteValues!["id"]);
            Assert.Equal(created, createdAt.Value);
        }

        /// <summary>
        /// Updating with mismatched ID should yield 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task UpdateReservation_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var dto = new ReservationUpdateDto { ReservationId = 99, BookId = 70 };
            SetUser(1, UserRoles.Admin);

            // Act
            var action = await _controller.UpdateReservation(100, dto, CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestResult>(action);
        }

        /// <summary>
        /// Updating non-existent reservation should yield 404 NotFound.
        /// </summary>
        [Fact]
        public async Task UpdateReservation_NotFound_Returns404()
        {
            // Arrange
            var dto = new ReservationUpdateDto { ReservationId = 33, BookId = 80 };
            SetUser(1, UserRoles.Librarian);
            _svcMock.Setup(s => s.UpdateAsync(dto, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

            // Act
            var action = await _controller.UpdateReservation(33, dto, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Successful update should yield 204 NoContent.
        /// </summary>
        [Fact]
        public async Task UpdateReservation_Success_ReturnsNoContent()
        {
            // Arrange
            var dto = new ReservationUpdateDto { ReservationId = 44, BookId = 90 };
            SetUser(1, UserRoles.Librarian);
            _svcMock.Setup(s => s.UpdateAsync(dto, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

            // Act
            var action = await _controller.UpdateReservation(44, dto, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(action);
        }

        /// <summary>
        /// Deleting non-existent reservation should yield 404 NotFound.
        /// </summary>
        [Fact]
        public async Task DeleteReservation_NotFound_Returns404()
        {
            // Arrange
            SetUser(2, UserRoles.User);
            _svcMock.Setup(s => s.GetByIdAsync(55, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ReservationReadDto?)null);

            // Act
            var action = await _controller.DeleteReservation(55, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Non-owner non-admin deleting another’s reservation should yield 403 Forbidden.
        /// </summary>
        [Fact]
        public async Task DeleteReservation_NonOwnerNonAdmin_ReturnsForbid()
        {
            // Arrange
            var dto = new ReservationReadDto { ReservationId = 66, UserId = 3, BookId = 100 };
            SetUser(2, UserRoles.User);
            _svcMock.Setup(s => s.GetByIdAsync(66, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dto);

            // Act
            var action = await _controller.DeleteReservation(66, CancellationToken.None);

            // Assert
            Assert.IsType<ForbidResult>(action);
        }

        /// <summary>
        /// Owner deleting their reservation successfully should yield 204 NoContent.
        /// </summary>
        [Fact]
        public async Task DeleteReservation_OwnerSuccess_ReturnsNoContent()
        {
            // Arrange
            var dto = new ReservationReadDto { ReservationId = 77, UserId = 4, BookId = 110 };
            SetUser(4, UserRoles.User);
            _svcMock.Setup(s => s.GetByIdAsync(77, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dto);
            _svcMock.Setup(s => s.DeleteAsync(77, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

            // Act
            var action = await _controller.DeleteReservation(77, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(action);
        }

        /// <summary>
        /// Cleanup expired reservations should return 200 OK with a message.
        /// </summary>
        [Fact]
        public async Task CleanupExpiredReservations_ReturnsOkMessage()
        {
            // Arrange
            SetUser(1, UserRoles.Admin);
            _cleanupMock.Setup(c => c.CleanupExpiredReservationsAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(5);

            // Act
            var action = await _controller.CleanupExpiredReservations(CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action);
            dynamic body = ok.Value!;
            Assert.Equal("5 expired reservations removed.", (string)body.message);
        }
    }
}