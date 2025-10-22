using BackendPBPI.Models.UserModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BackendPBPI.Models.RankingModel
{
    [Table("RankingHDR")]
    public class RankingHDRModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RankingID { get; set; }

        [Required]
        [MaxLength(100)]
        public string PlayerName { get; set; }

        [MaxLength(50)]
        public string PlayerRegions { get; set; }

        [Required]
        public int PlayerPoints { get; set; }

        [Required]
        public int UserID { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        [ForeignKey("UserID")]
        public virtual UserModel User { get; set; }

        public virtual RankingDTLModel RankingDetail { get; set; }

        public virtual ICollection<RankingFTRModel> RankingFooters { get; set; }
    }
}
