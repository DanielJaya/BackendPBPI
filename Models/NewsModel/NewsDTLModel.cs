using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BackendPBPI.Models.NewsModel
{
    [Table("NewsDTL")]
    public class NewsDTLModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NewsDTLID { get; set; }

        [Required]
        public int NewsHDRID { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string NewsContent { get; set; }

        [MaxLength(255)]
        public string NewsUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        [ForeignKey("NewsHDRID")]
        public virtual NewsHDRModel NewsHeader { get; set; }
    }
}