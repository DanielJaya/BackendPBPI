using BackendPBPI.Models.NewsModel;

namespace BackendPBPI.Interface.IRepository.News
{
    public interface INewsRepository
    {
        // NewsHDR Operations
        Task<NewsHDRModel> CreateNewsAsync(NewsHDRModel news);
        Task<NewsHDRModel> GetNewsByIdAsync(int newsId);
        Task<List<NewsHDRModel>> GetAllNewsAsync(bool includeInactive = false);
        Task<List<NewsHDRModel>> GetNewsByUserIdAsync(int userId);
        Task<NewsHDRModel> UpdateNewsAsync(NewsHDRModel news);
        Task<bool> DeleteNewsAsync(int newsId);
        Task<bool> NewsExistsAsync(int newsId);
        Task<int> GetNextSequenceNoAsync();

        // NewsDTL Operations
        Task<NewsDTLModel> CreateNewsDetailAsync(NewsDTLModel newsDetail);
        Task<NewsDTLModel> GetNewsDetailByIdAsync(int detailId);
        Task<List<NewsDTLModel>> GetNewsDetailsByHeaderIdAsync(int newsHdrId);
        Task<NewsDTLModel> UpdateNewsDetailAsync(NewsDTLModel newsDetail);
        Task<bool> DeleteNewsDetailAsync(int detailId);
        Task<bool> DeleteNewsDetailsByHeaderIdAsync(int newsHdrId);
    }
}