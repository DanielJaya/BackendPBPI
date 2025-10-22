using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendPBPI.Models.EventsModel;

namespace BackendPBPI.Interface.IRepository.Event
{
    public interface IEventsRepository
    {
        Task<EventsHDRModel> CreateEventAsync(EventsHDRModel eventHDR, EventsDTLModel eventDTL, EventsFTRModel eventFTR);
        Task<(List<EventsHDRModel> Events, int TotalCount)> GetEventsListAsync(int pageNumber, int pageSize, string? searchQuery);
        Task<EventsHDRModel?> GetEventByIdAsync(int eventId);
        Task<EventsHDRModel> UpdateEventAsync(EventsHDRModel eventHDR, EventsDTLModel? eventDTL, EventsFTRModel? eventFTR);
        Task<bool> DeleteEventAsync(int eventId);
        Task<bool> EventExistsAsync(int eventId);
    }
}