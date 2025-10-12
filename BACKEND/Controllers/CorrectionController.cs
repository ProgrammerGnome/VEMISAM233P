using Microsoft.AspNetCore.Mvc;
using ProjectName.Models.DTOs;
using ProjectName.Services;
using System.IO;

namespace ProjectName.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CorrectionController : ControllerBase
    {
        private readonly SolutionService _solutionService;
        private readonly CorrectionService _correctionService;

        public CorrectionController(SolutionService solutionService, CorrectionService correctionService)
        {
            _solutionService = solutionService;
            _correctionService = correctionService;
        }

        // POST /api/correction/submit
        [HttpPost("submit")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitSolution([FromForm] SolutionSubmitFormDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            if (dto.CodeFile == null || dto.CodeFile.Length == 0)
            {
                return BadRequest("A beküldött programkódot tartalmazó fájl hiányzik vagy üres.");
            }

            try
            {
                int id = await _solutionService.SaveSolutionAsync(dto); 
                
                return Ok(new { 
                    Message = "A megoldás sikeresen beküldve és regisztrálva.", 
                    FeltoltesId = id 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Hiba a megoldás mentése során: {ex.Message}");
            }
        }

        // POST /api/correction/start
        [HttpPost("start")]
        public async Task<IActionResult> StartCorrection([FromBody] CorrectionParamsDto parameters)
        {
            try
            {
                var result = await _correctionService.StartCorrection(parameters);
                return Ok(result);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
            catch (Exception ex)
            {
                 return StatusCode(StatusCodes.Status500InternalServerError, $"Hiba a kiértékelés során: {ex.Message}");
            }
        }
    }
}
