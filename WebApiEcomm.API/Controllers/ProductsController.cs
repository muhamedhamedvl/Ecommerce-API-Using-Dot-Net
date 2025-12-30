using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiEcomm.API.Helper;
using WebApiEcomm.Core.Entites.Dtos;
using WebApiEcomm.Core.Entites.Identity;
using WebApiEcomm.Core.Interfaces.IUnitOfWork;
using WebApiEcomm.Core.Services;
using WebApiEcomm.Core.Sharing;

namespace WebApiEcomm.API.Controllers
{
    public class ProductsController : BaseController
    {
        private readonly IImageManagementService imageManagementService;
        public ProductsController(IUnitOfWork work, IMapper mapper, IImageManagementService imageManagementService) : base(work, mapper)
        {
            this.imageManagementService = imageManagementService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllProducts([FromQuery]ProductParams productParams)
        {
            try
            {
                var Product = await 
                    _work
                    .ProductRepository
                    .GetAllAsync(productParams);
                var totalCount = await _work.ProductRepository.CountAsync();
                return Ok(new PagedResponse<ProductDto>(productParams.Page, productParams.PageSize, totalCount, Product));
                
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseApi(400, $"Error: {ex.Message}"));
            }
        }

        [HttpGet("{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductById(int productId)
        {
            try
            {
                var product = await _work.ProductRepository.GetByIdAsync(
                    productId,
                    p => p.Category,
                    p => p.Photos
                );

                if (product == null)
                {
                    return NotFound(new ResponseApi(404, "Product not found."));
                }

                var res = mapper.Map<ProductDto>(product);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseApi(400, $"Error: {ex.Message}"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddProduct(AddProductDto productdto)
        {
            try
            {
                var isAdded = await _work.ProductRepository.AddAsync(productdto);

                if (!isAdded)
                    return BadRequest(new ResponseApi(400, "Failed to add product."));

                return StatusCode(201, new ResponseApi(201, "product created"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseApi(400, $"Error: {ex.Message}"));
            }
        }

        [HttpPut("{productId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int productId, UpdateProductDto updateProductDTO)
        {
            try
            {
                updateProductDTO.Id = productId;
                await _work.ProductRepository.UpdateAsync(updateProductDTO);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{productId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> delete(int productId)
        {
            try
            {
                var product = await _work.ProductRepository
                    .GetByIdAsync(productId, x => x.Photos, x => x.Category);

                await _work.ProductRepository.DeleteAsync(product);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(error: ex.Message);
            }
        }
    }
}
