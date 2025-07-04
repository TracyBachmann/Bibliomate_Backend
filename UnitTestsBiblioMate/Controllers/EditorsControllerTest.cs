using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="EditorsController"/>.
    /// Verifies behavior of all CRUD endpoints with mocked <see cref="IEditorService"/>.
    /// </summary>
    public class EditorsControllerTest
    {
        private readonly Mock<IEditorService> _serviceMock;
        private readonly EditorsController _controller;

        /// <summary>
        /// Initializes mocks and controller for testing.
        /// </summary>
        public EditorsControllerTest()
        {
            _serviceMock = new Mock<IEditorService>();
            _controller = new EditorsController(_serviceMock.Object);
        }

        /// <summary>
        /// Ensures that GetEditors returns 200 OK with a list of editors.
        /// </summary>
        [Fact]
        public async Task GetEditors_ShouldReturnOkWithEditors()
        {
            // Arrange
            var list = new List<EditorReadDto>
            {
                new EditorReadDto { EditorId = 1, Name = "E1" },
                new EditorReadDto { EditorId = 2, Name = "E2" }
            };
            _serviceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            // Act
            var action = await _controller.GetEditors(CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// Ensures that GetEditor returns 200 OK when the editor exists.
        /// </summary>
        [Fact]
        public async Task GetEditor_ShouldReturnOkWhenFound()
        {
            // Arrange
            var dto = new EditorReadDto { EditorId = 5, Name = "Found" };
            _serviceMock
                .Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync((dto, default(ActionResult)));

            // Act
            var result = await _controller.GetEditor(5, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Ensures that GetEditor returns 404 NotFound when the editor does not exist.
        /// </summary>
        [Fact]
        public async Task GetEditor_ShouldReturnNotFoundWhenNotExists()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((null, new NotFoundResult()));

            // Act
            var result = await _controller.GetEditor(99, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Ensures that CreateEditor returns the service's action result.
        /// </summary>
        [Fact]
        public async Task CreateEditor_ShouldReturnServiceResult()
        {
            // Arrange
            var input = new EditorCreateDto { Name = "NewEditor" };
            var created = new EditorReadDto { EditorId = 10, Name = "NewEditor" };
            var action = new CreatedAtActionResult(
                actionName: nameof(EditorsController.GetEditor),
                controllerName: "Editors",
                routeValues: new { id = created.EditorId },
                value: created);

            _serviceMock
                .Setup(s => s.CreateAsync(input, It.IsAny<CancellationToken>()))
                .ReturnsAsync((created, action));

            // Act
            var result = await _controller.CreateEditor(input, CancellationToken.None);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(created, createdAt.Value);
            Assert.Equal(nameof(EditorsController.GetEditor), createdAt.ActionName);
        }

        /// <summary>
        /// Ensures that UpdateEditor returns 204 NoContent when update succeeds.
        /// </summary>
        [Fact]
        public async Task UpdateEditor_ShouldReturnNoContentWhenSuccess()
        {
            // Arrange
            var input = new EditorUpdateDto { Name = "Updated" };
            _serviceMock
                .Setup(s => s.UpdateAsync(7, input, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateEditor(7, input, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// Ensures that UpdateEditor returns 404 NotFound when the editor does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateEditor_ShouldReturnNotFoundWhenNotExists()
        {
            // Arrange
            var input = new EditorUpdateDto { Name = "Nobody" };
            _serviceMock
                .Setup(s => s.UpdateAsync(99, input, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateEditor(99, input, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Ensures that DeleteEditor returns 204 NoContent when deletion succeeds.
        /// </summary>
        [Fact]
        public async Task DeleteEditor_ShouldReturnNoContentWhenSuccess()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.DeleteAsync(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteEditor(3, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// Ensures that DeleteEditor returns 404 NotFound when the editor does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteEditor_ShouldReturnNotFoundWhenNotExists()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.DeleteAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteEditor(99, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}