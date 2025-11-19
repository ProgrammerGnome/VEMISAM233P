using Microsoft.AspNetCore.Mvc;
using ProjectName.Models.DTOs;
using ProjectName.Services;
using System.Text.Json;

namespace ProjectName.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase // EGYETLEN OSZTÁLY
    {
        private readonly TestGeneratorService _generatorService;
        
        public TestController(TestGeneratorService generatorService)
        {
            _generatorService = generatorService;
        }

        // POST /api/test/generate
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] TestParametersDto parameters)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            try
            {
                int zhId = await _generatorService.GenerateAndSaveTest(parameters);
                return CreatedAtAction(nameof(Generate), new { ZhId = zhId }, new { Message = "A teszt sikeresen generálva és elmentve." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Hiba a teszt generálása során: {ex.Message}");
            }
        }

        // POST /api/test/upload
        [HttpPost("upload")]
        [Consumes("multipart/form-data")] 
        public async Task<IActionResult> UploadTest([FromForm] ExternalTestUploadDto dto) 
        {
            if (dto.File == null || string.IsNullOrEmpty(dto.ZhMetadataJson)) 
                return BadRequest("Hiányzik a feltöltött fájl vagy a metadata.");

            try
            {
                int zhId = await _generatorService.SaveUploadedTestAsync(dto);

                return Ok(new { 
                    Message = $"A külső teszt sikeresen regisztrálva és elmentve.", 
                    ZhId = zhId 
                });
            }
            catch (JsonException)
            {
                return BadRequest("Érvénytelen formátumú ZhMetadataJson. Kérjük ellenőrizze a JSON szintaxist.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Hiba a teszt feltöltése és mentése során: {ex.Message}");
            }
        }
    } // Az osztály itt zárul, és nincs alatta több definíció!
}