using BackendPBPI.DTO.NewsDTO;
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

        public async Task<NewsResponseDto> CreateNewsAsync(int userId, CreateNewsRequestDto request)
        {
            try
            {
                _logger.LogInformation("Memulai proses create news: {NewsTitle} oleh user: {UserId}", request.NewsTitle, userId);

                // Validasi news details tidak kosong
                if (request.NewsDetails == null || !request.NewsDetails.Any())
                {
                    throw new Exception("News harus memiliki minimal 1 detail");
                }

                // Set sequence number jika tidak diisi
                if (request.SequenceNo == 0)
                {
                    request.SequenceNo = await _newsRepository.GetNextSequenceNoAsync();
                }

                // Create news header
                var newsHeader = new NewsHDRModel
                {
                    NewsTitle = request.NewsTitle,
                    NewsSubTitle = request.NewsSubTitle,
                    UserID = userId,
                    SequenceNo = request.SequenceNo,
                    Status = request.Status
                };

                var createdNews = await _newsRepository.CreateNewsAsync(newsHeader);
                _logger.LogInformation("News header berhasil dibuat dengan ID: {NewsID}", createdNews.NewsID);

                // Create news details
                var detailsList = new List<NewsDTLModel>();
                foreach (var detailDto in request.NewsDetails)
                {
                    var detail = new NewsDTLModel
                    {
                        NewsHDRID = createdNews.NewsID,
                        NewsContent = detailDto.NewsContent,
                        NewsUrl = detailDto.NewsUrl
                    };

                    var createdDetail = await _newsRepository.CreateNewsDetailAsync(detail);
                    detailsList.Add(createdDetail);
                }

                _logger.LogInformation("News berhasil dibuat dengan {Count} detail(s)", detailsList.Count);

                // Return response
                return await GetNewsByIdAsync(createdNews.NewsID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat create news: {NewsTitle}", request.NewsTitle);
                throw;
            }
        }

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

        public async Task<List<NewsListResponseDto>> GetMyNewsAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Mengambil news untuk user: {UserId}", userId);

                var newsList = await _newsRepository.GetNewsByUserIdAsync(userId);

                return newsList.Select(n => MapToNewsListResponseDto(n)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil news untuk user: {UserId}", userId);
                throw;
            }
        }

        public async Task<NewsResponseDto> UpdateNewsAsync(int newsId, int userId, UpdateNewsRequestDto request)
        {
            try
            {
                _logger.LogInformation("Update news ID: {NewsID} oleh user: {UserId}", newsId, userId);

                var news = await _newsRepository.GetNewsByIdAsync(newsId);

                if (news == null)
                {
                    _logger.LogWarning("News tidak ditemukan: {NewsID}", newsId);
                    throw new Exception($"News dengan ID {newsId} tidak ditemukan");
                }

                // Validasi ownership (hanya creator yang bisa update)
                if (news.UserID != userId)
                {
                    _logger.LogWarning("User {UserId} tidak memiliki akses untuk update news {NewsID}", userId, newsId);
                    throw new Exception("Anda tidak memiliki akses untuk mengupdate news ini");
                }

                // Update header
                news.NewsTitle = request.NewsTitle;
                news.NewsSubTitle = request.NewsSubTitle;
                news.SequenceNo = request.SequenceNo;
                news.Status = request.Status;

                await _newsRepository.UpdateNewsAsync(news);

                // Update details
                if (request.NewsDetails != null && request.NewsDetails.Any())
                {
                    var existingDetails = await _newsRepository.GetNewsDetailsByHeaderIdAsync(newsId);

                    // Update atau create details
                    foreach (var detailDto in request.NewsDetails)
                    {
                        if (detailDto.NewsDTLID.HasValue)
                        {
                            // Update existing detail
                            var existingDetail = existingDetails.FirstOrDefault(d => d.NewsDTLID == detailDto.NewsDTLID.Value);
                            if (existingDetail != null)
                            {
                                existingDetail.NewsContent = detailDto.NewsContent;
                                existingDetail.NewsUrl = detailDto.NewsUrl;
                                await _newsRepository.UpdateNewsDetailAsync(existingDetail);
                            }
                        }
                        else
                        {
                            // Create new detail
                            var newDetail = new NewsDTLModel
                            {
                                NewsHDRID = newsId,
                                NewsContent = detailDto.NewsContent,
                                NewsUrl = detailDto.NewsUrl
                            };
                            await _newsRepository.CreateNewsDetailAsync(newDetail);
                        }
                    }

                    // Delete details yang tidak ada di request
                    var requestDetailIds = request.NewsDetails
                        .Where(d => d.NewsDTLID.HasValue)
                        .Select(d => d.NewsDTLID.Value)
                        .ToList();

                    var detailsToDelete = existingDetails
                        .Where(d => !requestDetailIds.Contains(d.NewsDTLID))
                        .ToList();

                    foreach (var detail in detailsToDelete)
                    {
                        await _newsRepository.DeleteNewsDetailAsync(detail.NewsDTLID);
                    }
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

        public async Task<bool> DeleteNewsAsync(int newsId, int userId)
        {
            try
            {
                _logger.LogInformation("Delete news ID: {NewsID} oleh user: {UserId}", newsId, userId);

                var news = await _newsRepository.GetNewsByIdAsync(newsId);

                if (news == null)
                {
                    _logger.LogWarning("News tidak ditemukan: {NewsID}", newsId);
                    throw new Exception($"News dengan ID {newsId} tidak ditemukan");
                }

                // Validasi ownership (hanya creator yang bisa delete)
                if (news.UserID != userId)
                {
                    _logger.LogWarning("User {UserId} tidak memiliki akses untuk delete news {NewsID}", userId, newsId);
                    throw new Exception("Anda tidak memiliki akses untuk menghapus news ini");
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
                CreatedAt = news.CreatedAt,
                UpdatedAt = news.UpdatedAt,
                NewsDetails = news.NewsDetails?.Select(d => new NewsDTLResponseDto
                {
                    NewsDTLID = d.NewsDTLID,
                    NewsHDRID = d.NewsHDRID,
                    NewsContent = d.NewsContent,
                    NewsUrl = d.NewsUrl,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt
                }).ToList() ?? new List<NewsDTLResponseDto>()
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
                DetailsCount = news.NewsDetails?.Count ?? 0
            };
        }
    }
}