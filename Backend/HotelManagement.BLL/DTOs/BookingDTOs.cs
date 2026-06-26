using System.ComponentModel.DataAnnotations;
using HotelManagement.DAL.Enums;

namespace HotelManagement.BLL.DTOs;

public class BookingRoomDTO
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int RoomTypeId { get; set; }
    public int? RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public decimal LockedInPrice { get; set; }
}

public class BookingDTO
{
    public int Id { get; set; }
    public int GuestCount { get; set; }
    public List<BookingRoomDTO> Rooms { get; set; } = new();
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public BookingStatus BookingStatus { get; set; }
    public int? UserId { get; set; }
    public BookingOrigin Origin { get; set; }
    public string BookedAt { get; set; } = string.Empty;
    public List<int>? AmenityIds { get; set; }
}

public class CreateBookingRequestDTO : IValidatableObject
{
    public List<int> RoomTypeIds { get; set; } = new();

    [Range(1, 20)]
    public int GuestCount { get; set; } = 1;

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    // Optional because Authenticated Users don't need to provide them
    [StringLength(100)]
    public string? GuestName { get; set; }

    [StringLength(100), EmailAddress]
    public string? GuestEmail { get; set; }

    // Optional amenities selected at booking time
    public List<int>? AmenityIds { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var today = DateTime.UtcNow.Date;
        if (CheckInDate.Date < today)
            yield return new ValidationResult("Check-in date cannot be in the past.", new[] { nameof(CheckInDate) });

        if (CheckOutDate.Date <= CheckInDate.Date)
            yield return new ValidationResult("Check-out date must be after check-in date.", new[] { nameof(CheckOutDate) });

        if (RoomTypeIds == null || !RoomTypeIds.Any())
            yield return new ValidationResult("At least one RoomTypeId must be provided.", new[] { nameof(RoomTypeIds) });
        else if (RoomTypeIds.Any(id => id <= 0))
            yield return new ValidationResult("All RoomTypeIds must be greater than zero.", new[] { nameof(RoomTypeIds) });
    }
}

public class UpdateBookingDTO
{
    // public int? RoomId { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public BookingStatus? BookingStatus { get; set; }
}
