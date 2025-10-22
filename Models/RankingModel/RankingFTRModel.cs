using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BackendPBPI.Models.RankingModel
{
    [Table("RankingFTR")]
    public class RankingFTRModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RankingFTRID { get; set; }

        [Required]
        public int RankingHDRID { get; set; }

        [MaxLength(100)]
        public string MatchTitle { get; set; }

        public DateTime? MatchDate { get; set; }

        [MaxLength(25)]
        public string MatchLevel { get; set; }

        [MaxLength(50)]
        public string MatchResult { get; set; }

        public int? MatchPoints { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        [ForeignKey("RankingHDRID")]
        public virtual RankingHDRModel RankingHeader { get; set; }
    }
}