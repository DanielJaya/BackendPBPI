using BackendPBPI.DTO.NewsDTO;

namespace BackendPBPI.Interface.IService.News
{
    public interface INewsService
    {
        Task<NewsResponseDto> CreateNewsAsync(int userId, CreateNewsRequestDto request);
        Task<NewsResponseDto> GetNewsByIdAsync(int newsId);
        Task<List<NewsListResponseDto>> GetAllNewsAsync(bool includeInactive = false);
        Task<List<NewsListResponseDto>> GetMyNewsAsync(int userId);
        Task<NewsResponseDto> UpdateNewsAsync(int newsId, int userId, UpdateNewsRequestDto request);
        Task<bool> DeleteNewsAsync(int newsId, int userId);
    }
}