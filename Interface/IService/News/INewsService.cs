using BackendPBPI.DTO.NewsDTO;

namespace BackendPBPI.Interface.IService.News
{
    public interface INewsService
    {
        // Create News (Wajib dengan gambar)
        Task<NewsResponseDto> CreateNewsAsync(int userId, CreateNewsRequestDto request, byte[] imageBytes, string fileName, string contentType);

        // Get Operations
        Task<NewsResponseDto> GetNewsByIdAsync(int newsId);
        Task<List<NewsListResponseDto>> GetAllNewsAsync(bool includeInactive = false);

        // Update News (Support update gambar)
        Task<NewsResponseDto> UpdateNewsAsync(int newsId, UpdateNewsRequestDto request, byte[]? imageBytes = null, string? fileName = null, string? contentType = null);

        // Delete News
        Task<bool> DeleteNewsAsync(int newsId);
    }
}