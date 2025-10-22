using BackendPBPI.Data;
using BackendPBPI.DTO.RankingDTO;
using BackendPBPI.Interface.IRepository.Ranking;
using BackendPBPI.Models.RankingModel;
using Microsoft.EntityFrameworkCore;

namespace BackendPBPI.Repository.Ranking
{
    public class RankingRepository : IRankingRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RankingRepository> _logger;

        public RankingRepository(AppDbContext context, ILogger<RankingRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============ PLAYER CRUD ============

        public async Task<RankingHDRModel> AddPlayerAsync(RankingHDRModel hdrModel, RankingDTLModel dtlModel)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Add Header
                await _context.RankingHDR.AddAsync(hdrModel);
                await _context.SaveChangesAsync();

                // Add Detail with Header ID
                dtlModel.RankingHDRID = hdrModel.RankingID;
                await _context.RankingDTL.AddAsync(dtlModel);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Player {PlayerName} berhasil ditambahkan dengan ID {RankingID}",
                    hdrModel.PlayerName, hdrModel.RankingID);

                return hdrModel;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error saat menambahkan player {PlayerName}", hdrModel.PlayerName);
                throw;
            }
        }

        public async Task<bool> IsPlayerNameExistsAsync(string playerName, int? excludeRankingId = null)
        {
            var query = _context.RankingHDR
                .Where(p => p.PlayerName.ToLower() == playerName.ToLower() && p.DeletedAt == null);

            if (excludeRankingId.HasValue)
            {
                query = query.Where(p => p.RankingID != excludeRankingId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<RankingHDRModel> GetPlayerByIdAsync(int rankingId)
        {
            return await _context.RankingHDR
                .Include(h => h.RankingDetail)
                .Include(h => h.RankingFooters)
                .FirstOrDefaultAsync(h => h.RankingID == rankingId && h.DeletedAt == null);
        }

        public async Task<(List<RankingHDRModel> players, int totalCount)> GetPlayersAsync(
            int pageNumber, int pageSize, string searchQuery)
        {
            var query = _context.RankingHDR
                .Where(p => p.DeletedAt == null);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(p =>
                    p.PlayerName.Contains(searchQuery) ||
                    p.PlayerRegions.Contains(searchQuery));
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Get paginated and sorted data
            var players = await query
                .OrderByDescending(p => p.PlayerPoints) // Sort by points descending
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (players, totalCount);
        }

        public async Task<PlayerDetailDTO> GetPlayerDetailAsync(int rankingId)
        {
            var player = await _context.RankingHDR
                .Include(h => h.RankingDetail)
                .Include(h => h.RankingFooters.Where(f => f.DeletedAt == null))
                .FirstOrDefaultAsync(h => h.RankingID == rankingId && h.DeletedAt == null);

            if (player == null)
                return null;

            // Calculate rank
            var rank = await _context.RankingHDR
                .Where(p => p.DeletedAt == null && p.PlayerPoints > player.PlayerPoints)
                .CountAsync() + 1;

            var detail = new PlayerDetailDTO
            {
                RankingID = player.RankingID,
                PlayerName = player.PlayerName,
                PlayerRegions = player.PlayerRegions,
                PlayerPoints = player.PlayerPoints,
                PlayerRank = rank,
                CreatedAt = player.CreatedAt,
                UpdatedAt = player.UpdatedAt
            };

            // Add Detail Info
            if (player.RankingDetail != null)
            {
                detail.PlayerPicBase64 = player.RankingDetail.PlayerPic != null
                    ? Convert.ToBase64String(player.RankingDetail.PlayerPic)
                    : null;
                detail.PlayerGender = player.RankingDetail.PlayerGender;
                detail.PlayerNationality = player.RankingDetail.PlayerNationality;
                detail.PlaceOfBirth = player.RankingDetail.PlaceOfBirth;
                detail.DateOfBirth = player.RankingDetail.DateOfBirth;
            }

            // Add Match Histories
            detail.MatchHistories = player.RankingFooters?
                .OrderByDescending(f => f.MatchDate)
                .Select(f => new MatchHistoryDTO
                {
                    RankingFTRID = f.RankingFTRID,
                    PlayerMatch = f.MatchTitle,
                    MatchDate = f.MatchDate,
                    MatchLevel = f.MatchLevel,
                    MatchResult = f.MatchResult,
                    MatchPoints = f.MatchPoints,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt
                })
                .ToList() ?? new List<MatchHistoryDTO>();

            return detail;
        }

        public async Task UpdatePlayerAsync(RankingHDRModel hdrModel, RankingDTLModel dtlModel)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.RankingHDR.Update(hdrModel);
                _context.RankingDTL.Update(dtlModel);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Player {PlayerName} (ID: {RankingID}) berhasil diupdate",
                    hdrModel.PlayerName, hdrModel.RankingID);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error saat update player ID {RankingID}", hdrModel.RankingID);
                throw;
            }
        }

        public async Task SoftDeletePlayerAsync(int rankingId)
        {
            var player = await _context.RankingHDR.FindAsync(rankingId);
            if (player != null)
            {
                player.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Player {PlayerName} (ID: {RankingID}) berhasil dihapus (soft delete)",
                    player.PlayerName, rankingId);
            }
        }

        // ============ MATCH HISTORY CRUD ============

        public async Task<RankingFTRModel> AddMatchHistoryAsync(RankingFTRModel ftrModel)
        {
            await _context.RankingFTR.AddAsync(ftrModel);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Match history {MatchName} berhasil ditambahkan untuk player ID {RankingID}",
                ftrModel.MatchTitle, ftrModel.RankingHDRID);

            return ftrModel;
        }

        public async Task<RankingFTRModel> GetMatchHistoryByIdAsync(int rankingFTRId)
        {
            return await _context.RankingFTR
                .Include(f => f.RankingHeader)
                .FirstOrDefaultAsync(f => f.RankingFTRID == rankingFTRId && f.DeletedAt == null);
        }

        public async Task UpdateMatchHistoryAsync(RankingFTRModel ftrModel)
        {
            _context.RankingFTR.Update(ftrModel);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Match history ID {RankingFTRID} berhasil diupdate", ftrModel.RankingFTRID);
        }

        public async Task DeleteMatchHistoryAsync(int rankingFTRId)
        {
            var matchHistory = await _context.RankingFTR.FindAsync(rankingFTRId);
            if (matchHistory != null)
            {
                matchHistory.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Match history ID {RankingFTRID} berhasil dihapus", rankingFTRId);
            }
        }

        // ============ POINTS MANAGEMENT ============

        public async Task UpdatePlayerPointsAsync(int rankingId, int pointsToAdd)
        {
            var player = await _context.RankingHDR.FindAsync(rankingId);
            if (player != null)
            {
                player.PlayerPoints += pointsToAdd;
                player.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Player ID {RankingID} points diupdate: {PointsChange} (Total: {TotalPoints})",
                    rankingId, pointsToAdd, player.PlayerPoints);
            }
        }

        public async Task<int> GetPlayerCurrentPointsAsync(int rankingId)
        {
            var player = await _context.RankingHDR.FindAsync(rankingId);
            return player?.PlayerPoints ?? 0;
        }
    }
}