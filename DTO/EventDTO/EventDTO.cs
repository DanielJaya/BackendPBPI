using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BackendPBPI.DTO.EventDTO
{
    public class CreateEventRequestDTO
    {
        // HDR Section
        [Required(ErrorMessage = "Event Title is required")]
        [MaxLength(255, ErrorMessage = "Event Title cannot exceed 255 characters")]
        public string EventTitle { get; set; }

        [Required(ErrorMessage = "Event Date is required")]
        public DateTime EventDate { get; set; }

        public IFormFile? EventPic { get; set; }

        // DTL Section
        [MaxLength(255, ErrorMessage = "Location cannot exceed 255 characters")]
        public string? Location { get; set; }

        [MaxLength(255, ErrorMessage = "Location URL cannot exceed 255 characters")]
        public string? LocationURL { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public string? TimelineEventAndDate { get; set; }

        [MaxLength(255, ErrorMessage = "Category cannot exceed 255 characters")]
        public string? Category { get; set; }

        [MaxLength(100, ErrorMessage = "Event Level cannot exceed 100 characters")]
        public string? EventLevel { get; set; }

        [MaxLength(100, ErrorMessage = "Registration Fee cannot exceed 100 characters")]
        public string? RegistrationFee { get; set; }

        // FTR Section
        [MaxLength(255, ErrorMessage = "Additional Notes cannot exceed 255 characters")]
        public string? AdditionalNotes { get; set; }

        [MaxLength(255, ErrorMessage = "Event URL cannot exceed 255 characters")]
        public string? EventURL { get; set; }
    }

    public class UpdateEventRequestDTO
    {
        // HDR Section
        [MaxLength(255, ErrorMessage = "Event Title cannot exceed 255 characters")]
        public string? EventTitle { get; set; }

        public DateTime? EventDate { get; set; }

        public IFormFile? EventPic { get; set; }

        [MaxLength(100, ErrorMessage = "Event Pic FileName cannot exceed 100 characters")]
        public string? EventPicFileName { get; set; }

        // DTL Section
        [MaxLength(255, ErrorMessage = "Location cannot exceed 255 characters")]
        public string? Location { get; set; }

        [MaxLength(255, ErrorMessage = "Location URL cannot exceed 255 characters")]
        public string? LocationURL { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public string? TimelineEventAndDate { get; set; }

        [MaxLength(255, ErrorMessage = "Category cannot exceed 255 characters")]
        public string? Category { get; set; }

        [MaxLength(100, ErrorMessage = "Event Level cannot exceed 100 characters")]
        public string? EventLevel { get; set; }

        [MaxLength(100, ErrorMessage = "Registration Fee cannot exceed 100 characters")]
        public string? RegistrationFee { get; set; }

        // FTR Section
        [MaxLength(255, ErrorMessage = "Additional Notes cannot exceed 255 characters")]
        public string? AdditionalNotes { get; set; }

        [MaxLength(255, ErrorMessage = "Event URL cannot exceed 255 characters")]
        public string? EventURL { get; set; }
    }

    public class EventListResponseDTO
    {
        public int No { get; set; }
        public int EventID { get; set; }
        public string EventTitle { get; set; }
        public DateTime EventDate { get; set; }
        public string? Category { get; set; }
        public string? EventLevel { get; set; }
    }
    
        public class EventDetailResponseDTO
    {
        // HDR Section
        public int EventID { get; set; }
        public string EventTitle { get; set; }
        public DateTime EventDate { get; set; }
        public string? EventPicBase64 { get; set; }
        public string? EventPicFileName { get; set; }
        public string? EventPicContentType { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // DTL Section
        public int? EventDTLID { get; set; }
        public string? Location { get; set; }
        public string? LocationURL { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? TimelineEventAndDate { get; set; }
        public string? Category { get; set; }
        public string? EventLevel { get; set; }
        public string? RegistrationFee { get; set; }

        // FTR Section
        public int? EventFTRID { get; set; }
        public string? AdditionalNotes { get; set; }
        public string? EventURL { get; set; }
    }
}