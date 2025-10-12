using ProjectName.Models.DTOs;

namespace ProjectName.Llm
{
    public interface IGeminiClient
    {
        Task<string> GenerateTestAsync(string prompt);
        Task<CorrectionResultDto> CorrectSolutionAsync(string prompt);
    }
}
