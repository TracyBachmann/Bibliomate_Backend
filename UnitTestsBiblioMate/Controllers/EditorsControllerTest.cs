using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="EditorsController"/>.
    /// Verifies CRUD, search, and ensure endpoints with a mocked <see cref="IEditorService"/>.
    /// </summary>
    public class EditorsControllerTest
    {
        private readonly Mock<IEditorService> _serviceMock;
        private readonly EditorsController _controller;

        /// <summary>
        /// Initializes the test environment with:
        /// <list type="bullet">
        ///   <item><description>A mocked <see cref="IEditorService"/>.</description></item>
        ///   <item><description>An instance of <see cref="EditorsController"/> using the mock.</description></item>
        /// </list>
        /// </summary>
        public EditorsControllerTest()
        {
            _serviceMock = new Mock<IEditorService>();
            _controller  = new EditorsController(_serviceMock.Object);
        }

        /// <summary>
        /// Ensures <see cref="EditorsController.GetEditors"/> returns HTTP 200 OK
        /// with the full list of editors when service provides data.
        /// </summary>
        [Fact]
        public async Task GetEditors_ShouldReturnOkWithEditors()
        {
            var list = new List<EditorReadDto>
            {
                new EditorReadDto { EditorId = 1, Name = "E1" },
                new EditorReadDto { EditorId = 2, Name = "E2" }
            };
            _serviceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var action = await _controller.GetEditors(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// Ensures <see cref="EditorsController.GetEditor"/> returns HTTP 200 OK
        /// when the editor exists.
        /// </summary>
        [Fact]
        public async Task GetEditor_ShouldReturnOkWhenFound()
        {
            var dto = new EditorReadDto { EditorId = 5, Name = "Found" };
            _serviceMock
                .Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync((dto, default(ActionResult)));

            var result = await _controller.GetEditor(5, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Ensures <see cref="EditorsController.GetEditor"/> returns HTTP 404 NotFound
        /// when the requested editor does not exist.
        /// </summary>
        [Fact]
        public async Task GetEditor_ShouldReturnNotFoundWhenNotExists()
        {
            _serviceMock
                .Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((null, new NotFoundResult()));

            var result = await _controller.GetEditor(99, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="EditorsController.CreateEditor"/> returns a
        /// <see cref="CreatedAtActionResult"/> with the newly created editor.
        /// </summary>
        [Fact]
        public async Task CreateEditor_ShouldReturnServiceResult()
        {
            var input   = new EditorCreateDto { Name = "NewEditor" };
            var created = new EditorReadDto { EditorId = 10, Name = "NewEditor" };
            var action  = new CreatedAtActionResult(
                actionName: nameof(EditorsController.GetEditor),
                controllerName: "Editors",
                routeValues: new { id = created.EditorId },
                value: created);

            _serviceMock
                .Setup(s => s.CreateAsync(input, It.IsAny<CancellationToken>()))
                .ReturnsAsync((created, action));

            var result = await _controller.CreateEditor(input, CancellationToken.None);

            var createdAt = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(created, createdAt.Value);
            Assert.Equal(nameof(EditorsController.GetEditor), createdAt.ActionName);
        }

        /// <summary>
        /// Ensures <see cref="EditorsController.UpdateEditor"/> returns HTTP 204 NoContent
        /// when update succeeds.
        /// </summary>
        [Fact]
        public async Task UpdateEditor_ShouldReturnNoContentWhenSuccess()
        {
            var input = new EditorUpdateDto { Name = "Updated" };
            _serviceMock
                .Setup(s => s.UpdateAsync(7, input, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.UpdateEditor(7, input, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="EditorsController.UpdateEditor"/> returns HTTP 404 NotFound
        /// when the editor does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateEditor_ShouldReturnNotFoundWhenNotExists()
        {
            var input = new EditorUpdateDto { Name = "Nobody" };
            _serviceMock
                .Setup(s => s.UpdateAsync(99, input, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.UpdateEditor(99, input, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="EditorsController.DeleteEditor"/> returns HTTP 204 NoContent
        /// when deletion succeeds.
        /// </summary>
        [Fact]
        public async Task DeleteEditor_ShouldReturnNoContentWhenSuccess()
        {
            _serviceMock
                .Setup(s => s.DeleteAsync(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.DeleteEditor(3, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="EditorsController.DeleteEditor"/> returns HTTP 404 NotFound
        /// when the editor does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteEditor_ShouldReturnNotFoundWhenNotExists()
        {
            _serviceMock
                .Setup(s => s.DeleteAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.DeleteEditor(99, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="EditorsController.SearchEditors"/> returns HTTP 200 OK
        /// with matching results from the service.
        /// </summary>
        [Fact]
        public async Task SearchEditors_ShouldReturnOkWithResults()
        {
            var results = new List<EditorReadDto>
            {
                new EditorReadDto { EditorId = 1, Name = "SearchResult" }
            };
            _serviceMock
                .Setup(s => s.SearchAsync("SearchResult", 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(results);

            var action = await _controller.SearchEditors("SearchResult", 20, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(results, ok.Value);
        }

        /// <summary>
        /// Ensures <see cref="EditorsController.EnsureEditor"/> returns HTTP 201 Created
        /// when a new editor is created.
        /// </summary>
        [Fact]
        public async Task EnsureEditor_ShouldReturnCreatedWhenNew()
        {
            var dto  = new EditorCreateDto { Name = "FreshEditor" };
            var read = new EditorReadDto { EditorId = 11, Name = "FreshEditor" };

            _serviceMock
                .Setup(s => s.EnsureAsync(dto.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync((read, true));

            var result = await _controller.EnsureEditor(dto, CancellationToken.None);

            var created  = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returned = Assert.IsType<EditorReadDto>(created.Value);
            Assert.Equal(read.EditorId, returned.EditorId);
            Assert.Equal(read.Name, returned.Name);
        }

        /// <summary>
        /// Ensures <see cref="EditorsController.EnsureEditor"/> returns HTTP 200 OK
        /// when the editor already exists.
        /// </summary>
        [Fact]
        public async Task EnsureEditor_ShouldReturnOkWhenExists()
        {
            var dto  = new EditorCreateDto { Name = "ExistingEditor" };
            var read = new EditorReadDto { EditorId = 12, Name = "ExistingEditor" };

            _serviceMock
                .Setup(s => s.EnsureAsync(dto.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync((read, false));

            var result = await _controller.EnsureEditor(dto, CancellationToken.None);

            var ok       = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<EditorReadDto>(ok.Value);
            Assert.Equal(read.EditorId, returned.EditorId);
            Assert.Equal(read.Name, returned.Name);
        }
    }
}
