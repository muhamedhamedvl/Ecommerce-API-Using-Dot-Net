using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiEcomm.API.Controllers;
using WebApiEcomm.API.Helper;
using WebApiEcomm.Core.Entites.Dtos;
using WebApiEcomm.Core.Entites.Product;
using WebApiEcomm.Core.Interfaces;
using WebApiEcomm.Core.Interfaces.IUnitOfWork;
using WebApiEcomm.Core.Services;
using WebApiEcomm.Core.Sharing;

namespace WebApiEcomm.UnitTests.API.Controllers
{
    [TestFixture]
    public class productsControllerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IProductRepository> _mockProductRepo;
        private Mock<IMapper> _mockMapper;
        private Mock<IImageManagementService> _mockImageService;

        private ProductsController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockProductRepo = new Mock<IProductRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockImageService = new Mock<IImageManagementService>();

            // Wire repo to unit of work
            _mockUnitOfWork.Setup(u => u.ProductRepository).Returns(_mockProductRepo.Object);

            _controller = new ProductsController(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockImageService.Object
            );
        }

        [Test]
        public async Task GetAllProducts_ReturnsOk_WithPagination()
        {
            // Arrange
            var productParams = new ProductParams { PageNumber = 1, PageSize = 10 };
            var products = new List<ProductDto>
            {
                new ProductDto { Id = 1, Name = "Product1" },
                new ProductDto { Id = 2, Name = "Product2" }
            };

            _mockProductRepo.Setup(r => r.GetAllAsync(productParams)).ReturnsAsync(products);
            _mockProductRepo.Setup(r => r.CountAsync()).ReturnsAsync(products.Count);

            // Act
            var result = await _controller.GetAllProducts(productParams) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            var pagination = result.Value as Pagination<ProductDto>;
            Assert.That(pagination, Is.Not.Null);
            Assert.That(pagination.Data.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetProductById_WhenProductExists_ReturnsOk()
        {
            // Arrange
            var productEntity = new Product { Id = 1, Name = "Test Product" };
            var productDto = new ProductDto { Id = 1, Name = "Test Product" };

            _mockProductRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<System.Linq.Expressions.Expression<Func<Product, object>>[]>()))
                .ReturnsAsync(productEntity);

            _mockMapper.Setup(m => m.Map<ProductDto>(productEntity)).Returns(productDto);

            // Act
            var result = await _controller.GetProductById(1) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            var returnedProduct = result.Value as ProductDto;
            Assert.That(returnedProduct.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task GetProductById_WhenProductDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _mockProductRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<System.Linq.Expressions.Expression<Func<Product, object>>[]>()))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _controller.GetProductById(99);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task AddProduct_WhenSuccess_ReturnsOk()
        {
            // Arrange
            var newProduct = new AddProductDto { Name = "NewProduct" };
            _mockProductRepo.Setup(r => r.AddAsync(newProduct)).ReturnsAsync(true);

            // Act
            var result = await _controller.AddProduct(newProduct);

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
        }

        [Test]
        public async Task AddProduct_WhenFails_ReturnsBadRequest()
        {
            // Arrange
            var newProduct = new AddProductDto { Name = "FailProduct" };
            _mockProductRepo.Setup(r => r.AddAsync(newProduct)).ReturnsAsync(false);

            // Act
            var result = await _controller.AddProduct(newProduct);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Update_ReturnsOk()
        {
            // Arrange
            var updateDto = new UpdateProductDto { Id = 1, Name = "Updated Product" };
            _mockProductRepo.Setup(r => r.UpdateAsync(updateDto)).ReturnsAsync(true);

            // Act
            var result = await _controller.Update(updateDto);

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
        }

        [Test]
        public async Task Delete_ReturnsOk()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Delete Me" };

            _mockProductRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<System.Linq.Expressions.Expression<Func<Product, object>>[]>()))
                .ReturnsAsync(product);

            _mockProductRepo.Setup(r => r.DeleteAsync(product)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.delete(1);

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
        }
    }
}
