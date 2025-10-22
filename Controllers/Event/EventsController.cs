using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendPBPI.DTO.EventDTO;
using BackendPBPI.Helper;
using BackendPBPI.Interface.IService.Event;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendPBPI.Controllers.Event
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires JWT authentication
    public class EventsController : ControllerBase
    {
        private readonly IEventsService _eventsService;
        private readonly ILogger<EventsController> _logger;

        public EventsController(IEventsService eventsService, ILogger<EventsController> logger)
        {
            _eventsService = eventsService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new event (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateEvent([FromForm] CreateEventRequestDTO request)
        {
            try
            {
                var userId = JwtHelper.GetUserId(User);
                var userRole = JwtHelper.GetUserRole(User);

                _logger.LogInformation("Create event request received from UserID: {UserID}, Role: {Role}", userId, userRole);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for create event request");
                    return BadRequest(new { message = "Invalid input data", errors = ModelState });
                }

                var result = await _eventsService.CreateEventAsync(request, userId);

                _logger.LogInformation("Event created successfully - EventID: {EventID} by UserID: {UserID}", result.EventID, userId);

                return CreatedAtAction(nameof(GetEventById), new { id = result.EventID }, new
                {
                    success = true,
                    message = "Event created successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in CreateEvent endpoint");
                return StatusCode(500, new { success = false, message = "An error occurred while creating the event", error = ex.Message });
            }
        }

        /// <summary>
        /// Get paginated list of events with search functionality
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetEventsList(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchQuery = null)
        {
            try
            {
                var userId = JwtHelper.GetUserId(User);
                var userRole = JwtHelper.GetUserRole(User);

                _logger.LogInformation("Get events list request from UserID: {UserID}, Role: {Role}, Page: {PageNumber}, PageSize: {PageSize}, Search: {SearchQuery}",
                    userId, userRole, pageNumber, pageSize, searchQuery ?? "None");

                if (pageNumber < 1 || pageSize < 1)
                {
                    _logger.LogWarning("Invalid pagination parameters - PageNumber: {PageNumber}, PageSize: {PageSize}", pageNumber, pageSize);
                    return BadRequest(new { success = false, message = "Page number and page size must be greater than 0" });
                }

                var result = await _eventsService.GetEventsListAsync(pageNumber, pageSize, searchQuery);

                _logger.LogInformation("Events list retrieved successfully - Total records: {TotalRecords}, Current page: {CurrentPage}",
                    result.TotalRecords, result.CurrentPage);

                return Ok(new
                {
                    success = true,
                    message = "Events retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetEventsList endpoint");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving events", error = ex.Message });
            }
        }

        /// <summary>
        /// Get event details by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            try
            {
                var userId = JwtHelper.GetUserId(User);
                var userRole = JwtHelper.GetUserRole(User);

                _logger.LogInformation("Get event by ID request from UserID: {UserID}, Role: {Role}, EventID: {EventID}",
                    userId, userRole, id);

                var result = await _eventsService.GetEventByIdAsync(id);

                _logger.LogInformation("Event details retrieved successfully for EventID: {EventID}", id);

                return Ok(new
                {
                    success = true,
                    message = "Event details retrieved successfully",
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Event not found - EventID: {EventID}, Message: {Message}", id, ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetEventById endpoint for EventID: {EventID}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving event details", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing event (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEvent(int id, [FromForm] UpdateEventRequestDTO request)
        {
            try
            {
                var userId = JwtHelper.GetUserId(User);
                var userRole = JwtHelper.GetUserRole(User);

                _logger.LogInformation("Update event request received from UserID: {UserID}, Role: {Role}, EventID: {EventID}",
                    userId, userRole, id);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for update event request - EventID: {EventID}", id);
                    return BadRequest(new { success = false, message = "Invalid input data", errors = ModelState });
                }

                var result = await _eventsService.UpdateEventAsync(id, request);

                _logger.LogInformation("Event updated successfully - EventID: {EventID} by UserID: {UserID}", id, userId);

                return Ok(new
                {
                    success = true,
                    message = "Event updated successfully",
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Event not found for update - EventID: {EventID}, Message: {Message}", id, ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in UpdateEvent endpoint for EventID: {EventID}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while updating the event", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete an event (Admin only) - Soft delete
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            try
            {
                var userId = JwtHelper.GetUserId(User);
                var userRole = JwtHelper.GetUserRole(User);

                _logger.LogInformation("Delete event request received from UserID: {UserID}, Role: {Role}, EventID: {EventID}",
                    userId, userRole, id);

                var result = await _eventsService.DeleteEventAsync(id);

                if (!result)
                {
                    _logger.LogWarning("Event not found for deletion - EventID: {EventID}", id);
                    return NotFound(new { success = false, message = $"Event with ID {id} not found" });
                }

                _logger.LogInformation("Event deleted successfully - EventID: {EventID} by UserID: {UserID}", id, userId);

                return Ok(new
                {
                    success = true,
                    message = "Event deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in DeleteEvent endpoint for EventID: {EventID}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the event", error = ex.Message });
            }
        }
    }
}