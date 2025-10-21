using BackendPBPI.Data;
using BackendPBPI.Interface.IRepository.News;
using BackendPBPI.Models.NewsModel;
using Microsoft.EntityFrameworkCore;

namespace BackendPBPI.Repository.News
{
    public class NewsRepository : INewsRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NewsRepository> _logger;

        public NewsRepository(AppDbContext context, ILogger<NewsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // NewsHDR Operations
        // ============================================

        public async Task<NewsHDRModel> CreateNewsAsync(NewsHDRModel news)
        {
            try
            {
                _logger.LogInformation("Membuat news baru: {NewsTitle}", news.NewsTitle);
                news.CreatedAt = DateTime.UtcNow;
                _context.NewsHDR.Add(news);
                await _context.SaveChangesAsync();
                _logger.LogInformation("News berhasil dibuat dengan ID: {NewsID}", news.NewsID);
                return news;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat membuat news: {NewsTitle}", news.NewsTitle);
                throw;
            }
        }

        public async Task<NewsHDRModel> GetNewsByIdAsync(int newsId)
        {
            try
            {
                _logger.LogInformation("Mencari news dengan ID: {NewsID}", newsId);
                return await _context.NewsHDR
                    .Include(n => n.User)
                    .Include(n => n.NewsDetail) // One-to-One
                    .FirstOrDefaultAsync(n => n.NewsID == newsId && n.DeletedAt == null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mencari news dengan ID: {NewsID}", newsId);
                throw;
            }
        }

        public async Task<List<NewsHDRModel>> GetAllNewsAsync(bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Mengambil semua news");
                var query = _context.NewsHDR
                    .Include(n => n.User)
                    .Include(n => n.NewsDetail) // One-to-One
                    .Where(n => n.DeletedAt == null);

                if (!includeInactive)
                {
                    query = query.Where(n => n.Status);
                }

                return await query
                    .OrderByDescending(n => n.SequenceNo)
                    .ThenByDescending(n => n.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil semua news");
                throw;
            }
        }

        public async Task<NewsHDRModel> UpdateNewsAsync(NewsHDRModel news)
        {
            try
            {
                _logger.LogInformation("Mengupdate news ID: {NewsID}", news.NewsID);
                news.UpdatedAt = DateTime.UtcNow;
                _context.NewsHDR.Update(news);
                await _context.SaveChangesAsync();
                _logger.LogInformation("News berhasil diupdate: {NewsID}", news.NewsID);
                return news;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengupdate news: {NewsID}", news.NewsID);
                throw;
            }
        }

        public async Task<bool> DeleteNewsAsync(int newsId)
        {
            try
            {
                _logger.LogInformation("Menghapus news ID: {NewsID}", newsId);
                var news = await GetNewsByIdAsync(newsId);

                if (news == null)
                {
                    _logger.LogWarning("News tidak ditemukan: {NewsID}", newsId);
                    return false;
                }

                // Soft delete header
                news.DeletedAt = DateTime.UtcNow;

                // Soft delete detail (One-to-One)
                if (news.NewsDetail != null)
                {
                    news.NewsDetail.DeletedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("News berhasil dihapus: {NewsID}", newsId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menghapus news: {NewsID}", newsId);
                throw;
            }
        }

        public async Task<bool> NewsExistsAsync(int newsId)
        {
            try
            {
                return await _context.NewsHDR
                    .AnyAsync(n => n.NewsID == newsId && n.DeletedAt == null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengecek news exists");
                throw;
            }
        }

        public async Task<int> GetNextSequenceNoAsync()
        {
            try
            {
                var maxSequence = await _context.NewsHDR
                    .Where(n => n.DeletedAt == null)
                    .MaxAsync(n => (int?)n.SequenceNo) ?? 0;

                return maxSequence + 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mendapatkan next sequence no");
                throw;
            }
        }

        // ============================================
        // NewsDTL Operations (One-to-One)
        // ============================================

        public async Task<NewsDTLModel> CreateNewsDetailAsync(NewsDTLModel newsDetail)
        {
            try
            {
                _logger.LogInformation("Membuat news detail untuk NewsHDRID: {NewsHDRID}", newsDetail.NewsHDRID);
                newsDetail.CreatedAt = DateTime.UtcNow;
                _context.NewsDTL.Add(newsDetail);
                await _context.SaveChangesAsync();
                _logger.LogInformation("News detail berhasil dibuat dengan ID: {NewsDTLID}", newsDetail.NewsDTLID);
                return newsDetail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat membuat news detail");
                throw;
            }
        }

        public async Task<NewsDTLModel> GetNewsDetailByHeaderIdAsync(int newsHdrId)
        {
            try
            {
                _logger.LogInformation("Mencari news detail untuk NewsHDRID: {NewsHDRID}", newsHdrId);
                return await _context.NewsDTL
                    .FirstOrDefaultAsync(d => d.NewsHDRID == newsHdrId && d.DeletedAt == null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mencari news detail untuk NewsHDRID: {NewsHDRID}", newsHdrId);
                throw;
            }
        }

        public async Task<NewsDTLModel> UpdateNewsDetailAsync(NewsDTLModel newsDetail)
        {
            try
            {
                _logger.LogInformation("Mengupdate news detail ID: {NewsDTLID}", newsDetail.NewsDTLID);
                newsDetail.UpdatedAt = DateTime.UtcNow;
                _context.NewsDTL.Update(newsDetail);
                await _context.SaveChangesAsync();
                _logger.LogInformation("News detail berhasil diupdate: {NewsDTLID}", newsDetail.NewsDTLID);
                return newsDetail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengupdate news detail: {NewsDTLID}", newsDetail.NewsDTLID);
                throw;
            }
        }

        public async Task<bool> DeleteNewsDetailAsync(int detailId)
        {
            try
            {
                _logger.LogInformation("Menghapus news detail ID: {NewsDTLID}", detailId);
                var detail = await _context.NewsDTL
                    .FirstOrDefaultAsync(d => d.NewsDTLID == detailId && d.DeletedAt == null);

                if (detail == null)
                {
                    _logger.LogWarning("News detail tidak ditemukan: {NewsDTLID}", detailId);
                    return false;
                }

                detail.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.LogInformation("News detail berhasil dihapus: {NewsDTLID}", detailId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menghapus news detail: {NewsDTLID}", detailId);
                throw;
            }
        }
    }
}