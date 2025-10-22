using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendPBPI.DTO.EventDTO;
using BackendPBPI.DTO.PagedDTO;

namespace BackendPBPI.Interface.IService.Event
{
    public interface IEventsService
    {
        Task<EventDetailResponseDTO> CreateEventAsync(CreateEventRequestDTO request, int userId);
        Task<PagedResponseDTO<EventListResponseDTO>> GetEventsListAsync(int pageNumber, int pageSize, string? searchQuery);
        Task<EventDetailResponseDTO> GetEventByIdAsync(int eventId);
        Task<EventDetailResponseDTO> UpdateEventAsync(int eventId, UpdateEventRequestDTO request);
        Task<bool> DeleteEventAsync(int eventId);
    }
}