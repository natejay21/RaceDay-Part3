using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests.Controllers
{
    public class EventsControllerTests
    {
        private RaceDayDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RaceDayDbContext(options);
        }

        [Fact]
        public async Task GetEvents_ReturnsAllEvents()
        {
            // Arrange
            var context = CreateInMemoryContext();
            context.Events.Add(new RaceEvent { EventName = "Test Marathon", EventDate = DateTime.Now, Location = "Test", EventType = "Run" });
            await context.SaveChangesAsync();

            // FIX: Added null! for IConfiguration
            var controller = new EventsController(context, null!);

            // Act
            var result = await controller.GetEvents();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var events = Assert.IsType<List<RaceEvent>>(okResult.Value);
            Assert.Single(events);
        }

        [Fact]
        public async Task PostEvent_AddsEventToDatabase()
        {
            // Arrange
            var context = CreateInMemoryContext();

            // FIX: Added null! for IConfiguration
            var controller = new EventsController(context, null!);

            // Act
            // FIX: Pass individual scalar fields to match the new PostEvent signature
            var result = await controller.PostEvent(
                eventName: "New Event",
                eventDate: DateTime.Now,
                location: "Test",
                description: "A test event",
                eventType: "Cycle",
                bannerImage: null);

            // Assert
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetEvent", createdAtResult.ActionName);

            // Verify it's actually in the database
            var eventsInDb = await context.Events.ToListAsync();
            Assert.Single(eventsInDb);
        }
    }
}