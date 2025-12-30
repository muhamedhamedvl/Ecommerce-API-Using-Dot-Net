using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiEcomm.API.Controllers;
using WebApiEcomm.API.Helper;
using WebApiEcomm.Core.Entites.Dtos;
using WebApiEcomm.Core.Entites.Product;
using WebApiEcomm.Core.Interfaces;
using WebApiEcomm.Core.Interfaces.IUnitOfWork;

namespace WebApiEcomm.UnitTests.API.Controllers
{
    [TestFixture]
    public class categoriesControllerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<ICategoryRepository> _mockCategoryRepo;
        private Mock<IMapper> _mockMapper;
        private CategoriesController _controller;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCategoryRepo = new Mock<ICategoryRepository>();
            _mockMapper = new Mock<IMapper>();

            _mockUnitOfWork.Setup(u => u.CategoryRepository).Returns(_mockCategoryRepo.Object);

            _controller = new CategoriesController(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Test]
        public async Task GetAllCategories_WhenCategoriesExist_ReturnsOk()
        {
            // Arrange
            var categories = new List<Category> { new Category { Id = 1, Name = "Electronics" } };
            _mockCategoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _controller.GetAllCategories() as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            var value = result.Value as IEnumerable<Category>;
            Assert.That(value.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetAllCategories_WhenNoCategories_ReturnsBadRequest()
        {
            // Arrange
            _mockCategoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>());

            // Act
            var result = await _controller.GetAllCategories() as BadRequestObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task GetCategoryById_WhenCategoryExists_ReturnsOk()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Books" };
            _mockCategoryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

            // Act
            var result = await _controller.GetCategoryById(1) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That((result.Value as Category).Name, Is.EqualTo("Books"));
        }

        [Test]
        public async Task GetCategoryById_WhenCategoryDoesNotExist_ReturnsBadRequest()
        {
            // Arrange
            _mockCategoryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Category)null);

            // Act
            var result = await _controller.GetCategoryById(1) as BadRequestObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task CreateCategory_WhenValidDto_ReturnsOk()
        {
            // Arrange
            var dto = new CategoryDto("Clothes", null);
            var category = new Category { Id = 1, Name = "Clothes" };
            _mockMapper.Setup(m => m.Map<Category>(dto)).Returns(category);

            // Act
            var result = await _controller.CreateCategory(dto) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That((result.Value as ResponseApi).Message, Is.EqualTo("Item has been added"));
        }

        [Test]
        public async Task CreateCategory_WhenDtoIsNull_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.CreateCategory(null) as BadRequestObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task UpdateCategory_WhenValidDto_ReturnsOk()
        {
            // Arrange
            var dto = new UpdateCategoryDto(1, "Updated", null);
            var category = new Category { Id = 1, Name = "Updated" };
            _mockMapper.Setup(m => m.Map<Category>(dto)).Returns(category);

            // Act
            var result = await _controller.UpdateCategory(dto) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That((result.Value as ResponseApi).Message, Is.EqualTo("Item has been updated"));
        }

        [Test]
        public async Task Delete_WhenValidId_ReturnsOk()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "ToDelete" };
            _mockCategoryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

            // Act
            var result = await _controller.Delete(1) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That((result.Value as ResponseApi).Message, Is.EqualTo("Item has been deleted"));
        }

        [Test]
        public async Task Delete_WhenIdIsInvalid_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Delete(0) as BadRequestObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }
    }
}
