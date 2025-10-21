using BackendPBPI.Models.UserModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BackendPBPI.Models.EventsModel
{
    [Table("EventsHDR")]
    public class EventsHDRModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventID { get; set; }

        [Required]
        [MaxLength(255)]
        public string EventTitle { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Column(TypeName = "varbinary(max)")]
        public byte[] EventPic { get; set; }

        [MaxLength(100)]
        public string EventPicFileName { get; set; }

        [MaxLength(50)]
        public string EventPicContentType { get; set; }

        [Required]
        public int UserID { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        [ForeignKey("UserID")]
        public virtual UserModel User { get; set; }

        public virtual EventsDTLModel EventsDetail { get; set; }

        public virtual EventsFTRModel EventsFooter { get; set; }
    }
}
