using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Interfaces;
using Nutq.Web.DTOs;
using System.Linq;
using System.Threading.Tasks;

namespace Nutq.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VocabularyController : ControllerBase
    {
        private readonly IVocabularyRepository _vocabularyRepo;

        public VocabularyController(IVocabularyRepository vocabularyRepo)
        {
            _vocabularyRepo = vocabularyRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetByCategoryAndDifficulty([FromQuery] string category, [FromQuery] string difficulty)
        {
            if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(difficulty))
                return BadRequest(new { error = "category and difficulty query parameters are required" });

            var vocabs = await _vocabularyRepo.GetByCategoryAndDifficultyLevelAsync(category, difficulty);
            var dtos = vocabs.Select(v => new VocabularyDto
            {
                Id = v.Id,
                WordArabic = v.WordArabic,
                WordEnglish = v.WordEnglish,
                Category = v.Category,
                DifficultyLevelName = v.DifficultyLevel?.Name ?? v.DifficultyLevel?.Level,
                ImageUrl = v.ImageUrl,
                SoundUrl = v.SoundUrl,
                VideoUrl = v.VideoUrl
            }).ToList();

            return Ok(dtos);
        }
    }
}
