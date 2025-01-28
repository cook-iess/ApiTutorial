using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;

namespace PokemonReviewApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PokemonController : Controller
    {
        private readonly IPokemonRepository _pokemonRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IMapper _mapper;
        public PokemonController(IPokemonRepository pokemonRepository, IReviewRepository reviewRepository, IMapper mapper)
        {
            _pokemonRepository = pokemonRepository;
            _reviewRepository = reviewRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Pokemon>))]
        public IActionResult GetPokemons()
        {
            var pokemons = _mapper.Map<List<PokemonDto>>(_pokemonRepository.GetPokemons());

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(pokemons);
        }

        [HttpGet("{pokeId}")]
        [ProducesResponseType(200, Type = typeof(Pokemon))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemon(int pokeId)
        {
            if(!_pokemonRepository.PokemonExists(pokeId))
                return NotFound();

            var pokemon = _mapper.Map<PokemonDto>(_pokemonRepository.GetPokemon(pokeId));

            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(pokemon);
        }

        [HttpGet("{pokeId}/rating")]
        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(400)]

        public IActionResult GetPokemonRating(int pokeId)
        {
            if(!_pokemonRepository.PokemonExists(pokeId))
                return NotFound();

            var rating = _pokemonRepository.GetPokemonRating(pokeId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(rating);
        }
        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]

        public IActionResult CreatePokemon([FromQuery] int ownerId, [FromQuery] int catId, [FromBody] PokemonDto pokemonCreate)
        {
            if(pokemonCreate == null)
                return BadRequest(ModelState);

            var pokemos = _pokemonRepository.GetPokemons().Where(p => p.Name.Trim().ToUpper() == pokemonCreate.Name.Trim().ToUpper()).FirstOrDefault();

            if (pokemos != null)
            {
                ModelState.AddModelError("", $"Pokemon {pokemonCreate.Name} already exists");
                return StatusCode(422, ModelState);
            }

            var pokemonMap = _mapper.Map<Pokemon>(pokemonCreate);

            if(!_pokemonRepository.CreatePokemon(ownerId, catId, pokemonMap))
            {
                ModelState.AddModelError("", $"Something went wrong saving the pokemon {pokemonCreate.Name}");
                return StatusCode(500, ModelState);
            }

            return Ok("Pokemon Created!");

        }

        [HttpPut("{pokeId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]

        public IActionResult UpdatePokemon(int pokeId, [FromQuery] int ownerId, [FromQuery] int catId, [FromBody] PokemonDto pokemonUpdate)
        {
            if (pokemonUpdate == null)
            {
                ModelState.AddModelError("", "Pokemon object is null");
                return BadRequest(ModelState);
            }

            if (!_pokemonRepository.PokemonExists(pokeId))
            {
                ModelState.AddModelError("", $"Pokemon {pokemonUpdate.Name} does not exist");
                return StatusCode(404, ModelState);
            }

            if (pokeId != pokemonUpdate.Id)
            {
                ModelState.AddModelError("", "Id mismatch");
                return BadRequest(ModelState);
            }

            var pokemonMap = _mapper.Map<Pokemon>(pokemonUpdate);
            if (!_pokemonRepository.UpdatePokemon(ownerId, catId, pokemonMap))
            {
                ModelState.AddModelError("", $"Something went wrong updating the pokemon {pokemonUpdate.Name}");
                return StatusCode(500, ModelState);
            }
            return Ok("Pokemon Updated!");
        }

        [HttpDelete("{pokeId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]

        public IActionResult DeletePokemon(int pokeId)
        {

            if (!_pokemonRepository.PokemonExists(pokeId))
            {
                ModelState.AddModelError("", $"Pokemon {pokeId} does not exist");
                return StatusCode(404, ModelState);
            }

            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var pokemon = _pokemonRepository.GetPokemon(pokeId);
            var reviwes = _reviewRepository.GetReviewsOfPokemon(pokeId);

            if (!_reviewRepository.DeleteReviews([.. reviwes]))
            {
                ModelState.AddModelError("", $"Something went wrong deleting the reviews for the pokemon {pokemon.Name}");
                return StatusCode(500, ModelState);
            }

            if (!_pokemonRepository.DeletePokeon(pokemon))
            {
                ModelState.AddModelError("", $"Something went wrong deleting the pokemon {pokemon.Name}");
                return StatusCode(500, ModelState);
            }

            return Ok("Pokemon Deleted!");
        }

    }
}
