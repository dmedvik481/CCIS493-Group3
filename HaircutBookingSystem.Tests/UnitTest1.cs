using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HaircutBookingSystem.Models;
using Xunit;

namespace HaircutBookingSystem.Tests
{
    public class BookingRequestTests
    {
        // Helper to run data-annotation + IValidatableObject validation
        private List<ValidationResult> ValidateModel(BookingRequest model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void Booking_WithPastDateAndTime_ShouldHaveFutureError()
        {
            // Arrange
            var booking = new BookingRequest
            {
                ServiceId = 1,
                StylistId = 1,
                Date = DateTime.Today.AddDays(-1),   // yesterday
                Time = new TimeSpan(4, 30, 0),       // 4:30 AM
                FullName = "Test User",
                Email = "test@example.com",
                Phone = "1234567890"
            };

            // Act
            var results = ValidateModel(booking);

            // Assert
            Assert.Contains(results, r =>
                r.ErrorMessage == "The selected date and time must be in the future.");
        }

        [Fact]
        public void Booking_WithFutureDateWithinOpeningHours_ShouldBeValid()
        {
            // Arrange
            var booking = new BookingRequest
            {
                ServiceId = 1,
                StylistId = 1,
                Date = DateTime.Today.AddDays(1),    // tomorrow
                Time = new TimeSpan(10, 0, 0),       // 10:00 AM
                FullName = "Test User",
                Email = "test@example.com",
                Phone = "1234567890"
            };

            // Act
            var results = ValidateModel(booking);

            // Assert
            Assert.Empty(results);  // no validation errors
        }

        [Fact]
        public void Booking_WithTimeOutsideOpeningHours_ShouldHaveOpeningHoursError()
        {
            // Arrange
            var booking = new BookingRequest
            {
                ServiceId = 1,
                StylistId = 1,
                Date = DateTime.Today.AddDays(1),
                Time = new TimeSpan(20, 0, 0),       // 8:00 PM
                FullName = "Test User",
                Email = "test@example.com",
                Phone = "1234567890"
            };

            // Act
            var results = ValidateModel(booking);

            // Assert
            Assert.Contains(results, r =>
                r.ErrorMessage == "Please choose a time during opening hours (9:00 AM – 5:00 PM).");
        }
    }
}
