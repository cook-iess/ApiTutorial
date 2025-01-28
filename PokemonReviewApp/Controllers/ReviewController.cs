using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;

namespace PokemonReviewApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : Controller
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IPokemonRepository _pokemonRepository;
        private readonly IReviewerRepository _reviewerRepository;
        private readonly IMapper _mapper;

        public ReviewController(IReviewRepository reviewRepository, IPokemonRepository pokemonRepository, IReviewerRepository reviewerRepository, IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _pokemonRepository = pokemonRepository;
            _reviewerRepository = reviewerRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Review>))]

        public IActionResult GetReviews()
        {
            var reviews = _mapper.Map<List<ReviewDto>>(_reviewRepository.GetReviews());

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(reviews);
        }

        [HttpGet("{reviewId}")]
        [ProducesResponseType(200, Type = typeof(Review))]
        [ProducesResponseType(400)]

        public IActionResult GetReview(int reviewId)
        {
            if (!_reviewRepository.ReviewExists(reviewId))
                return NotFound();

            var review = _mapper.Map<ReviewDto>(_reviewRepository.GetReview(reviewId));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(review);
        }

        [HttpGet("pokemon/{pokeId}")]
        [ProducesResponseType(200, Type = typeof(Review))]
        [ProducesResponseType(400)]

        public IActionResult GetReviewsForAPokemon(int pokeId)
        {
            if (!_reviewRepository.ReviewExists(pokeId))
                return NotFound();

            var reviews = _mapper.Map<List<ReviewDto>>(_reviewRepository.GetReviewsOfPokemon(pokeId));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(reviews);
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]

        public IActionResult CreateReview([FromQuery] int reviewerId, [FromQuery] int pokeId , [FromBody] ReviewDto reviewCreate)
        {
            if (reviewCreate == null)
                return BadRequest(ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var review = _reviewRepository.GetReviews().Where(r => r.Title.Trim().ToUpper() == reviewCreate.Title.Trim().ToUpper()).FirstOrDefault();
            
            if (review != null)
            {
                ModelState.AddModelError("", $"Review {reviewCreate.Title} already exists");
                return StatusCode(422, ModelState);
            }

            var reviewMap = _mapper.Map<Review>(reviewCreate);
            reviewMap.Pokemon = _pokemonRepository.GetPokemon(pokeId);
            reviewMap.Reviewer = _reviewerRepository.GetReviewer(reviewerId);

            if (!_reviewRepository.CreateReview(reviewMap))
            {
                ModelState.AddModelError("", $"Something went wrong saving the review {reviewCreate.Id}");
                return StatusCode(500, ModelState);
            }

            return Ok("Review Created");
        }

        [HttpPut("{reviewId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]

        public IActionResult UpdateReview(int reviewId, [FromBody] ReviewDto reviewUpdate)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (reviewUpdate == null)
            {
                ModelState.AddModelError("", "Review object is null");
                return BadRequest(ModelState);
            }

            if (!_reviewRepository.ReviewExists(reviewId))
            {
                ModelState.AddModelError("", "Review does not exist");
                return BadRequest(ModelState);
            }

            if (reviewId != reviewUpdate.Id)
            {
                ModelState.AddModelError("", "Review Id's do not match");
                return BadRequest(ModelState);
            }

            //var review = _reviewRepository.GetReviews().Where(r => r.Title.Trim().ToUpper() == reviewUpdate.Title.Trim().ToUpper()).FirstOrDefault();

            //if (review != null)
            //{
            //    ModelState.AddModelError("", $"Review {reviewUpdate.Title} already exists");
            //    return StatusCode(422, ModelState);
            //}

            var reviewMap = _mapper.Map<Review>(reviewUpdate);

            if (!_reviewRepository.UpdateReview(reviewMap))
            {
                ModelState.AddModelError("", $"Something went wrong updating the review {reviewUpdate.Id}");
                return StatusCode(500, ModelState);
            }
            return Ok("Review Updated");
        }

        [HttpDelete("{reviewId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]

        public IActionResult DeleteReview(int reviewId)
        {
            if (!_reviewRepository.ReviewExists(reviewId))
                return NotFound();

            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var review = _reviewRepository.GetReview(reviewId);

            if (!_reviewRepository.DeleteReview(review))
            {
                ModelState.AddModelError("", $"Something went wrong deleting the review {reviewId}");
                return StatusCode(500, ModelState);
            }

            return Ok("Review Deleted!");
        }
    }
}
