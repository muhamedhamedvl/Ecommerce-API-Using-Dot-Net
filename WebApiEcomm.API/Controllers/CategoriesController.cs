using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiEcomm.API.Helper;
using WebApiEcomm.Core.Entites.Dtos;
using WebApiEcomm.Core.Entites.Product;
using WebApiEcomm.Core.Interfaces.IUnitOfWork;
using WebApiEcomm.Core.Sharing;

namespace WebApiEcomm.API.Controllers
{
    public class CategoriesController : BaseController
    {
        public CategoriesController(IUnitOfWork work, IMapper mapper) : base(work, mapper)
        {
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCategories([FromQuery] PaginationParams paginationParams)
        {
            try
            {
                var allCategories = await _work.CategoryRepository.GetAllAsync();
                if (allCategories == null || !allCategories.Any())
                {
                    return Ok(new PagedResponse<CategoryDto>(paginationParams.Page, paginationParams.PageSize, 0, new List<CategoryDto>()));
                }

                // Map to DTOs
                var categoryDtos = mapper.Map<IEnumerable<CategoryDto>>(allCategories);

                // Apply pagination
                var totalCount = categoryDtos.Count();
                var paginatedCategories = categoryDtos
                    .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ToList();

                return Ok(new PagedResponse<CategoryDto>(paginationParams.Page, paginationParams.PageSize, totalCount, paginatedCategories));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoryById(int categoryId)
        {
            try
            {
                var category = await _work.CategoryRepository.GetByIdAsync(categoryId);
                if (category == null)
                {
                    return NotFound(new ResponseApi(404, "Category not found."));
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving data: {ex.Message}");
            }
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory(CategoryDto categorydto)
        {
            if (categorydto == null)
            {
                return BadRequest(new ResponseApi(400));
            }
            try
            {
                var newCategory = mapper.Map<Category>(categorydto);
                await _work.CategoryRepository.AddAsync(newCategory);
                return StatusCode(201 , new ResponseApi(201 , "category created"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("{categoryId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int categoryId, UpdateCategoryDto updatecategorydto)
        { 
            try
            {
                var  category = mapper.Map<Category>(updatecategorydto);
                category.Id = categoryId;
                await _work.CategoryRepository.UpdateAsync(category);
                return NoContent();
            }
            catch(Exception ex)
            {
                return BadRequest (ex.Message);
            }
        }
        [HttpDelete("{categoryId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int categoryId)
        {
            if(categoryId <= 0)
            {
                return BadRequest(new ResponseApi(400));
            }
            try
            {
                var category = await _work.CategoryRepository.GetByIdAsync(categoryId);
                if (category is null)
                {
                    return NotFound(new ResponseApi(404, "Category not found."));
                }
                await _work.CategoryRepository.DeleteAsync(category);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                
            }
        } 
    } 
}
