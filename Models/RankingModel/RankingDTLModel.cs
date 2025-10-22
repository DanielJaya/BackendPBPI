using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BackendPBPI.Models.RankingModel
{
    [Table("RankingDTL")]
    public class RankingDTLModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RankingDTLID { get; set; }

        [Required]
        public int RankingHDRID { get; set; }

        [Column(TypeName = "varbinary(max)")]
        public byte[] PlayerPic { get; set; }

        [MaxLength(10)]
        public string PlayerGender { get; set; }

        [MaxLength(50)]
        public string PlayerNationality { get; set; }

        [MaxLength(50)]
        public string PlaceOfBirth { get; set; }

        [MaxLength(50)]
        public string DateOfBirth { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        [ForeignKey("RankingHDRID")]
        public virtual RankingHDRModel RankingHeader { get; set; }
    }
}
