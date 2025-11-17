using System.ComponentModel.DataAnnotations;

namespace ProjectName.Models.DTOs
{
    public record TestParamsDto
    {
        [Required] public string TargyId { get; init; } = string.Empty;
        [Required] public string Temakor { get; init; } = string.Empty;
        public string FeladatTipus { get; init; } = "programozás";
        public string ProgNyelv { get; init; } = "C#";
        public int MaxPont { get; init; } = 10;
        public int FeladatDb { get; init; } = 1;
        public string NehezsegiSzint { get; init; } = "közepes";
    }

    public class BekuldottMegoldasContent // <<< ÁTNEVEZTÜK a SubmissionData helyett, hogy ne legyen zavar
    {
        [Required]
        public string Code { get; set; } = string.Empty; 
        
        public string FileName { get; set; } = string.Empty;
        public string EnvironmentDetails { get; set; } = string.Empty;
    }

    public record SolutionSubmitFormDto
    {
        [Required] public int ZhId { get; init; }
        [Required] public string NeptunKod { get; init; } = string.Empty;
        
        [Required] 
        public IFormFile CodeFile { get; init; } = null!; // A feltöltött fájl
    }

    public record SubmissionData
    {
        [Required] 
        public int ZhId { get; init; }
        
        [Required] 
        public string NeptunKod { get; init; } = string.Empty;
        
        [Required] 
        public IFormFile CodeFile { get; init; } = null!;
    }

    // --- Megoldás Beküldés (Controller/Service) ---
    public record SolutionSubmitDto
    {
        [Required] public int ZhId { get; init; }
        [Required] public string NeptunKod { get; init; } = string.Empty;
        [Required] public SubmissionData BekuldottMegoldas { get; init; } = new SubmissionData();
    }

    // --- Kiértékelés (Controller/Service) ---
    public record CorrectionParamsDto
    {
        [Required] public int FeltoltesId { get; init; } 
        [Required] public string PontozasiRendszer { get; init; } = string.Empty;
        [Required] public string? Temakor { get; init; } = string.Empty;
        public string? ProgNyelv { get; init; }
        public int? MaxPont { get; init; }
        public string? Mintamegoldas { get; init; }
        [Required] public string? PontozasiSzempontok { get; init; } = string.Empty;
        [Required] public string? FeladatLeiras { get; init; } = string.Empty;
    }

    // --- Gemini Válasz DTO ---
    public record CorrectionResultDto
    {
        public int Pont { get; init; }
        public string Ertekeles { get; init; } = string.Empty;
    }
    
    // --- Prompt Frissítés ---
    public record PromptUpdateDto
    {
        [Required] public string SablonNev { get; init; } = string.Empty;
        [Required] public string PromptSzoveg { get; init; } = string.Empty;
    }

    // --- DTO a külső teszt feltöltéshez ---
    public record ExternalTestUploadDto
    {
        // A feltöltött fájl
        [Required] 
        public IFormFile File { get; init; } = null!;
        
        // A hozzá tartozó metadata (JSON stringként)
        [Required] 
        public string ZhMetadataJson { get; init; } = string.Empty;
    }
}
