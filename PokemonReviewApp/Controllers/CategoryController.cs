using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;
using PokemonReviewApp.Repository;

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
        public IActionResult GetCategories()
        {
            var categories = _mapper.Map<List<CategoryDto>>(_categoryRepository.GetCategories());

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(categories);
        }

        [HttpGet("{categoryId}")]
        public IActionResult GetCategory(int categoryId)
        {
            if (!_categoryRepository.CategoryExists(categoryId))
                return NotFound();

            var category = _mapper.Map<CategoryDto>(_categoryRepository.GetCategory(categoryId));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(category);
        }

        [HttpGet("pokemon/{categoryId}")]

        public IActionResult GetPokemonByCategoryID(int categoryId)
        {
            var pokemons = _mapper.Map<List<PokemonDto>>(_categoryRepository.GetPokemonByCategory(categoryId));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(pokemons);
        }

        [HttpPost]

        public IActionResult CreateCategory([FromBody] CategoryDto categoryCreate)
        {
            if (categoryCreate == null)
                return BadRequest(ModelState);
            var category = _categoryRepository.GetCategories().Where(c => c.Name.Trim().ToUpper() == categoryCreate.Name.Trim().ToUpper()).FirstOrDefault();
            if (category != null)
            {
                ModelState.AddModelError("", $"Category {categoryCreate.Name} already exists");
                return StatusCode(422, ModelState);
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var categoryMap = _mapper.Map<Category>(categoryCreate);

            if (!_categoryRepository.CreateCategory(categoryMap))
            {
                ModelState.AddModelError("", $"Something went wrong saving the category {categoryMap.Name}");
                return StatusCode(500, ModelState);
            }

            return Ok("Category Created!");
        }

        [HttpPut("{categoryId}")]
        public IActionResult UpdateCategory(int categoryId, [FromBody] CategoryDto categoryUpdate)
        {
            if (categoryUpdate == null)
                return BadRequest(ModelState);

            if (categoryId != categoryUpdate.Id)
            {
                ModelState.AddModelError("", "Id mismatch");
                return BadRequest(ModelState);
            }

            if (!_categoryRepository.CategoryExists(categoryId))
            {
                ModelState.AddModelError("", "Category does not exist");
                return NotFound();
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = _mapper.Map<Category>(categoryUpdate);

            if (!_categoryRepository.UpdateCategory(category))
            {
                ModelState.AddModelError("", $"Something went wrong updating the category {category.Name}");
                return StatusCode(500, ModelState);
            }

            return Ok("Category Updated!");
        }

        [HttpDelete("{categoryId}")]
        public IActionResult DeleteCategory(int categoryId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_categoryRepository.CategoryExists(categoryId))
            {
                ModelState.AddModelError("", "Category does not exist");
                return NotFound();
            }

            var category = _categoryRepository.GetCategory(categoryId);

            if (!_categoryRepository.DeleteCategory(category))
            {
                ModelState.AddModelError("", $"Something went wrong deleting {category.Name}");
                return StatusCode(500, ModelState);
            }
            return Ok("Category Deleted!");
        }
    }
}
