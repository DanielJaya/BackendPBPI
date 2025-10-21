using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BackendPBPI.Models.EventsModel
{
    [Table("EventsDTL")]
    public class EventsDTLModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventDTLID { get; set; }

        [Required]
        public int EventHDRID { get; set; }

        [MaxLength(255)]
        public string Location { get; set; }

        [MaxLength(255)]
        public string LocationURL { get; set; }

        public DateTime? RegistrationDate { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string TimelineEventAndDate { get; set; }

        [MaxLength(255)]
        public string Category { get; set; }

        [MaxLength(100)]
        public string EventLevel { get; set; }

        [MaxLength(100)]
        public string RegistrationFee { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        [ForeignKey("EventHDRID")]
        public virtual EventsHDRModel EventsHeader { get; set; }
    }
}
