using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<VillaAPIController> _logger;

        public VillaAPIController(ILogger<VillaAPIController> logger, ApplicationDbContext db)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet(Name="GetVillas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Villa>> GetVillas()
        {
            var villaList = _db.Villas.ToList();
            return Ok(villaList);
        }
        [HttpGet("{id:int}", Name ="GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Villa> GetVilla(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            var villa = _db.Villas.FirstOrDefault(x => x.Id == id);
            if (villa == null)
            {
                return NotFound();
            }
            return Ok(villa);
        }
        [HttpPost(Name="CreateVilla")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public ActionResult<Villa> CreateVilla([FromBody]VillaDTO villaDTO)
        {
            if (villaDTO == null)
            {
                return BadRequest(villaDTO);
            }
            if (villaDTO.Id != 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            if (_db.Villas.FirstOrDefault(x => x.Name == villaDTO.Name) != null)
            {
                ModelState.AddModelError("DuplicateCreation", "Villa already exists!");
                return BadRequest(ModelState);
            }
            var villa = new Villa
            {
                Name = villaDTO.Name,
                Details = villaDTO.Details,
                Rate = villaDTO.Rate,
                Sqft = villaDTO.Sqft,
                Occupancy = villaDTO.Occupancy,
                ImageUrl = villaDTO.ImageUrl,
                Amenity = villaDTO.Amenity,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
            };
            _db.Villas.Add(villa);
            _db.SaveChanges();
            return CreatedAtRoute("GetVilla", new { id = villa.Id }, villa);
        }
        [HttpDelete("{id:int}", Name="DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteVilla(int id)
        {
            if (id==0)
            {
                return BadRequest();
            }
            var villa = _db.Villas.FirstOrDefault(x => x.Id == id);
            if (villa == null)
            {
                return NotFound();
            }
            _db.Villas.Remove(villa);
            _db.SaveChanges();
            return NoContent();
        }
        [HttpPut("{id:int}", Name="UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateVilla(int id, [FromBody]VillaDTO villaDTO)
        {
            if (id==0 || villaDTO == null || id != villaDTO.Id)
            {
                return BadRequest();
            }
            var villa = _db.Villas.AsNoTracking().FirstOrDefault(x => x.Id == id);
            if (villa == null)
            {
                return NotFound();
            }
            var model = new Villa
            {
                Id = id,
                Name = villaDTO.Name,
                Details = villaDTO.Details,
                Rate = villaDTO.Rate,
                Sqft = villaDTO.Sqft,
                Occupancy = villaDTO.Occupancy,
                ImageUrl = villaDTO.ImageUrl,
                Amenity = villaDTO.Amenity,
                CreatedDate = villa.CreatedDate,
                UpdatedDate = DateTime.Now,
            };
            _db.Villas.Update(model);
            _db.SaveChanges();
            return NoContent();
        }
    }
}
