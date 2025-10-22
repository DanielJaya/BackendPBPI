using BackendPBPI.DTO.PagedDTO;
using BackendPBPI.DTO.RankingDTO;

namespace BackendPBPI.Interface.IService.Ranking
{
    public interface IRankingService
    {
        // Player Operations
        Task<PlayerResponseDTO> AddPlayerAsync(AddPlayerRequestDTO request, int userId);
        Task<PagedResponseDTO<PlayerListItemDTO>> GetPlayersListAsync(int pageNumber, int pageSize, string searchQuery);
        Task<PlayerDetailDTO> GetPlayerDetailAsync(int rankingId);
        Task<PlayerResponseDTO> UpdatePlayerAsync(int rankingId, UpdatePlayerRequestDTO request);
        Task<bool> DeletePlayerAsync(int rankingId);

        // Match History Operations
        Task<MatchHistoryResponseDTO> AddMatchHistoryAsync(AddMatchHistoryRequestDTO request, int userId);
        Task<MatchHistoryResponseDTO> UpdateMatchHistoryAsync(int rankingFTRId, UpdateMatchHistoryRequestDTO request);
        Task<bool> DeleteMatchHistoryAsync(int rankingFTRId);
    }
}
