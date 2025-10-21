using System.ComponentModel.DataAnnotations;

namespace BackendPBPI.DTO.NewsDTO
{
    // ============================================
    // Request DTOs
    // ============================================

    public class CreateNewsRequestDto
    {
        [Required(ErrorMessage = "News title is required")]
        [MaxLength(100, ErrorMessage = "News title cannot exceed 100 characters")]
        public string NewsTitle { get; set; }

        [MaxLength(100, ErrorMessage = "News subtitle cannot exceed 100 characters")]
        public string NewsSubTitle { get; set; }

        public int SequenceNo { get; set; }

        public bool Status { get; set; } = true;

        [Required(ErrorMessage = "At least one news detail is required")]
        public List<CreateNewsDTLDto> NewsDetails { get; set; }
    }

    public class CreateNewsDTLDto
    {
        [Required(ErrorMessage = "News content is required")]
        public string NewsContent { get; set; }

        [MaxLength(255, ErrorMessage = "News URL cannot exceed 255 characters")]
        public string NewsUrl { get; set; }
    }

    public class UpdateNewsRequestDto
    {
        [Required(ErrorMessage = "News title is required")]
        [MaxLength(100, ErrorMessage = "News title cannot exceed 100 characters")]
        public string NewsTitle { get; set; }

        [MaxLength(100, ErrorMessage = "News subtitle cannot exceed 100 characters")]
        public string NewsSubTitle { get; set; }

        public int SequenceNo { get; set; }

        public bool Status { get; set; }

        public List<UpdateNewsDTLDto> NewsDetails { get; set; }
    }

    public class UpdateNewsDTLDto
    {
        public int? NewsDTLID { get; set; } // Null jika detail baru

        [Required(ErrorMessage = "News content is required")]
        public string NewsContent { get; set; }

        [MaxLength(255, ErrorMessage = "News URL cannot exceed 255 characters")]
        public string NewsUrl { get; set; }
    }

    // ============================================
    // Response DTOs
    // ============================================

    public class NewsResponseDto
    {
        public int NewsID { get; set; }
        public string NewsTitle { get; set; }
        public string NewsSubTitle { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public int SequenceNo { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<NewsDTLResponseDto> NewsDetails { get; set; }
    }

    public class NewsDTLResponseDto
    {
        public int NewsDTLID { get; set; }
        public int NewsHDRID { get; set; }
        public string NewsContent { get; set; }
        public string NewsUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class NewsListResponseDto
    {
        public int NewsID { get; set; }
        public string NewsTitle { get; set; }
        public string NewsSubTitle { get; set; }
        public string UserName { get; set; }
        public int SequenceNo { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int DetailsCount { get; set; }
    }
}