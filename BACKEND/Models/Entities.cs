using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProjectName.Models.DTOs;

namespace ProjectName.Models
{
    // A ZH tábla leképzése
    [Table("zh")]
    public class ZH
    {
        [Key]
        [Column("zh_id")]
        public int ZhId { get; set; }

        [Column("targy_id")]
        public string TargyId { get; set; } = string.Empty;
        
        [Column("cim")]
        public string Cim { get; set; } = string.Empty;

        [Column("leiras", TypeName = "text")]
        public string? Leiras { get; set; } 
        
        [Column("prog_nyelv")]
        public string? ProgNyelv { get; set; }
        
        [Column("maxpont")]
        public int MaxPont { get; set; }
    }

    // A Hallgato tábla leképzése
    [Table("hallgato")]
    public class Hallgato
    {
        [Key]
        [Column("neptun_kod")]
        [StringLength(6)]
        public string NeptunKod { get; set; } = string.Empty;

        [Column("nev")]
        public string Nev { get; set; } = string.Empty;
    }

    // A Feltoltott_megoldasok tábla leképzése
    [Table("feltoltott_megoldasok")]
    public class FeltoltottMegoldas
    {
        [Key]
        [Column("feltoltes_id")]
        public int FeltoltesId { get; set; }

        [Column("zh_id")]
        public int ZhId { get; set; }
        [ForeignKey(nameof(ZhId))]
        public ZH Zh { get; set; } = null!; 
        
        [Column("hallgato_id")]
        [StringLength(6)]
        public string HallgatoId { get; set; } = string.Empty;

        [ForeignKey(nameof(HallgatoId))]
        public Hallgato Hallgato { get; set; } = null!;

        [Column("bekuldott_megoldas", TypeName = "jsonb")]
        public BekuldottMegoldasContent BekuldottMegoldas { get; set; } = new BekuldottMegoldasContent();

        [Column("pont")]
        public int? Pont { get; set; } 

        [Column("ertekeles", TypeName = "text")]
        public string? Ertekeles { get; set; } 
    }

    // A Prompt sablonok tárolására
    [Table("prompt_sablonok")]
    public class Prompt
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Column("sablon_nev")]
        public string SablonNev { get; set; } = string.Empty; 
        
        [Column("prompt_szoveg", TypeName = "text")]
        public string PromptSzoveg { get; set; } = string.Empty; 
    }
}