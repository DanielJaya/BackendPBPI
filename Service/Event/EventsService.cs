using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendPBPI.DTO.EventDTO;
using BackendPBPI.DTO.PagedDTO;
using BackendPBPI.Interface.IRepository.Event;
using BackendPBPI.Interface.IService.Event;
using BackendPBPI.Models.EventsModel;

namespace BackendPBPI.Service.Event
{
    public class EventsService : IEventsService
    {
        private readonly IEventsRepository _eventsRepository;
        private readonly ILogger<EventsService> _logger;

        public EventsService(IEventsRepository eventsRepository, ILogger<EventsService> logger)
        {
            _eventsRepository = eventsRepository;
            _logger = logger;
        }

        public async Task<EventDetailResponseDTO> CreateEventAsync(CreateEventRequestDTO request, int userId)
        {
            try
            {
                _logger.LogInformation("Creating new event with title: {EventTitle} by UserID: {UserID}", request.EventTitle, userId);

                // Prepare HDR
                var eventHDR = new EventsHDRModel
                {
                    EventTitle = request.EventTitle,
                    EventDate = request.EventDate,
                    UserID = userId
                };

                // Handle image upload
                if (request.EventPic != null)
                {
                    using var memoryStream = new MemoryStream();
                    await request.EventPic.CopyToAsync(memoryStream);
                    eventHDR.EventPic = memoryStream.ToArray();
                    eventHDR.EventPicFileName = request.EventPic.FileName;
                    eventHDR.EventPicContentType = request.EventPic.ContentType;

                    _logger.LogInformation("Event image uploaded - FileName: {FileName}, Size: {Size} bytes", 
                        request.EventPic.FileName, memoryStream.Length);
                }

                // Prepare DTL
                var eventDTL = new EventsDTLModel
                {
                    Location = request.Location,
                    LocationURL = request.LocationURL,
                    RegistrationDate = request.RegistrationDate,
                    TimelineEventAndDate = request.TimelineEventAndDate,
                    Category = request.Category,
                    EventLevel = request.EventLevel,
                    RegistrationFee = request.RegistrationFee
                };

                // Prepare FTR
                var eventFTR = new EventsFTRModel
                {
                    AdditionalNotes = request.AdditionalNotes,
                    EventURL = request.EventURL
                };

                // Create event
                var createdEvent = await _eventsRepository.CreateEventAsync(eventHDR, eventDTL, eventFTR);

                _logger.LogInformation("Event created successfully with EventID: {EventID}", createdEvent.EventID);

                // Fetch complete event details
                var eventDetails = await _eventsRepository.GetEventByIdAsync(createdEvent.EventID);
                return MapToEventDetailResponse(eventDetails!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating event with title: {EventTitle}", request.EventTitle);
                throw new Exception($"Failed to create event: {ex.Message}", ex);
            }
        }

        public async Task<PagedResponseDTO<EventListResponseDTO>> GetEventsListAsync(int pageNumber, int pageSize, string? searchQuery)
        {
            try
            {
                _logger.LogInformation("Fetching events list - Page: {PageNumber}, PageSize: {PageSize}, Search: {SearchQuery}", 
                    pageNumber, pageSize, searchQuery ?? "None");

                var (events, totalCount) = await _eventsRepository.GetEventsListAsync(pageNumber, pageSize, searchQuery);

                var eventListResponse = events.Select((e, index) => new EventListResponseDTO
                {
                    No = ((pageNumber - 1) * pageSize) + index + 1,
                    EventID = e.EventID,
                    EventTitle = e.EventTitle,
                    EventDate = e.EventDate,
                    Category = e.EventsDetail?.Category,
                    EventLevel = e.EventsDetail?.EventLevel
                }).ToList();

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                _logger.LogInformation("Events list retrieved - Total: {TotalCount}, CurrentPage: {CurrentPage}, TotalPages: {TotalPages}", 
                    totalCount, pageNumber, totalPages);

                return new PagedResponseDTO<EventListResponseDTO>
                {
                    Data = eventListResponse,
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
                _logger.LogError(ex, "Error occurred while fetching events list");
                throw new Exception($"Failed to retrieve events list: {ex.Message}", ex);
            }
        }

        public async Task<EventDetailResponseDTO> GetEventByIdAsync(int eventId)
        {
            try
            {
                _logger.LogInformation("Fetching event details for EventID: {EventID}", eventId);

                var eventData = await _eventsRepository.GetEventByIdAsync(eventId);
                
                if (eventData == null)
                {
                    _logger.LogWarning("Event not found for EventID: {EventID}", eventId);
                    throw new KeyNotFoundException($"Event with ID {eventId} not found");
                }

                _logger.LogInformation("Event details retrieved successfully for EventID: {EventID}", eventId);

                return MapToEventDetailResponse(eventData);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching event details for EventID: {EventID}", eventId);
                throw new Exception($"Failed to retrieve event details: {ex.Message}", ex);
            }
        }

        public async Task<EventDetailResponseDTO> UpdateEventAsync(int eventId, UpdateEventRequestDTO request)
        {
            try
            {
                _logger.LogInformation("Updating event with EventID: {EventID}", eventId);

                // Get existing event
                var existingEvent = await _eventsRepository.GetEventByIdAsync(eventId);
                if (existingEvent == null)
                {
                    _logger.LogWarning("Event not found for update - EventID: {EventID}", eventId);
                    throw new KeyNotFoundException($"Event with ID {eventId} not found");
                }

                // Update HDR - only changed fields
                bool hdrChanged = false;
                if (request.EventTitle != null && request.EventTitle != existingEvent.EventTitle)
                {
                    existingEvent.EventTitle = request.EventTitle;
                    hdrChanged = true;
                    _logger.LogInformation("Event title updated to: {EventTitle}", request.EventTitle);
                }

                if (request.EventDate.HasValue && request.EventDate.Value != existingEvent.EventDate)
                {
                    existingEvent.EventDate = request.EventDate.Value;
                    hdrChanged = true;
                    _logger.LogInformation("Event date updated to: {EventDate}", request.EventDate);
                }

                // Handle image update
                if (request.EventPic != null)
                {
                    using var memoryStream = new MemoryStream();
                    await request.EventPic.CopyToAsync(memoryStream);
                    existingEvent.EventPic = memoryStream.ToArray();
                    existingEvent.EventPicFileName = request.EventPic.FileName;
                    existingEvent.EventPicContentType = request.EventPic.ContentType;
                    hdrChanged = true;
                    _logger.LogInformation("Event image updated - FileName: {FileName}, Size: {Size} bytes", 
                        request.EventPic.FileName, memoryStream.Length);
                }

                // Update DTL - only changed fields
                EventsDTLModel? eventDTL = null;
                if (existingEvent.EventsDetail != null)
                {
                    bool dtlChanged = false;
                    eventDTL = existingEvent.EventsDetail;

                    if (request.Location != null && request.Location != eventDTL.Location)
                    {
                        eventDTL.Location = request.Location;
                        dtlChanged = true;
                    }

                    if (request.LocationURL != null && request.LocationURL != eventDTL.LocationURL)
                    {
                        eventDTL.LocationURL = request.LocationURL;
                        dtlChanged = true;
                    }

                    if (request.RegistrationDate.HasValue && request.RegistrationDate != eventDTL.RegistrationDate)
                    {
                        eventDTL.RegistrationDate = request.RegistrationDate;
                        dtlChanged = true;
                    }

                    if (request.TimelineEventAndDate != null && request.TimelineEventAndDate != eventDTL.TimelineEventAndDate)
                    {
                        eventDTL.TimelineEventAndDate = request.TimelineEventAndDate;
                        dtlChanged = true;
                    }

                    if (request.Category != null && request.Category != eventDTL.Category)
                    {
                        eventDTL.Category = request.Category;
                        dtlChanged = true;
                    }

                    if (request.EventLevel != null && request.EventLevel != eventDTL.EventLevel)
                    {
                        eventDTL.EventLevel = request.EventLevel;
                        dtlChanged = true;
                    }

                    if (request.RegistrationFee != null && request.RegistrationFee != eventDTL.RegistrationFee)
                    {
                        eventDTL.RegistrationFee = request.RegistrationFee;
                        dtlChanged = true;
                    }

                    if (dtlChanged)
                    {
                        _logger.LogInformation("Event DTL has changes and will be updated");
                    }
                    else
                    {
                        eventDTL = null; // No changes, don't update
                    }
                }

                // Update FTR - only changed fields
                EventsFTRModel? eventFTR = null;
                if (existingEvent.EventsFooter != null)
                {
                    bool ftrChanged = false;
                    eventFTR = existingEvent.EventsFooter;

                    if (request.AdditionalNotes != null && request.AdditionalNotes != eventFTR.AdditionalNotes)
                    {
                        eventFTR.AdditionalNotes = request.AdditionalNotes;
                        ftrChanged = true;
                    }

                    if (request.EventURL != null && request.EventURL != eventFTR.EventURL)
                    {
                        eventFTR.EventURL = request.EventURL;
                        ftrChanged = true;
                    }

                    if (ftrChanged)
                    {
                        _logger.LogInformation("Event FTR has changes and will be updated");
                    }
                    else
                    {
                        eventFTR = null; // No changes, don't update
                    }
                }

                // Perform update
                await _eventsRepository.UpdateEventAsync(existingEvent, eventDTL, eventFTR);

                _logger.LogInformation("Event updated successfully for EventID: {EventID}", eventId);

                // Fetch updated event details
                var updatedEvent = await _eventsRepository.GetEventByIdAsync(eventId);
                return MapToEventDetailResponse(updatedEvent!);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating event with EventID: {EventID}", eventId);
                throw new Exception($"Failed to update event: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteEventAsync(int eventId)
        {
            try
            {
                _logger.LogInformation("Attempting to delete event with EventID: {EventID}", eventId);

                var result = await _eventsRepository.DeleteEventAsync(eventId);

                if (result)
                {
                    _logger.LogInformation("Event deleted successfully - EventID: {EventID}", eventId);
                }
                else
                {
                    _logger.LogWarning("Event deletion failed - EventID not found: {EventID}", eventId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting event with EventID: {EventID}", eventId);
                throw new Exception($"Failed to delete event: {ex.Message}", ex);
            }
        }

        // Helper method to map to response DTO
        private EventDetailResponseDTO MapToEventDetailResponse(EventsHDRModel eventData)
        {
            return new EventDetailResponseDTO
            {
                // HDR
                EventID = eventData.EventID,
                EventTitle = eventData.EventTitle,
                EventDate = eventData.EventDate,
                EventPicBase64 = eventData.EventPic != null ? Convert.ToBase64String(eventData.EventPic) : null,
                EventPicFileName = eventData.EventPicFileName,
                EventPicContentType = eventData.EventPicContentType,
                UserID = eventData.UserID,
                CreatedAt = eventData.CreatedAt,
                UpdatedAt = eventData.UpdatedAt,

                // DTL
                EventDTLID = eventData.EventsDetail?.EventDTLID,
                Location = eventData.EventsDetail?.Location,
                LocationURL = eventData.EventsDetail?.LocationURL,
                RegistrationDate = eventData.EventsDetail?.RegistrationDate,
                TimelineEventAndDate = eventData.EventsDetail?.TimelineEventAndDate,
                Category = eventData.EventsDetail?.Category,
                EventLevel = eventData.EventsDetail?.EventLevel,
                RegistrationFee = eventData.EventsDetail?.RegistrationFee,

                // FTR
                EventFTRID = eventData.EventsFooter?.EventFTRID,
                AdditionalNotes = eventData.EventsFooter?.AdditionalNotes,
                EventURL = eventData.EventsFooter?.EventURL
            };
        }
    }
}