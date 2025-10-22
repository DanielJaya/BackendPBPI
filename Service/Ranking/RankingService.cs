using BackendPBPI.DTO.PagedDTO;
using BackendPBPI.DTO.RankingDTO;
using BackendPBPI.Helper;
using BackendPBPI.Interface.IRepository.Ranking;
using BackendPBPI.Interface.IService.Ranking;
using BackendPBPI.Models.RankingModel;

namespace BackendPBPI.Service.Ranking
{
    public class RankingService : IRankingService
    {
        private readonly IRankingRepository _repository;
        private readonly ILogger<RankingService> _logger;

        public RankingService(IRankingRepository repository, ILogger<RankingService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // ============ PLAYER OPERATIONS ============

        public async Task<PlayerResponseDTO> AddPlayerAsync(AddPlayerRequestDTO request, int userId)
        {
            try
            {
                // Validate player name uniqueness
                if (await _repository.IsPlayerNameExistsAsync(request.PlayerName))
                {
                    throw new InvalidOperationException($"Nama pemain '{request.PlayerName}' sudah terdaftar");
                }

                // Validate and process image if exists
                byte[] playerPicBytes = null;
                if (request.PlayerPic != null)
                {
                    var (isValid, errorMessage) = ImageHelper.ValidateImage(request.PlayerPic);
                    if (!isValid)
                    {
                        throw new InvalidOperationException(errorMessage);
                    }
                    playerPicBytes = await ImageHelper.ConvertToByteArrayAsync(request.PlayerPic);
                }

                // Create Header Model
                var hdrModel = new RankingHDRModel
                {
                    PlayerName = request.PlayerName,
                    PlayerRegions = request.PlayerRegions,
                    PlayerPoints = request.PlayerPoints,
                    UserID = userId,
                    CreatedAt = DateTime.UtcNow
                };

                // Create Detail Model
                var dtlModel = new RankingDTLModel
                {
                    PlayerPic = playerPicBytes,
                    PlayerGender = request.PlayerGender,
                    PlayerNationality = request.PlayerNationality,
                    PlaceOfBirth = request.PlaceOfBirth,
                    DateOfBirth = request.DateOfBirth,
                    CreatedAt = DateTime.UtcNow
                };

                // Add to database
                var result = await _repository.AddPlayerAsync(hdrModel, dtlModel);

                _logger.LogInformation("Player {PlayerName} berhasil ditambahkan oleh User ID {UserId}",
                    request.PlayerName, userId);

                return new PlayerResponseDTO
                {
                    RankingID = result.RankingID,
                    PlayerName = result.PlayerName,
                    PlayerRegions = result.PlayerRegions,
                    PlayerPoints = result.PlayerPoints,
                    Message = "Player berhasil ditambahkan"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menambahkan player {PlayerName}", request.PlayerName);
                throw;
            }
        }

        public async Task<PagedResponseDTO<PlayerListItemDTO>> GetPlayersListAsync(
            int pageNumber, int pageSize, string searchQuery)
        {
            try
            {
                var (players, totalCount) = await _repository.GetPlayersAsync(pageNumber, pageSize, searchQuery);

                // Calculate ranks based on points
                var allPlayerPoints = players
                    .Select(p => p.PlayerPoints)
                    .Distinct()
                    .OrderByDescending(p => p)
                    .ToList();

                var playerListItems = players.Select(player => new PlayerListItemDTO
                {
                    RankingID = player.RankingID,
                    PlayerName = player.PlayerName,
                    PlayerRegions = player.PlayerRegions,
                    PlayerPoints = player.PlayerPoints,
                    PlayerRank = allPlayerPoints.IndexOf(player.PlayerPoints) + 1
                }).ToList();

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                return new PagedResponseDTO<PlayerListItemDTO>
                {
                    Data = playerListItems,
                    TotalRecords = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    HasPreviousPage = pageNumber > 1,
                    HasNextPage = pageNumber < totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil list players");
                throw;
            }
        }

        public async Task<PlayerDetailDTO> GetPlayerDetailAsync(int rankingId)
        {
            try
            {
                var detail = await _repository.GetPlayerDetailAsync(rankingId);

                if (detail == null)
                {
                    throw new KeyNotFoundException($"Player dengan ID {rankingId} tidak ditemukan");
                }

                return detail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil detail player ID {RankingId}", rankingId);
                throw;
            }
        }

        public async Task<PlayerResponseDTO> UpdatePlayerAsync(int rankingId, UpdatePlayerRequestDTO request)
        {
            try
            {
                var existingPlayer = await _repository.GetPlayerByIdAsync(rankingId);
                if (existingPlayer == null)
                {
                    throw new KeyNotFoundException($"Player dengan ID {rankingId} tidak ditemukan");
                }

                // Validate player name uniqueness if changed
                if (!string.IsNullOrWhiteSpace(request.PlayerName) &&
                    request.PlayerName != existingPlayer.PlayerName)
                {
                    if (await _repository.IsPlayerNameExistsAsync(request.PlayerName, rankingId))
                    {
                        throw new InvalidOperationException($"Nama pemain '{request.PlayerName}' sudah terdaftar");
                    }
                }

                // Update Header - Only update if value is provided
                if (!string.IsNullOrWhiteSpace(request.PlayerName))
                    existingPlayer.PlayerName = request.PlayerName;

                if (!string.IsNullOrWhiteSpace(request.PlayerRegions))
                    existingPlayer.PlayerRegions = request.PlayerRegions;

                if (request.PlayerPoints.HasValue)
                    existingPlayer.PlayerPoints = request.PlayerPoints.Value;

                existingPlayer.UpdatedAt = DateTime.UtcNow;

                // Update Detail
                var existingDetail = existingPlayer.RankingDetail;
                if (existingDetail == null)
                {
                    // Create new detail if not exists
                    existingDetail = new RankingDTLModel
                    {
                        RankingHDRID = rankingId,
                        CreatedAt = DateTime.UtcNow
                    };
                }

                // Update detail fields only if provided
                if (request.PlayerPic != null)
                {
                    var (isValid, errorMessage) = ImageHelper.ValidateImage(request.PlayerPic);
                    if (!isValid)
                    {
                        throw new InvalidOperationException(errorMessage);
                    }
                    existingDetail.PlayerPic = await ImageHelper.ConvertToByteArrayAsync(request.PlayerPic);
                }

                if (!string.IsNullOrWhiteSpace(request.PlayerGender))
                    existingDetail.PlayerGender = request.PlayerGender;

                if (!string.IsNullOrWhiteSpace(request.PlayerNationality))
                    existingDetail.PlayerNationality = request.PlayerNationality;

                if (!string.IsNullOrWhiteSpace(request.PlaceOfBirth))
                    existingDetail.PlaceOfBirth = request.PlaceOfBirth;

                if (!string.IsNullOrWhiteSpace(request.DateOfBirth))
                    existingDetail.DateOfBirth = request.DateOfBirth;

                existingDetail.UpdatedAt = DateTime.UtcNow;

                // Save changes
                await _repository.UpdatePlayerAsync(existingPlayer, existingDetail);

                _logger.LogInformation("Player ID {RankingId} berhasil diupdate", rankingId);

                return new PlayerResponseDTO
                {
                    RankingID = existingPlayer.RankingID,
                    PlayerName = existingPlayer.PlayerName,
                    PlayerRegions = existingPlayer.PlayerRegions,
                    PlayerPoints = existingPlayer.PlayerPoints,
                    Message = "Player berhasil diupdate"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat update player ID {RankingId}", rankingId);
                throw;
            }
        }

        public async Task<bool> DeletePlayerAsync(int rankingId)
        {
            try
            {
                var player = await _repository.GetPlayerByIdAsync(rankingId);
                if (player == null)
                {
                    throw new KeyNotFoundException($"Player dengan ID {rankingId} tidak ditemukan");
                }

                await _repository.SoftDeletePlayerAsync(rankingId);

                _logger.LogInformation("Player ID {RankingId} berhasil dihapus (soft delete)", rankingId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menghapus player ID {RankingId}", rankingId);
                throw;
            }
        }

        // ============ MATCH HISTORY OPERATIONS ============

        public async Task<MatchHistoryResponseDTO> AddMatchHistoryAsync(AddMatchHistoryRequestDTO request, int userId)
        {
            try
            {
                // Validate player exists
                var player = await _repository.GetPlayerByIdAsync(request.RankingID);
                if (player == null)
                {
                    throw new KeyNotFoundException($"Player dengan ID {request.RankingID} tidak ditemukan");
                }

                var oldPoints = player.PlayerPoints;

                // Create Match History
                var ftrModel = new RankingFTRModel
                {
                    RankingHDRID = request.RankingID,
                    MatchTitle = request.PlayerMatch,
                    MatchDate = request.MatchDate,
                    MatchLevel = request.MatchLevel,
                    MatchResult = request.MatchResult,
                    MatchPoints = request.MatchPoints,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _repository.AddMatchHistoryAsync(ftrModel);

                // Update player points
                await _repository.UpdatePlayerPointsAsync(request.RankingID, request.MatchPoints);

                var newPoints = oldPoints + request.MatchPoints;

                _logger.LogInformation("Match history ditambahkan untuk Player ID {RankingId}. Points: {OldPoints} -> {NewPoints}",
                    request.RankingID, oldPoints, newPoints);

                return new MatchHistoryResponseDTO
                {
                    RankingFTRID = result.RankingFTRID,
                    RankingID = request.RankingID,
                    PlayerName = player.PlayerName,
                    OldPlayerPoints = oldPoints,
                    NewPlayerPoints = newPoints,
                    MatchPoints = request.MatchPoints,
                    Message = "Match history berhasil ditambahkan dan points player telah diupdate"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menambahkan match history untuk Player ID {RankingId}", request.RankingID);
                throw;
            }
        }

        public async Task<MatchHistoryResponseDTO> UpdateMatchHistoryAsync(
            int rankingFTRId, UpdateMatchHistoryRequestDTO request)
        {
            try
            {
                var existingMatch = await _repository.GetMatchHistoryByIdAsync(rankingFTRId);
                if (existingMatch == null)
                {
                    throw new KeyNotFoundException($"Match history dengan ID {rankingFTRId} tidak ditemukan");
                }

                var player = existingMatch.RankingHeader;
                var oldPlayerPoints = player.PlayerPoints;
                var oldMatchPoints = existingMatch.MatchPoints ?? 0;

                // Update match history fields only if provided
                if (!string.IsNullOrWhiteSpace(request.PlayerMatch))
                    existingMatch.MatchTitle = request.PlayerMatch;

                if (request.MatchDate.HasValue)
                    existingMatch.MatchDate = request.MatchDate;

                if (!string.IsNullOrWhiteSpace(request.MatchLevel))
                    existingMatch.MatchLevel = request.MatchLevel;

                if (!string.IsNullOrWhiteSpace(request.MatchResult))
                    existingMatch.MatchResult = request.MatchResult;

                // Handle points update
                int pointsDifference = 0;
                int newMatchPoints = oldMatchPoints;

                if (request.MatchPoints.HasValue)
                {
                    newMatchPoints = request.MatchPoints.Value;
                    pointsDifference = newMatchPoints - oldMatchPoints;
                    existingMatch.MatchPoints = newMatchPoints;
                }

                existingMatch.UpdatedAt = DateTime.UtcNow;

                // Save match history
                await _repository.UpdateMatchHistoryAsync(existingMatch);

                // Update player points if there's a difference
                if (pointsDifference != 0)
                {
                    await _repository.UpdatePlayerPointsAsync(player.RankingID, pointsDifference);
                }

                var newPlayerPoints = oldPlayerPoints + pointsDifference;

                _logger.LogInformation("Match history ID {RankingFTRId} diupdate. Player points: {OldPoints} -> {NewPoints}",
                    rankingFTRId, oldPlayerPoints, newPlayerPoints);

                return new MatchHistoryResponseDTO
                {
                    RankingFTRID = rankingFTRId,
                    RankingID = player.RankingID,
                    PlayerName = player.PlayerName,
                    OldPlayerPoints = oldPlayerPoints,
                    NewPlayerPoints = newPlayerPoints,
                    MatchPoints = newMatchPoints,
                    Message = pointsDifference != 0
                        ? "Match history berhasil diupdate dan points player telah disesuaikan"
                        : "Match history berhasil diupdate"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat update match history ID {RankingFTRId}", rankingFTRId);
                throw;
            }
        }

        public async Task<bool> DeleteMatchHistoryAsync(int rankingFTRId)
        {
            try
            {
                var matchHistory = await _repository.GetMatchHistoryByIdAsync(rankingFTRId);
                if (matchHistory == null)
                {
                    throw new KeyNotFoundException($"Match history dengan ID {rankingFTRId} tidak ditemukan");
                }

                var matchPoints = matchHistory.MatchPoints ?? 0;
                var playerId = matchHistory.RankingHDRID;

                // Delete match history (soft delete)
                await _repository.DeleteMatchHistoryAsync(rankingFTRId);

                // Adjust player points (subtract the match points)
                if (matchPoints != 0)
                {
                    await _repository.UpdatePlayerPointsAsync(playerId, -matchPoints);
                }

                _logger.LogInformation("Match history ID {RankingFTRId} dihapus. Player ID {PlayerId} points dikurangi {MatchPoints}",
                    rankingFTRId, playerId, matchPoints);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menghapus match history ID {RankingFTRId}", rankingFTRId);
                throw;
            }
        }
    }
}