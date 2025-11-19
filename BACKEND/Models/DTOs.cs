using System.ComponentModel.DataAnnotations;
using ProjectName.Models.DOs;

namespace ProjectName.Models.DTOs
{
    public record TestParametersDto
    {
        [Required] public string SubjectId { get; init; } = string.Empty;
        [Required] public string TopicName { get; init; } = string.Empty;
        public string TaskTypeName { get; init; } = "programozás";
        public string ProgrammingLanguage { get; init; } = "C#";
        public int MaximumAchievablePoints { get; init; } = 10;
        public int NumberOfTasks { get; init; } = 1;
        public string DifficultyLevelName { get; init; } = "közepes";
    }

    public record SolutionSubmissionFormDto
    {
        [Required] public int TaskId { get; init; }
        [Required] public string NeptunCode { get; init; } = string.Empty;
        [Required] public IFormFile CodeFile { get; init; } = null!; // a feltöltött fájl
    }

    // megoldás beküldés (Controller/Service)
    public record SolutionSubmissionDto
    {
        [Required] public int TestId { get; init; }
        [Required] public string NeptunCode { get; init; } = string.Empty;
        [Required] public SolutionSubmissionDo SolutionSubmissionDo { get; init; } = new SolutionSubmissionDo();
    }

    // kiértékelés (Controller/Service)
    public record CorrectionParametersDto
    {
        [Required] public int uploadId { get; init; } 
        [Required] public string ScoringSystemText { get; init; } = string.Empty;
        [Required] public string? TopicName { get; init; } = string.Empty;
        public string? ProgrammingLanguage { get; init; }
        public int? MaximumAchievablePoints { get; init; }
        public string? SampleSolution { get; init; }
        [Required] public string? ScoringCriteria { get; init; } = string.Empty;
        [Required] public string? TaskDescription { get; init; } = string.Empty;
    }

    // javítási válasz DTO
    public record CorrectionResultDto
    {
        public int AchievedPoints { get; init; }
        public string ResponseText { get; init; } = string.Empty;
    }
    
    // prompt frissítés
    public record PromptUpdateDto
    {
        [Required] public string TemplateName { get; init; } = string.Empty;
        [Required] public string PromptText { get; init; } = string.Empty;
    }

    // DTO a külső teszt feltöltéshez
    public record ExternalTestUploadDto
    {
        [Required] public IFormFile File { get; init; } = null!; // a feltöltött fájl
        [Required] public string TestMetadataJson { get; init; } = string.Empty; // a feltöltött fájlhoz tartozó metadata (JSON stringként)
    }
}