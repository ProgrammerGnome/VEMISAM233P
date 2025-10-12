using ProjectName.Llm;
using ProjectName.Models;
using ProjectName.Models.DTOs;
using ProjectName.Repositories;
using System.Text.Json;

namespace ProjectName.Services
{
    public class TestGeneratorService
    {
        private readonly IGeminiClient _geminiClient;
        private readonly IPromptRepository _promptRepository;
        private readonly IZhRepository _zhRepository;

        public TestGeneratorService(IGeminiClient geminiClient, IPromptRepository promptRepository, IZhRepository zhRepository)
        {
            _geminiClient = geminiClient;
            _promptRepository = promptRepository;
            _zhRepository = zhRepository;
        }

        public async Task<int> GenerateAndSaveTest(TestParamsDto dto)
        {
            var sablon = await _promptRepository.GetPromptByNameAsync("programozas_zh_generalo") 
                ?? throw new Exception("A prompt sablon (programozas_zh_generalo) hiányzik a DB-ből!");
            
            string teljesPrompt = sablon.PromptSzoveg
                .Replace("{temakor}", dto.Temakor)
                .Replace("{feladat_tipus}", dto.FeladatTipus)
                .Replace("{prog_nyelv}", dto.ProgNyelv)
                .Replace("{max_pont}", dto.MaxPont.ToString());
            
            string jsonResponse = await _geminiClient.GenerateTestAsync(teljesPrompt);

            var zhData = JsonSerializer.Deserialize<ZH>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new Exception("A Gemini API-tól kapott válasz nem volt értelmezhető ZH-ként.");
            
            zhData.TargyId = dto.TargyId;
            
            await _zhRepository.AddZhAsync(zhData);
            
            return zhData.ZhId;
        }

        public async Task<int> SaveUploadedTestAsync(ExternalTestUploadDto dto)
        {
            ZH? zhEntity = JsonSerializer.Deserialize<ZH>(dto.ZhMetadataJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (zhEntity == null)
            {
                throw new JsonException("A metadata JSON nem deszerializálható érvényes ZH entitássá.");
            }
            
            // TODO
            // A valóságban itt menteném el a fizikai fájlt (dto.File) egy tárhelyre.
            // A ZH entitás Leiras mezőjét frissítjük a fájlra való hivatkozással
            zhEntity.Leiras = $"***KÜLSŐ FELTÖLTÉS FÁJL: {dto.File.FileName} ({dto.File.ContentType})***\n\n" + 
                              (zhEntity.Leiras ?? "Nincs további leírás megadva.");
            
            zhEntity.ZhId = 0; 
            
            await _zhRepository.AddZhAsync(zhEntity);

            return zhEntity.ZhId;
        }
    }
}
