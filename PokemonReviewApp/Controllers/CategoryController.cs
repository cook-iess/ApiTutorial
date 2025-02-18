using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;

namespace PokemonReviewApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : Controller
    {
        private readonly ICatagoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        public CategoryController(ICatagoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = _mapper.Map<List<CategoryDto>>(await _categoryRepository.GetCategories());

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(categories);
        }

        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetCategory(int categoryId)
        {
            if (!await _categoryRepository.CategoryExists(categoryId))
                return NotFound();

            var category = _mapper.Map<CategoryDto>(await _categoryRepository.GetCategory(categoryId));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(category);
        }

        [HttpGet("pokemon/{categoryId}")]
        public async Task<IActionResult> GetPokemonByCategoryID(int categoryId)
        {
            var pokemons = _mapper.Map<List<PokemonDto>>(await _categoryRepository.GetPokemonByCategory(categoryId));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(pokemons);
        }

        // ... other code ...

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDto categoryCreate)
        {
            if (categoryCreate == null)
                return BadRequest(ModelState);

            var categories = await _categoryRepository.GetCategories();
            var category = categories.Where(c => string.Equals(c.Name.Trim(), categoryCreate.Name.Trim(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (category != null)
            {
                ModelState.AddModelError("", $"Category {categoryCreate.Name} already exists");
                return StatusCode(422, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var categoryMap = _mapper.Map<Category>(categoryCreate);

            if (!await _categoryRepository.CreateCategory(categoryMap))
            {
                ModelState.AddModelError("", $"Something went wrong saving the category {categoryMap.Name}");
                return StatusCode(500, ModelState);
            }

            return Ok("Category Created!");
        }

        [HttpPut("{categoryId}")]
        public async Task<IActionResult> UpdateCategory(int categoryId, [FromBody] CategoryDto categoryUpdate)
        {
            if (categoryUpdate == null)
                return BadRequest(ModelState);

            if (categoryId != categoryUpdate.Id)
            {
                ModelState.AddModelError("", "Id mismatch");
                return BadRequest(ModelState);
            }

            if (!await _categoryRepository.CategoryExists(categoryId))
            {
                ModelState.AddModelError("", "Category does not exist");
                return NotFound();
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = _mapper.Map<Category>(categoryUpdate);

            if (!await _categoryRepository.UpdateCategory(category))
            {
                ModelState.AddModelError("", $"Something went wrong updating the category {category.Name}");
                return StatusCode(500, ModelState);
            }

            return Ok("Category Updated!");
        }

        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _categoryRepository.CategoryExists(categoryId))
            {
                ModelState.AddModelError("", "Category does not exist");
                return NotFound();
            }

            var category = await _categoryRepository.GetCategory(categoryId);

            if (!await _categoryRepository.DeleteCategory(category))
            {
                ModelState.AddModelError("", $"Something went wrong deleting {category.Name}");
                return StatusCode(500, ModelState);
            }
            return Ok("Category Deleted!");
        }
    }
}
