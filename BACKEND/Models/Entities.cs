using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProjectName.Models.DTOs;

namespace ProjectName.Models
{
    [Table("tests")]
    public class Test
    {
        [Key]
        [Column("id", TypeName = "integer")]
        public int Id { get; set; }

        [Column("title", TypeName = "varchar(255)")]
        public string Title { get; set; } = string.Empty;

        [Column("description", TypeName = "varchar(255)")]
        public string? Description { get; set; }

        [Column("subject_name", TypeName = "varchar(255)")]
        public string SubjectName { get; set; } = string.Empty;
        
        [Column("programming_language", TypeName = "varchar(255)")]
        public string? ProgrammingLanguage { get; set; }
        
        [Column("maximum_achievable_points", TypeName = "integer")]
        public int MaximumAchievablePoints { get; set; }

        [Column("scoring_criteria", TypeName = "varchar(255)")] //consider TypeName = "text"
        public string? ScoringCriteria { get; set; }

        [Column("sample_solution", TypeName = "varchar(255)")] //consider TypeName = "text"
        public string? SampleSolution { get; set; }

        [Column("is_active", TypeName = "integer")]
        public int IsActive { get; set; } = 1;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("tests")]
    public class Test
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Column("description", TypeName = "text")]
        [Required]
        public string Description { get; set; }

        [Column("programming_language")]
        [MaxLength(255)]
        public string? ProgrammingLanguage { get; set; }
        
        [Column("maximum_achievable_points")]
        public int MaximumAchievablePoints { get; set; }

        [Column("scoring_criteria", TypeName = "text")]
        public string? ScoringCriteria { get; set; }

        [Column("sample_solution", TypeName = "text")]
        public string? SampleSolution { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties for related entities
        public virtual ICollection<TestQuestion> Questions { get; set; } = new List<TestQuestion>();
        public virtual ICollection<TestAttempt> Attempts { get; set; } = new List<TestAttempt>();
    }

    [Table("solutions")]
    public class Solution
    {
        [Key]
        [Column("id", TypeName = "integer")]
        public int Id { get; set; }

        [ForeignKey(nameof(TestId))]
        [Column("test_id")]
        public int TestId { get; set; }

        [Column("content", TypeName = "jsonb")]
        public string Content { get; set; } = string.Empty;
        //public BekuldottMegoldasContent BekuldottMegoldas { get; set; } = new BekuldottMegoldasContent();

        [Column("achieved_points", TypeName = "integer")]
        public int? AchievedPoints { get; set; } 

        [Column("response_text", TypeName = "text")]
        public string? ResponseText { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("prompt_templates")]
    public class PromptTemplate
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Column("name")]
        public string Name { get; set; } = string.Empty;
        
        [Column("content", TypeName = "text")]
        public string Content { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}