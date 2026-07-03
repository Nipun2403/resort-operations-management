using HotelManagement.DAL.Enums;

using System.ComponentModel.DataAnnotations;

namespace HotelManagement.BLL.DTOs;

public class BillingFolioDTO
{
    public int BookingId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public int NightsStayed { get; set; }
    public decimal RoomBasePrice { get; set; }
    public decimal RoomTotal => NightsStayed * RoomBasePrice;
    public decimal FoodTotal { get; set; }
    public decimal AmenityTotal { get; set; }
    public decimal TotalBill => RoomTotal + FoodTotal + AmenityTotal;
    public string PaymentStatus { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public List<string> FoodItems { get; set; } = new List<string>();
    public List<string> AmenityItems { get; set; } = new List<string>();
}

public class BillingDashboardDTO
{
    public int BookingId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    // public string RoomNumber { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public BookingStatus BookingStatus { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal BaseRoomTotal { get; set; }
}

public class PaymentRequestDTO
{
    public decimal Amount { get; set; }
    [Required(AllowEmptyStrings = false)]
    [StringLength(100, MinimumLength = 1)]
    public string PaymentMethod { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false)]
    [StringLength(100, MinimumLength = 1)]
    public string TransactionId { get; set; } = string.Empty;
}

public class ReceiptDTO
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public decimal AmountPaid { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string PaidAt { get; set; } = string.Empty;
}
