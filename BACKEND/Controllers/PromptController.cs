using Microsoft.AspNetCore.Mvc;
using ProjectName.Models.DTOs;
using ProjectName.Repositories;
using ProjectName.Models;

namespace ProjectName.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromptController : ControllerBase
    {
        private readonly IPromptRepository _promptRepository;

        public PromptController(IPromptRepository promptRepository)
        {
            _promptRepository = promptRepository;
        }

        // GET /api/prompt/{name}
        [HttpGet("{name}")]
        public async Task<IActionResult> GetPrompt(string name)
        {
            var prompt = await _promptRepository.GetPromptByNameAsync(name);
            if (prompt == null)
            {
                return NotFound($"Prompt sablon ({name}) nem található.");
            }
            return Ok(new PromptUpdateDto { SablonNev = prompt.SablonNev, PromptSzoveg = prompt.PromptSzoveg });
        }

        // PUT /api/prompt
        [HttpPut]
        public async Task<IActionResult> UpdatePrompt([FromBody] PromptUpdateDto dto)
        {
            var prompt = await _promptRepository.GetPromptByNameAsync(dto.SablonNev);
            
            if (prompt == null)
            {
                prompt = new Models.Prompt { SablonNev = dto.SablonNev, PromptSzoveg = dto.PromptSzoveg };
            }
            else
            {
                prompt.PromptSzoveg = dto.PromptSzoveg;
            }
            
            await _promptRepository.UpdatePromptAsync(prompt); 

            return Ok(new { Message = $"'{dto.SablonNev}' prompt sablon sikeresen frissítve/létrehozva." });
        }

        // DELETE /api/prompt/{name}
        [HttpDelete("{name}")]
        public async Task<IActionResult> DeletePrompt(string name)
        {
            var prompt = await _promptRepository.GetPromptByNameAsync(name);
            
            if (prompt == null)
            {
                return NotFound($"Prompt sablon ({name}) nem található. Nem történt törlés.");
            }
            
            await _promptRepository.DeletePromptAsync(prompt.Id);
            
            return Ok(new { Message = $"'{name}' prompt sablon sikeresen törölve." });
        }
    }
}
