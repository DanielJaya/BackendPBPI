using BackendPBPI.DTO.RankingDTO;
using BackendPBPI.Models.RankingModel;

namespace BackendPBPI.Interface.IRepository.Ranking
{
    public interface IRankingRepository
    {
        // Player CRUD
        Task<RankingHDRModel> AddPlayerAsync(RankingHDRModel hdrModel, RankingDTLModel dtlModel);
        Task<bool> IsPlayerNameExistsAsync(string playerName, int? excludeRankingId = null);
        Task<RankingHDRModel> GetPlayerByIdAsync(int rankingId);
        Task<(List<RankingHDRModel> players, int totalCount)> GetPlayersAsync(int pageNumber, int pageSize, string searchQuery);
        Task<PlayerDetailDTO> GetPlayerDetailAsync(int rankingId);
        Task UpdatePlayerAsync(RankingHDRModel hdrModel, RankingDTLModel dtlModel);
        Task SoftDeletePlayerAsync(int rankingId);

        // Match History CRUD
        Task<RankingFTRModel> AddMatchHistoryAsync(RankingFTRModel ftrModel);
        Task<RankingFTRModel> GetMatchHistoryByIdAsync(int rankingFTRId);
        Task UpdateMatchHistoryAsync(RankingFTRModel ftrModel);
        Task DeleteMatchHistoryAsync(int rankingFTRId);

        // Points Management
        Task UpdatePlayerPointsAsync(int rankingId, int pointsToAdd);
        Task<int> GetPlayerCurrentPointsAsync(int rankingId);
    }
}
