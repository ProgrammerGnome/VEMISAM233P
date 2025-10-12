using ProjectName.Llm;
using ProjectName.Models.DTOs;
using ProjectName.Repositories;
using System.Text;

namespace ProjectName.Services
{
    public class CorrectionService
    {
        private readonly IGeminiClient _geminiClient;
        private readonly IUploadedSolutionsRepository _solutionRepository;
        private readonly IPromptRepository _promptRepository;

        public CorrectionService(IGeminiClient geminiClient, IUploadedSolutionsRepository solutionRepository, IPromptRepository promptRepository)
        {
            _geminiClient = geminiClient;
            _solutionRepository = solutionRepository;
            _promptRepository = promptRepository;
        }

        // TaskToLlmForCorrection() + Automatikus teszt kiértékelés
        public async Task<CorrectionResultDto> StartCorrection(CorrectionParamsDto dto)
        {
            // 1. Megoldás és ZH adatok lekérése (Feltoltott_megoldasok.hallgato_id + bekuldott_megoldas kiszedése)
            var solution = await _solutionRepository.GetSolutionWithZhAsync(dto.FeltoltesId)
                ?? throw new KeyNotFoundException($"Megoldás (ID: {dto.FeltoltesId}) nem található.");
            
            var zh = solution.Zh;
            
            // Lekérjük a javítási prompt sablont
            var sablon = await _promptRepository.GetPromptByNameAsync("javitas_prompt_template")
                 ?? throw new Exception("A javításhoz szükséges prompt sablon hiányzik!");

            // 2. Prompt létrehozása
            StringBuilder promptBuilder = new StringBuilder(sablon.PromptSzoveg);
            promptBuilder.AppendLine($"\n--- Javítási Paraméterek ---");
            promptBuilder.AppendLine($"Javítási fókusz: {dto.JavitasFokusz}");
            promptBuilder.AppendLine($"Pontozási rendszer: {dto.PontozasiRendszer}");
            promptBuilder.AppendLine($"Maximális pont: {zh.MaxPont}");
            
            promptBuilder.AppendLine("\n--- ZH Feladat Leírása ---");
            promptBuilder.AppendLine(zh.Leiras);
            
            promptBuilder.AppendLine("\n--- Hallgatói Megoldás ---");
            promptBuilder.AppendLine($"Programozási nyelv: {zh.ProgNyelv}");
            promptBuilder.AppendLine(solution.BekuldottMegoldas.Code);
            
            // 3. API hívás (Gemini API)
            var correctionResult = await _geminiClient.CorrectSolutionAsync(promptBuilder.ToString());

            // 4. DB frissítés ("Feltoltott_megoldasok" tábla [pont, ertekeles] mezőivel frissítjük)
            solution.Pont = correctionResult.Pont;
            solution.Ertekeles = correctionResult.Ertekeles;

            await _solutionRepository.UpdateSolutionAsync(solution);
            
            return correctionResult;
        }
    }
}
