using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendPBPI.Data;
using BackendPBPI.Interface.IRepository.Event;
using BackendPBPI.Models.EventsModel;
using Microsoft.EntityFrameworkCore;

namespace BackendPBPI.Repository.Event
{
    public class EventsRepository : IEventsRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EventsRepository> _logger;

        public EventsRepository(AppDbContext context, ILogger<EventsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<EventsHDRModel> CreateEventAsync(EventsHDRModel eventHDR, EventsDTLModel eventDTL, EventsFTRModel eventFTR)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Starting transaction to create event with title: {EventTitle}", eventHDR.EventTitle);

                // Create HDR
                eventHDR.CreatedAt = DateTime.Now;
                _context.EventsHDR.Add(eventHDR);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Event HDR created with ID: {EventID}", eventHDR.EventID);

                // Create DTL
                eventDTL.EventHDRID = eventHDR.EventID;
                eventDTL.CreatedAt = DateTime.Now;
                _context.EventsDTL.Add(eventDTL);

                // Create FTR
                eventFTR.EventHDRID = eventHDR.EventID;
                eventFTR.CreatedAt = DateTime.Now;
                _context.EventsFTR.Add(eventFTR);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Event created successfully with EventID: {EventID}, DTL ID: {EventDTLID}, FTR ID: {EventFTRID}", 
                    eventHDR.EventID, eventDTL.EventDTLID, eventFTR.EventFTRID);

                return eventHDR;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while creating event with title: {EventTitle}. Transaction rolled back.", eventHDR.EventTitle);
                throw;
            }
        }

        public async Task<(List<EventsHDRModel> Events, int TotalCount)> GetEventsListAsync(int pageNumber, int pageSize, string? searchQuery)
        {
            try
            {
                _logger.LogInformation("Fetching events list - Page: {PageNumber}, PageSize: {PageSize}, SearchQuery: {SearchQuery}", 
                    pageNumber, pageSize, searchQuery ?? "None");

                var query = _context.EventsHDR
                    .Include(e => e.EventsDetail)
                    .Where(e => e.DeletedAt == null)
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    query = query.Where(e => 
                        e.EventTitle.Contains(searchQuery) ||
                        (e.EventsDetail != null && e.EventsDetail.Category != null && e.EventsDetail.Category.Contains(searchQuery)) ||
                        (e.EventsDetail != null && e.EventsDetail.EventLevel != null && e.EventsDetail.EventLevel.Contains(searchQuery))
                    );
                }

                var totalCount = await query.CountAsync();
                _logger.LogInformation("Total events found after filtering: {TotalCount}", totalCount);

                var events = await query
                    .OrderByDescending(e => e.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} events for current page", events.Count);

                return (events, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching events list");
                throw;
            }
        }

        public async Task<EventsHDRModel?> GetEventByIdAsync(int eventId)
        {
            try
            {
                _logger.LogInformation("Fetching event details for EventID: {EventID}", eventId);

                var eventData = await _context.EventsHDR
                    .Include(e => e.EventsDetail)
                    .Include(e => e.EventsFooter)
                    .FirstOrDefaultAsync(e => e.EventID == eventId && e.DeletedAt == null);

                if (eventData == null)
                {
                    _logger.LogWarning("Event not found or deleted for EventID: {EventID}", eventId);
                }
                else
                {
                    _logger.LogInformation("Event details retrieved successfully for EventID: {EventID}", eventId);
                }

                return eventData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching event by ID: {EventID}", eventId);
                throw;
            }
        }

        public async Task<EventsHDRModel> UpdateEventAsync(EventsHDRModel eventHDR, EventsDTLModel? eventDTL, EventsFTRModel? eventFTR)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Starting transaction to update event with EventID: {EventID}", eventHDR.EventID);

                // Update HDR
                eventHDR.UpdatedAt = DateTime.Now;
                _context.EventsHDR.Update(eventHDR);

                // Update DTL if provided
                if (eventDTL != null)
                {
                    eventDTL.UpdatedAt = DateTime.Now;
                    _context.EventsDTL.Update(eventDTL);
                    _logger.LogInformation("Event DTL updated for EventDTLID: {EventDTLID}", eventDTL.EventDTLID);
                }

                // Update FTR if provided
                if (eventFTR != null)
                {
                    eventFTR.UpdatedAt = DateTime.Now;
                    _context.EventsFTR.Update(eventFTR);
                    _logger.LogInformation("Event FTR updated for EventFTRID: {EventFTRID}", eventFTR.EventFTRID);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Event updated successfully for EventID: {EventID}", eventHDR.EventID);

                return eventHDR;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while updating event with EventID: {EventID}. Transaction rolled back.", eventHDR.EventID);
                throw;
            }
        }

        public async Task<bool> DeleteEventAsync(int eventId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Starting soft delete for EventID: {EventID}", eventId);

                var eventHDR = await _context.EventsHDR
                    .Include(e => e.EventsDetail)
                    .Include(e => e.EventsFooter)
                    .FirstOrDefaultAsync(e => e.EventID == eventId && e.DeletedAt == null);

                if (eventHDR == null)
                {
                    _logger.LogWarning("Event not found or already deleted for EventID: {EventID}", eventId);
                    return false;
                }

                var deletedAt = DateTime.Now;

                // Soft delete HDR
                eventHDR.DeletedAt = deletedAt;
                _context.EventsHDR.Update(eventHDR);

                // Soft delete DTL
                if (eventHDR.EventsDetail != null)
                {
                    eventHDR.EventsDetail.DeletedAt = deletedAt;
                    _context.EventsDTL.Update(eventHDR.EventsDetail);
                }

                // Soft delete FTR
                if (eventHDR.EventsFooter != null)
                {
                    eventHDR.EventsFooter.DeletedAt = deletedAt;
                    _context.EventsFTR.Update(eventHDR.EventsFooter);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Event soft deleted successfully for EventID: {EventID}", eventId);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while deleting event with EventID: {EventID}. Transaction rolled back.", eventId);
                throw;
            }
        }

        public async Task<bool> EventExistsAsync(int eventId)
        {
            try
            {
                var exists = await _context.EventsHDR.AnyAsync(e => e.EventID == eventId && e.DeletedAt == null);
                _logger.LogInformation("Event existence check for EventID: {EventID} - Result: {Exists}", eventId, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking event existence for EventID: {EventID}", eventId);
                throw;
            }
        }
    }
}