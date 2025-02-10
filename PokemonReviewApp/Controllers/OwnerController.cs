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
        public IActionResult GetOwners()
        {
            var owners = _mapper.Map<List<OwnerDto>>(_ownerRepositry.GetOwners());

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(owners);
        }

        [HttpGet("{ownerId}")]
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

        [HttpPut("{ownerId}")]
        public IActionResult UpdateOwner(int ownerId, [FromBody] OwnerDto ownerUpdate)
        {
            if (ownerUpdate == null)
            {
                ModelState.AddModelError("", "Owner object is null");
                return BadRequest(ModelState);
            }

            if (!_ownerRepositry.OwnerExist(ownerId))
            {
                ModelState.AddModelError("", "Owner does not exist");
                return BadRequest(ModelState);
            }

            if (ownerId != ownerUpdate.Id)
            {
                ModelState.AddModelError("", "Id mismatch");
                return BadRequest(ModelState);
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ownerMap = _mapper.Map<Owner>(ownerUpdate);
            if (!_ownerRepositry.UpdateOwner(ownerMap))
            {
                ModelState.AddModelError("", $"Something went wrong updating {ownerMap.FirstName}");
                return StatusCode(500, ModelState);
            }
            return Ok("Owner Updated!");
        }

        [HttpDelete("{ownerId}")]
        public IActionResult DeleteOwner(int ownerId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_ownerRepositry.OwnerExist(ownerId))
                return NotFound();

            var owner = _ownerRepositry.GetOwner(ownerId);

            if (!_ownerRepositry.DeleteOwner(owner))
            {
                ModelState.AddModelError("", $"Something went wrong deleting {owner.FirstName}");
                return StatusCode(500, ModelState);
            }

            return Ok("Owner Deleted!");
        }
    }
}
