using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using BackendPBPI.Models.UserModels;

namespace BackendPBPI.Models.NewsModel
{
    [Table("NewsHDR")]
    public class NewsHDRModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NewsID { get; set; }

        [Required]
        [MaxLength(100)]
        public string NewsTitle { get; set; }

        [MaxLength(100)]
        public string NewsSubTitle { get; set; }

        [Required]
        public int UserID { get; set; }

        public int SequenceNo { get; set; }

        public bool Status { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        [ForeignKey("UserID")]
        public virtual UserModel User { get; set; }

        public virtual ICollection<NewsDTLModel> NewsDetails { get; set; }
    }
}