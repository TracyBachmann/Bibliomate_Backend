using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="TagsController"/>.
    /// Verifies CRUD endpoints, anonymous search, and ensure behavior.
    /// </summary>
    public class TagsControllerTest
    {
        private readonly Mock<ITagService> _serviceMock;
        private readonly TagsController    _controller;

        public TagsControllerTest()
        {
            _serviceMock = new Mock<ITagService>();
            _controller  = new TagsController(_serviceMock.Object);
        }

        /// <summary>
        /// Retrieving all tags should return 200 OK with the full list.
        /// </summary>
        [Fact]
        public async Task GetTags_ReturnsOkWithList()
        {
            var list = new List<TagReadDto>
            {
                new TagReadDto { TagId = 1, Name = "TagA" },
                new TagReadDto { TagId = 2, Name = "TagB" }
            };
            _serviceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var action = await _controller.GetTags(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// Retrieving a tag by ID that exists should return 200 OK with the tag.
        /// </summary>
        [Fact]
        public async Task GetTag_Exists_ReturnsOk()
        {
            var dto = new TagReadDto { TagId = 5, Name = "TagX" };
            _serviceMock
                .Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var action = await _controller.GetTag(5, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Retrieving a tag by ID that does not exist should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task GetTag_NotFound_Returns404()
        {
            _serviceMock
                .Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TagReadDto?)null);

            var action = await _controller.GetTag(99, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action.Result);
        }

        /// <summary>
        /// Creating a new tag should return 201 Created with location header and tag DTO.
        /// </summary>
        [Fact]
        public async Task CreateTag_ReturnsCreated()
        {
            var createDto = new TagCreateDto { Name = "NewTag" };
            var created   = new TagReadDto { TagId = 10, Name = "NewTag" };
            _serviceMock
                .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

            var action = await _controller.CreateTag(createDto, CancellationToken.None);

            var createdAt = Assert.IsType<CreatedAtActionResult>(action.Result);
            Assert.Equal(nameof(TagsController.GetTag), createdAt.ActionName);
            Assert.Equal(created.TagId, createdAt.RouteValues!["id"]);
            Assert.Equal(created, createdAt.Value);
        }

        /// <summary>
        /// Updating with mismatched IDs between route and body should return 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task UpdateTag_IdMismatch_ReturnsBadRequest()
        {
            var dto = new TagUpdateDto { TagId = 5, Name = "X" };

            var action = await _controller.UpdateTag(6, dto, CancellationToken.None);

            var bad = Assert.IsType<BadRequestObjectResult>(action);

            // Handle either a string or an object { error, details }
            if (bad.Value is string str)
            {
                Assert.Contains("TagId", str);
            }
            else
            {
                dynamic body = bad.Value!;
                Assert.Equal("IdMismatch", (string)body.error);
            }
        }

        /// <summary>
        /// Updating a tag that does not exist should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task UpdateTag_NotFound_Returns404()
        {
            var dto = new TagUpdateDto { TagId = 7, Name = "Y" };
            _serviceMock
                .Setup(s => s.UpdateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var action = await _controller.UpdateTag(7, dto, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Successfully updating an existing tag should return 204 NoContent.
        /// </summary>
        [Fact]
        public async Task UpdateTag_Success_ReturnsNoContent()
        {
            var dto = new TagUpdateDto { TagId = 8, Name = "Z" };
            _serviceMock
                .Setup(s => s.UpdateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var action = await _controller.UpdateTag(8, dto, CancellationToken.None);

            Assert.IsType<NoContentResult>(action);
        }

        /// <summary>
        /// Deleting a tag that does not exist should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task DeleteTag_NotFound_Returns404()
        {
            _serviceMock
                .Setup(s => s.DeleteAsync(20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var action = await _controller.DeleteTag(20, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Successfully deleting a tag should return 204 NoContent.
        /// </summary>
        [Fact]
        public async Task DeleteTag_Success_ReturnsNoContent()
        {
            _serviceMock
                .Setup(s => s.DeleteAsync(21, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var action = await _controller.DeleteTag(21, CancellationToken.None);

            Assert.IsType<NoContentResult>(action);
        }

        /// <summary>
        /// Searching tags should return 200 OK with the filtered list.
        /// </summary>
        [Fact]
        public async Task Search_ReturnsOkWithFilteredList()
        {
            var list = new List<TagReadDto>
            {
                new TagReadDto { TagId = 1, Name = "CSharp" },
                new TagReadDto { TagId = 2, Name = "Coding" }
            };
            _serviceMock
                .Setup(s => s.SearchAsync("c", 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var action = await _controller.Search(search: "c", take: 10, cancellationToken: CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// Ensuring a new tag that does not exist should create it and return 201 CreatedAt.
        /// </summary>
        [Fact]
        public async Task Ensure_Created_ReturnsCreatedAt()
        {
            var create = new TagCreateDto { Name = "NewOne" };
            var read   = new TagReadDto { TagId = 42, Name = "NewOne" };

            _serviceMock
                .Setup(s => s.EnsureAsync("NewOne", It.IsAny<CancellationToken>()))
                .ReturnsAsync((read, true));

            var action = await _controller.Ensure(create, CancellationToken.None);

            var createdAt = Assert.IsType<CreatedAtActionResult>(action.Result);
            Assert.Equal(nameof(TagsController.GetTag), createdAt.ActionName);
            Assert.Equal(read.TagId, createdAt.RouteValues!["id"]);
            Assert.Equal(read, createdAt.Value);
        }

        /// <summary>
        /// Ensuring a tag that already exists should return 200 OK with the existing tag.
        /// </summary>
        [Fact]
        public async Task Ensure_AlreadyExists_ReturnsOk()
        {
            var create = new TagCreateDto { Name = "Existing" };
            var read   = new TagReadDto { TagId = 7, Name = "Existing" };

            _serviceMock
                .Setup(s => s.EnsureAsync("Existing", It.IsAny<CancellationToken>()))
                .ReturnsAsync((read, false));

            var action = await _controller.Ensure(create, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(read, ok.Value);
        }
    }
}
