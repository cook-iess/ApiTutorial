using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;

namespace PokemonReviewApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerController : Controller
    {
        private readonly IOwnerRepositry _ownerRepositry;
        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;

        public OwnerController(IOwnerRepositry ownerRepositry, ICountryRepository countryRepository, IMapper mapper)
        {
            _ownerRepositry = ownerRepositry;
            _countryRepository = countryRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Owner>))]

        public IActionResult GetOwners()
        {
            var owners = _mapper.Map<List<OwnerDto>>(_ownerRepositry.GetOwners());

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(owners);
        }

        [HttpGet("{ownerId}")]
        [ProducesResponseType(200, Type = typeof(Owner))]
        [ProducesResponseType(400)]

        public IActionResult GetOwner(int ownerId)
        {
            if(!_ownerRepositry.OwnerExist(ownerId))
                return NotFound();

            var owner = _mapper.Map<OwnerDto>(_ownerRepositry.GetOwner(ownerId));

            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(owner);
        }

        [HttpGet("{ownerId}/pokemon")]
        [ProducesResponseType(200, Type = typeof(Owner))]
        [ProducesResponseType(400)]

        public IActionResult GetPokemonByOwner(int ownerId)
        {
            if(!_ownerRepositry.OwnerExist(ownerId))
                return NotFound();
            
            var owner = _mapper.Map<List<PokemonDto>>(_ownerRepositry.GetPokemonByOwner(ownerId));

            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(owner);
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]

        public IActionResult CreateOwner([FromQuery] int countryId ,[FromBody]OwnerDto ownerCreate)
        {
            if (ownerCreate == null)
                return BadRequest(ModelState);

            var owner = _ownerRepositry.GetOwners().Where(o => o.FirstName.Trim().ToUpper() == ownerCreate.FirstName.Trim().ToUpper()).FirstOrDefault();

            if (owner != null)
            {
                ModelState.AddModelError("", $"Owner {ownerCreate.FirstName} already exists");
                return StatusCode(422, ModelState);
            }

            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var ownerMap = _mapper.Map<Owner>(ownerCreate);
            ownerMap.Country = _countryRepository.GetCountry(countryId);    

            if (!_ownerRepositry.CreateOwner(ownerMap))
            {
                ModelState.AddModelError("", $"Something went wrong saving {ownerMap.FirstName}");
                return StatusCode(500, ModelState);
            }
            return Ok("Owner Created!");
        }
    }
}
