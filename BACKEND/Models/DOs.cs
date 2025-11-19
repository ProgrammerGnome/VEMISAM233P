using System.ComponentModel.DataAnnotations;

namespace ProjectName.Models.DOs
{
    public class SolutionDo
    {
        [Required] public string CodeString { get; set; } = string.Empty; 
        public string FileName { get; set; } = string.Empty;
        public string EnvironmentDetails { get; set; } = string.Empty;
    }

    public record SolutionSubmissionDo
    {
        [Required] public int TestId { get; init; }
        [Required] public string NeptunCode { get; init; } = string.Empty;
        [Required] public IFormFile CodeFile { get; init; } = null!;
    }
}