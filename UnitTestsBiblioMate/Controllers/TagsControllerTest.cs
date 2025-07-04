using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="TagsController"/>.
    /// Verifies CRUD endpoints and anonymous access.
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
        /// Retrieving all tags returns 200 OK with the list.
        /// </summary>
        [Fact]
        public async Task GetTags_ReturnsOkWithList()
        {
            // Arrange
            var list = new List<TagReadDto>
            {
                new TagReadDto { TagId = 1, Name = "TagA" },
                new TagReadDto { TagId = 2, Name = "TagB" }
            };
            _serviceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            // Act
            var action = await _controller.GetTags(CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// Retrieving an existing tag returns 200 OK.
        /// </summary>
        [Fact]
        public async Task GetTag_Exists_ReturnsOk()
        {
            // Arrange
            var dto = new TagReadDto { TagId = 5, Name = "TagX" };
            _serviceMock
                .Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // Act
            var action = await _controller.GetTag(5, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Retrieving a missing tag returns 404 NotFound.
        /// </summary>
        [Fact]
        public async Task GetTag_NotFound_Returns404()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TagReadDto?)null);

            // Act
            var action = await _controller.GetTag(99, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action.Result);
        }

        /// <summary>
        /// Creating a tag returns 201 Created with the DTO.
        /// </summary>
        [Fact]
        public async Task CreateTag_ReturnsCreated()
        {
            // Arrange
            var createDto = new TagCreateDto { Name = "NewTag" };
            var created   = new TagReadDto { TagId = 10, Name = "NewTag" };
            _serviceMock
                .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

            // Act
            var action = await _controller.CreateTag(createDto, CancellationToken.None);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(action.Result);
            Assert.Equal(nameof(TagsController.GetTag), createdAt.ActionName);
            Assert.Equal(created.TagId, createdAt.RouteValues!["id"]);
            Assert.Equal(created, createdAt.Value);
        }

        /// <summary>
        /// Updating with mismatched ID returns 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task UpdateTag_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var dto = new TagUpdateDto { TagId = 5, Name = "X" };

            // Act
            var action = await _controller.UpdateTag(6, dto, CancellationToken.None);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(action);
            Assert.Equal("Route ID and payload TagId do not match.", bad.Value);
        }

        /// <summary>
        /// Updating non-existent tag returns 404 NotFound.
        /// </summary>
        [Fact]
        public async Task UpdateTag_NotFound_Returns404()
        {
            // Arrange
            var dto = new TagUpdateDto { TagId = 7, Name = "Y" };
            _serviceMock
                .Setup(s => s.UpdateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var action = await _controller.UpdateTag(7, dto, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Successful update returns 204 NoContent.
        /// </summary>
        [Fact]
        public async Task UpdateTag_Success_ReturnsNoContent()
        {
            // Arrange
            var dto = new TagUpdateDto { TagId = 8, Name = "Z" };
            _serviceMock
                .Setup(s => s.UpdateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var action = await _controller.UpdateTag(8, dto, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(action);
        }

        /// <summary>
        /// Deleting non-existent tag returns 404 NotFound.
        /// </summary>
        [Fact]
        public async Task DeleteTag_NotFound_Returns404()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.DeleteAsync(20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var action = await _controller.DeleteTag(20, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Successful delete returns 204 NoContent.
        /// </summary>
        [Fact]
        public async Task DeleteTag_Success_ReturnsNoContent()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.DeleteAsync(21, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var action = await _controller.DeleteTag(21, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(action);
        }
    }
}