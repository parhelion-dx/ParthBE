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
    public class SubjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SubjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<SubjectDto>>> GetAll()
        {
            var subjects = await _context.Subjects
                .Select(s => new SubjectDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description
                })
                .ToListAsync();

            return Ok(subjects);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SubjectDto>> Create(CreateSubjectDto dto)
        {
            var subject = new Subject
            {
                Name = dto.Name,
                Description = dto.Description
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            return Ok(new SubjectDto
            {
                Id = subject.Id,
                Name = subject.Name,
                Description = subject.Description
            });
        }
    }
}
