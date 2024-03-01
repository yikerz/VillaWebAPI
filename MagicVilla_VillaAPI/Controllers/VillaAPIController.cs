using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        private readonly IVillaRepository _villaRepo;
        private readonly ILogger<VillaAPIController> _logger;
        private readonly IMapper _mapper;

        public VillaAPIController(ILogger<VillaAPIController> logger, IVillaRepository villaRepo, IMapper mapper)
        {
            _villaRepo = villaRepo;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet(Name="GetVillas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Villa>>> GetVillas()
        {
            var villaList = await _villaRepo.GetAllAsync();
            return Ok(villaList);
        }
        [HttpGet("{id:int}", Name ="GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Villa>> GetVilla(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            var villa = await _villaRepo.GetAsync(x => x.Id == id);
            if (villa == null)
            {
                return NotFound();
            }
            return Ok(villa);
        }
        [HttpPost(Name="CreateVilla")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<Villa>> CreateVilla([FromBody]VillaCreateDTO villaCreatDTO)
        {
            if (villaCreatDTO == null)
            {
                return BadRequest(villaCreatDTO);
            }
            if (await _villaRepo.GetAsync(x => x.Name == villaCreatDTO.Name) != null)
            {
                ModelState.AddModelError("DuplicateCreation", "Villa already exists!");
                return BadRequest(ModelState);
            }
            Villa model = _mapper.Map<Villa>(villaCreatDTO);
            await _villaRepo.CreateAsync(model);
            return CreatedAtRoute("GetVilla", new { id = model.Id }, model);
        }
        [HttpDelete("{id:int}", Name="DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteVilla(int id)
        {
            if (id==0)
            {
                return BadRequest();
            }
            var villa = await _villaRepo.GetAsync(x => x.Id == id);
            if (villa == null)
            {
                return NotFound();
            }
            await _villaRepo.RemoveAsync(villa);
            return NoContent();
        }
        [HttpPut("{id:int}", Name="UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateVilla(int id, [FromBody]VillaUpdateDTO villaUpdateDTO)
        {
            if (id==0 || villaUpdateDTO == null || id != villaUpdateDTO.Id)
            {
                return BadRequest();
            }
            var villa = await _villaRepo.GetAsync(x => x.Id == id, tracked: false);
            if (villa == null)
            {
                return NotFound();
            }
            Villa model = _mapper.Map<Villa>(villaUpdateDTO);
            await _villaRepo.UpdateAsync(model);
            return NoContent();
        }
    }
}
