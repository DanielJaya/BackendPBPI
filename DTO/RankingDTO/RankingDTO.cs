using System.ComponentModel.DataAnnotations;

namespace BackendPBPI.DTO.RankingDTO
{
    // ============ REQUEST DTOs ============

    /// <summary>
    /// DTO untuk menambah Player baru (HDR + DTL)
    /// </summary>
    public class AddPlayerRequestDTO
    {
        [Required(ErrorMessage = "Nama pemain harus diisi")]
        [MaxLength(100, ErrorMessage = "Nama pemain maksimal 100 karakter")]
        public string PlayerName { get; set; }

        [MaxLength(50, ErrorMessage = "Region maksimal 50 karakter")]
        public string PlayerRegions { get; set; }

        [Required(ErrorMessage = "Points pemain harus diisi")]
        [Range(0, int.MaxValue, ErrorMessage = "Points tidak boleh negatif")]
        public int PlayerPoints { get; set; }

        // Detail Player
        public IFormFile PlayerPic { get; set; }

        [MaxLength(10, ErrorMessage = "Gender maksimal 10 karakter")]
        public string PlayerGender { get; set; }

        [MaxLength(50, ErrorMessage = "Nationality maksimal 50 karakter")]
        public string PlayerNationality { get; set; }

        [MaxLength(50, ErrorMessage = "Tempat lahir maksimal 50 karakter")]
        public string PlaceOfBirth { get; set; }

        [MaxLength(50, ErrorMessage = "Tanggal lahir maksimal 50 karakter")]
        public string DateOfBirth { get; set; }
    }

    /// <summary>
    /// DTO untuk menambah Match History
    /// </summary>
    public class AddMatchHistoryRequestDTO
    {
        [Required(ErrorMessage = "Player ID harus diisi")]
        public int RankingID { get; set; }

        [Required(ErrorMessage = "Nama match harus diisi")]
        [MaxLength(100, ErrorMessage = "Nama match maksimal 100 karakter")]
        public string PlayerMatch { get; set; }

        [Required(ErrorMessage = "Tanggal match harus diisi")]
        public DateTime MatchDate { get; set; }

        [MaxLength(25, ErrorMessage = "Level match maksimal 25 karakter")]
        public string MatchLevel { get; set; }

        [MaxLength(50, ErrorMessage = "Hasil match maksimal 50 karakter")]
        public string MatchResult { get; set; }

        [Required(ErrorMessage = "Match points harus diisi")]
        public int MatchPoints { get; set; } // Bisa minus jika kalah
    }

    /// <summary>
    /// DTO untuk update Player (HDR + DTL) - Partial Update
    /// </summary>
    public class UpdatePlayerRequestDTO
    {
        [MaxLength(100, ErrorMessage = "Nama pemain maksimal 100 karakter")]
        public string PlayerName { get; set; }

        [MaxLength(50, ErrorMessage = "Region maksimal 50 karakter")]
        public string PlayerRegions { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Points tidak boleh negatif")]
        public int? PlayerPoints { get; set; }

        // Detail Player
        public IFormFile PlayerPic { get; set; }

        [MaxLength(10, ErrorMessage = "Gender maksimal 10 karakter")]
        public string PlayerGender { get; set; }

        [MaxLength(50, ErrorMessage = "Nationality maksimal 50 karakter")]
        public string PlayerNationality { get; set; }

        [MaxLength(50, ErrorMessage = "Tempat lahir maksimal 50 karakter")]
        public string PlaceOfBirth { get; set; }

        [MaxLength(50, ErrorMessage = "Tanggal lahir maksimal 50 karakter")]
        public string DateOfBirth { get; set; }
    }

    /// <summary>
    /// DTO untuk update Match History - Partial Update
    /// </summary>
    public class UpdateMatchHistoryRequestDTO
    {
        [MaxLength(100, ErrorMessage = "Nama match maksimal 100 karakter")]
        public string PlayerMatch { get; set; }

        public DateTime? MatchDate { get; set; }

        [MaxLength(25, ErrorMessage = "Level match maksimal 25 karakter")]
        public string MatchLevel { get; set; }

        [MaxLength(50, ErrorMessage = "Hasil match maksimal 50 karakter")]
        public string MatchResult { get; set; }

        public int? MatchPoints { get; set; } // Bisa minus jika kalah
    }

    // ============ RESPONSE DTOs ============

    /// <summary>
    /// DTO untuk List Player (Pagination & Search)
    /// </summary>
    public class PlayerListItemDTO
    {
        public int RankingID { get; set; }
        public int PlayerRank { get; set; } // Dihitung otomatis berdasarkan points
        public string PlayerName { get; set; }
        public string PlayerRegions { get; set; }
        public int PlayerPoints { get; set; }
    }

    /// <summary>
    /// DTO untuk Detail Player lengkap (HDR + DTL + FTR)
    /// </summary>
    public class PlayerDetailDTO
    {
        // Header Info
        public int RankingID { get; set; }
        public string PlayerName { get; set; }
        public string PlayerRegions { get; set; }
        public int PlayerPoints { get; set; }
        public int PlayerRank { get; set; } // Dihitung otomatis

        // Detail Info
        public string PlayerPicBase64 { get; set; } // Base64 string untuk frontend
        public string PlayerGender { get; set; }
        public string PlayerNationality { get; set; }
        public string PlaceOfBirth { get; set; }
        public string DateOfBirth { get; set; }

        // Match History
        public List<MatchHistoryDTO> MatchHistories { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO untuk Match History
    /// </summary>
    public class MatchHistoryDTO
    {
        public int RankingFTRID { get; set; }
        public string PlayerMatch { get; set; }
        public DateTime? MatchDate { get; set; }
        public string MatchLevel { get; set; }
        public string MatchResult { get; set; }
        public int? MatchPoints { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO untuk response Add/Update Player
    /// </summary>
    public class PlayerResponseDTO
    {
        public int RankingID { get; set; }
        public string PlayerName { get; set; }
        public string PlayerRegions { get; set; }
        public int PlayerPoints { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// DTO untuk response Add/Update Match History
    /// </summary>
    public class MatchHistoryResponseDTO
    {
        public int RankingFTRID { get; set; }
        public int RankingID { get; set; }
        public string PlayerName { get; set; }
        public int OldPlayerPoints { get; set; }
        public int NewPlayerPoints { get; set; }
        public int MatchPoints { get; set; }
        public string Message { get; set; }
    }
}