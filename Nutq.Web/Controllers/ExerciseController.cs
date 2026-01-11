using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Interfaces;
using Nutq.Web.DTOs;

namespace Nutq.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExerciseController : ControllerBase
    {
        private readonly IExerciseService _exerciseService;

        public ExerciseController(IExerciseService exerciseService)
        {
            _exerciseService = exerciseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var exercises = await _exerciseService.GetAllAsync();

            var dto = exercises.Select(e => new ExerciseDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Category = e.Category,
                Difficulty = e.Difficulty,
                DifficultyId = e.DifficultyId
            });

            return Ok(dto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var e = await _exerciseService.GetByIdAsync(id);
            if (e == null) return NotFound();

            return Ok(new ExerciseDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Category = e.Category,
                Difficulty = e.Difficulty,
                DifficultyId = e.DifficultyId
            });
        }

        [HttpGet("difficulty/{difficultyId}")]
        public async Task<IActionResult> GetByDifficulty(int difficultyId)
        {
            var exercises = await _exerciseService.GetByDifficultyAsync(difficultyId);

            var dto = exercises.Select(e => new ExerciseDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Category = e.Category,
                Difficulty = e.Difficulty,
                DifficultyId = e.DifficultyId
            });

            return Ok(dto);
        }
    }
}
