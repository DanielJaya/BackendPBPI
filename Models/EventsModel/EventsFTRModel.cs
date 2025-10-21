using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BackendPBPI.Models.EventsModel
{
    [Table("EventsFTR")]
    public class EventsFTRModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventFTRID { get; set; }

        [Required]
        public int EventHDRID { get; set; }

        [MaxLength(255)]
        public string AdditionalNotes { get; set; }

        [MaxLength(255)]
        public string EventURL { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        [ForeignKey("EventHDRID")]
        public virtual EventsHDRModel EventsHeader { get; set; }
    }
}
