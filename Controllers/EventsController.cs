using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IConfiguration _configuration;

        public EventsController(RaceDayDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/events (Public - Anyone can view events)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RaceEvent>>> GetEvents()
        {
            return await _context.Events.Include(e => e.Categories).ToListAsync();
        }

        // GET: api/events/5 (Public)
        [HttpGet("{id}")]
        public async Task<ActionResult<RaceEvent>> GetEvent(int id)
        {
            var raceEvent = await _context.Events
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (raceEvent == null)
                return NotFound("Event not found.");

            return raceEvent;
        }

        // POST: api/events (Organiser ONLY) - FIXED: binds scalar fields, image is now optional
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<RaceEvent>> PostEvent(
            [FromForm] string eventName,
            [FromForm] DateTime eventDate,
            [FromForm] string location,
            [FromForm] string description,
            [FromForm] string eventType,
            IFormFile? bannerImage) // <-- Added ? to make it optional
        {
            string imageUrl = null;

            // 1. Upload the file to Azure Blob Storage ONLY if one was provided
            if (bannerImage != null && bannerImage.Length > 0)
            {
                try
                {
                    var connectionString = _configuration["AzureStorage:ConnectionString"];
                    var containerName = _configuration["AzureStorage:ContainerName"];

                    var blobServiceClient = new BlobServiceClient(connectionString);
                    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                    // Create the container with PUBLIC access so the image URL can be viewed in the browser
                    await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                    // Generate a unique filename so we don't overwrite existing images
                    var blobName = Guid.NewGuid().ToString() + Path.GetExtension(bannerImage.FileName);
                    var blobClient = containerClient.GetBlobClient(blobName);

                    using (var stream = bannerImage.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = bannerImage.ContentType });
                    }

                    imageUrl = blobClient.Uri.ToString(); // Save the public URL
                }
                catch (Exception ex)
                {
                    // Now that binding is safe, this try-catch WILL work and show the real Azure error
                    return BadRequest($"Azure upload failed: {ex.Message}");
                }
            }

            // 2. Build the event manually (navigation properties like Categories are never touched)
            var raceEvent = new RaceEvent
            {
                EventName = eventName,
                EventDate = eventDate,
                Location = location,
                Description = description,
                EventType = eventType,
                BannerImageUrl = imageUrl
            };

            _context.Events.Add(raceEvent);
            await _context.SaveChangesAsync();

            // Returns 201 Created and the URL to get the new event
            return CreatedAtAction(nameof(GetEvent), new { id = raceEvent.EventId }, raceEvent);
        }

        // PUT: api/events/5 (Organiser ONLY)
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> PutEvent(int id, RaceEvent raceEvent)
        {
            if (id != raceEvent.EventId)
                return BadRequest("Event ID mismatch.");

            _context.Entry(raceEvent).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(id))
                    return NotFound("Event not found.");
                else
                    throw;
            }

            return NoContent(); // 204 No Content is the REST standard for a successful update
        }

        // DELETE: api/events/5 (Organiser ONLY)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var raceEvent = await _context.Events.FindAsync(id);
            if (raceEvent == null)
                return NotFound("Event not found.");

            _context.Events.Remove(raceEvent);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper method to check if an event exists
        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}