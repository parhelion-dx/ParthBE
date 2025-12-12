using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LocationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LocationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<LocationDto>>> GetAll()
        {
            var locations = await _context.Locations
                .Select(l => new LocationDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Description = l.Description
                })
                .ToListAsync();

            return Ok(locations);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<LocationDto>> Create(CreateLocationDto dto)
        {
            var location = new Location
            {
                Name = dto.Name,
                Description = dto.Description
            };

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return Ok(new LocationDto
            {
                Id = location.Id,
                Name = location.Name,
                Description = location.Description
            });
        }
    }
}
