using Assignment2.Data;
using Assignment2.DTOs;
using Assignment2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignment2.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayStore _store;
        public CategoriesController(RaceDayStore store) => _store = store;

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetCategories() => Ok(_store.Categories);

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public IActionResult GetCategory(int id)
        {
            var category = _store.Categories.FirstOrDefault(c => c.CategoryID == id);
            return category == null ? NotFound(new { message = "Category does not exist." }) : Ok(category);
        }

        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public IActionResult CreateCategory(CreateCategoryDto dto)
        {
            if (_store.Categories.Any(c => c.CategoryName.Equals(dto.CategoryName.Trim(), StringComparison.OrdinalIgnoreCase)))
                return Conflict(new { message = "Category already exists." });

            var category = new Category
            {
                CategoryID = _store.GetNextCategoryId(),
                CategoryName = dto.CategoryName.Trim(),
                Description = dto.Description.Trim()
            };
            _store.Categories.Add(category);
            return StatusCode(201, category);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public IActionResult UpdateCategory(int id, CreateCategoryDto dto)
        {
            var category = _store.Categories.FirstOrDefault(c => c.CategoryID == id);
            if (category == null) return NotFound(new { message = "Category does not exist." });
            category.CategoryName = dto.CategoryName.Trim();
            category.Description = dto.Description.Trim();
            return Ok(category);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public IActionResult DeleteCategory(int id)
        {
            var category = _store.Categories.FirstOrDefault(c => c.CategoryID == id);
            if (category == null) return NotFound(new { message = "Category does not exist." });
            if (_store.EventCategories.Any(ec => ec.CategoryID == id))
                return BadRequest(new { message = "Category is being used by an event." });
            _store.Categories.Remove(category);
            return NoContent();
        }
    }
}