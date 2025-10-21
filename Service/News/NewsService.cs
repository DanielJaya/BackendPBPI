using BackendPBPI.DTO.NewsDTO;
using BackendPBPI.Helper;
using BackendPBPI.Interface.IRepository.News;
using BackendPBPI.Interface.IService.News;
using BackendPBPI.Models.NewsModel;

namespace BackendPBPI.Service.News
{
    public class NewsService : INewsService
    {
        private readonly INewsRepository _newsRepository;
        private readonly ILogger<NewsService> _logger;

        public NewsService(INewsRepository newsRepository, ILogger<NewsService> logger)
        {
            _newsRepository = newsRepository;
            _logger = logger;
        }

        /// <summary>
        /// Create News (Wajib dengan gambar)
        /// </summary>
        public async Task<NewsResponseDto> CreateNewsAsync(int userId, CreateNewsRequestDto request, byte[] imageBytes, string fileName, string contentType)
        {
            try
            {
                _logger.LogInformation("Memulai proses create news dengan gambar: {NewsTitle}", request.NewsTitle);

                // Validasi news detail tidak null
                if (request.NewsDetail == null)
                {
                    throw new Exception("News harus memiliki detail");
                }

                // Validasi gambar tidak boleh kosong
                if (imageBytes == null || imageBytes.Length == 0)
                {
                    throw new Exception("Gambar wajib diupload saat membuat news");
                }

                // Set sequence number jika tidak diisi
                if (request.SequenceNo == 0)
                {
                    request.SequenceNo = await _newsRepository.GetNextSequenceNoAsync();
                }

                // Create news header dengan gambar
                var newsHeader = new NewsHDRModel
                {
                    NewsTitle = request.NewsTitle,
                    NewsSubTitle = request.NewsSubTitle,
                    UserID = userId,
                    SequenceNo = request.SequenceNo,
                    Status = request.Status,
                    NewsPic = imageBytes,
                    NewsPicFileName = fileName,
                    NewsPicContentType = contentType
                };

                var createdNews = await _newsRepository.CreateNewsAsync(newsHeader);
                _logger.LogInformation("News header berhasil dibuat dengan ID: {NewsID}", createdNews.NewsID);

                // Create news detail (One-to-One)
                var detail = new NewsDTLModel
                {
                    NewsHDRID = createdNews.NewsID,
                    NewsContent = request.NewsDetail.NewsContent,
                    NewsUrl = request.NewsDetail.NewsUrl
                };

                await _newsRepository.CreateNewsDetailAsync(detail);
                _logger.LogInformation("News detail berhasil dibuat untuk NewsID: {NewsID}", createdNews.NewsID);

                return await GetNewsByIdAsync(createdNews.NewsID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat create news dengan gambar");
                throw;
            }
        }

        /// <summary>
        /// Get News By ID
        /// </summary>
        public async Task<NewsResponseDto> GetNewsByIdAsync(int newsId)
        {
            try
            {
                _logger.LogInformation("Mengambil news dengan ID: {NewsID}", newsId);

                var news = await _newsRepository.GetNewsByIdAsync(newsId);

                if (news == null)
                {
                    _logger.LogWarning("News tidak ditemukan: {NewsID}", newsId);
                    throw new Exception($"News dengan ID {newsId} tidak ditemukan");
                }

                return MapToNewsResponseDto(news);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil news: {NewsID}", newsId);
                throw;
            }
        }

        /// <summary>
        /// Get All News
        /// </summary>
        public async Task<List<NewsListResponseDto>> GetAllNewsAsync(bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Mengambil semua news");

                var newsList = await _newsRepository.GetAllNewsAsync(includeInactive);

                return newsList.Select(n => MapToNewsListResponseDto(n)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil semua news");
                throw;
            }
        }

        /// <summary>
        /// Update News - Hanya update field yang berubah
        /// Admin bisa update semua news tanpa cek ownership
        /// </summary>
        public async Task<NewsResponseDto> UpdateNewsAsync(int newsId, UpdateNewsRequestDto request, byte[]? imageBytes = null, string? fileName = null, string? contentType = null)
        {
            try
            {
                _logger.LogInformation("Update news ID: {NewsID}", newsId);

                var news = await _newsRepository.GetNewsByIdAsync(newsId);

                if (news == null)
                {
                    _logger.LogWarning("News tidak ditemukan: {NewsID}", newsId);
                    throw new Exception($"News dengan ID {newsId} tidak ditemukan");
                }

                // Validasi news detail tidak null
                if (request.NewsDetail == null)
                {
                    throw new Exception("News harus memiliki detail");
                }

                // Update header - Hanya field yang berubah
                bool headerChanged = false;

                if (news.NewsTitle != request.NewsTitle)
                {
                    news.NewsTitle = request.NewsTitle;
                    headerChanged = true;
                }

                if (news.NewsSubTitle != request.NewsSubTitle)
                {
                    news.NewsSubTitle = request.NewsSubTitle;
                    headerChanged = true;
                }

                if (news.SequenceNo != request.SequenceNo)
                {
                    news.SequenceNo = request.SequenceNo;
                    headerChanged = true;
                }

                if (news.Status != request.Status)
                {
                    news.Status = request.Status;
                    headerChanged = true;
                }

                // Update gambar jika ada gambar baru
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    news.NewsPic = imageBytes;
                    news.NewsPicFileName = fileName;
                    news.NewsPicContentType = contentType;
                    headerChanged = true;
                    _logger.LogInformation("Gambar news diupdate untuk NewsID: {NewsID}", newsId);
                }

                // Simpan perubahan header jika ada
                if (headerChanged)
                {
                    await _newsRepository.UpdateNewsAsync(news);
                    _logger.LogInformation("News header berhasil diupdate: {NewsID}", newsId);
                }
                else
                {
                    _logger.LogInformation("Tidak ada perubahan pada news header: {NewsID}", newsId);
                }

                // Update detail (One-to-One)
                var existingDetail = await _newsRepository.GetNewsDetailByHeaderIdAsync(newsId);

                if (request.NewsDetail.NewsDTLID.HasValue && existingDetail != null)
                {
                    // Update existing detail - Hanya field yang berubah
                    bool detailChanged = false;

                    if (existingDetail.NewsContent != request.NewsDetail.NewsContent)
                    {
                        existingDetail.NewsContent = request.NewsDetail.NewsContent;
                        detailChanged = true;
                    }

                    if (existingDetail.NewsUrl != request.NewsDetail.NewsUrl)
                    {
                        existingDetail.NewsUrl = request.NewsDetail.NewsUrl;
                        detailChanged = true;
                    }

                    if (detailChanged)
                    {
                        await _newsRepository.UpdateNewsDetailAsync(existingDetail);
                        _logger.LogInformation("News detail berhasil diupdate: {NewsDTLID}", existingDetail.NewsDTLID);
                    }
                    else
                    {
                        _logger.LogInformation("Tidak ada perubahan pada news detail: {NewsDTLID}", existingDetail.NewsDTLID);
                    }
                }
                else
                {
                    // Create new detail jika belum ada atau ID tidak diberikan
                    if (existingDetail != null)
                    {
                        // Hapus detail lama jika ada
                        await _newsRepository.DeleteNewsDetailAsync(existingDetail.NewsDTLID);
                    }

                    var newDetail = new NewsDTLModel
                    {
                        NewsHDRID = newsId,
                        NewsContent = request.NewsDetail.NewsContent,
                        NewsUrl = request.NewsDetail.NewsUrl
                    };
                    await _newsRepository.CreateNewsDetailAsync(newDetail);
                    _logger.LogInformation("News detail baru berhasil dibuat untuk NewsID: {NewsID}", newsId);
                }

                _logger.LogInformation("News berhasil diupdate: {NewsID}", newsId);

                return await GetNewsByIdAsync(newsId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat update news: {NewsID}", newsId);
                throw;
            }
        }

        /// <summary>
        /// Delete News - Admin bisa delete tanpa cek ownership
        /// </summary>
        public async Task<bool> DeleteNewsAsync(int newsId)
        {
            try
            {
                _logger.LogInformation("Delete news ID: {NewsID}", newsId);

                var news = await _newsRepository.GetNewsByIdAsync(newsId);

                if (news == null)
                {
                    _logger.LogWarning("News tidak ditemukan: {NewsID}", newsId);
                    throw new Exception($"News dengan ID {newsId} tidak ditemukan");
                }

                var result = await _newsRepository.DeleteNewsAsync(newsId);
                _logger.LogInformation("News berhasil dihapus: {NewsID}", newsId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat delete news: {NewsID}", newsId);
                throw;
            }
        }

        // ============================================
        // Helper Methods
        // ============================================

        private NewsResponseDto MapToNewsResponseDto(NewsHDRModel news)
        {
            return new NewsResponseDto
            {
                NewsID = news.NewsID,
                NewsTitle = news.NewsTitle,
                NewsSubTitle = news.NewsSubTitle,
                UserID = news.UserID,
                UserName = news.User?.UserName ?? "Unknown",
                SequenceNo = news.SequenceNo,
                Status = news.Status,
                NewsPicBase64 = news.NewsPic != null ? ImageHelper.ConvertToBase64(news.NewsPic) : null,
                NewsPicContentType = news.NewsPicContentType,
                CreatedAt = news.CreatedAt,
                UpdatedAt = news.UpdatedAt,
                NewsDetail = news.NewsDetail != null ? new NewsDTLResponseDto
                {
                    NewsDTLID = news.NewsDetail.NewsDTLID,
                    NewsHDRID = news.NewsDetail.NewsHDRID,
                    NewsContent = news.NewsDetail.NewsContent,
                    NewsUrl = news.NewsDetail.NewsUrl,
                    CreatedAt = news.NewsDetail.CreatedAt,
                    UpdatedAt = news.NewsDetail.UpdatedAt
                } : null
            };
        }

        private NewsListResponseDto MapToNewsListResponseDto(NewsHDRModel news)
        {
            return new NewsListResponseDto
            {
                NewsID = news.NewsID,
                NewsTitle = news.NewsTitle,
                NewsSubTitle = news.NewsSubTitle,
                UserName = news.User?.UserName ?? "Unknown",
                SequenceNo = news.SequenceNo,
                Status = news.Status,
                CreatedAt = news.CreatedAt,
                HasDetail = news.NewsDetail != null
            };
        }
    }
}