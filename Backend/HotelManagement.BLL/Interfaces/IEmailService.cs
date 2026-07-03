using HotelManagement.BLL.DTOs;

namespace HotelManagement.BLL.Interfaces;

public interface IEmailService
{
    Task SendBookingConfirmationAsync(BookingDTO booking);
    Task SendPaymentReceiptAsync(BillingFolioDTO folio, string guestEmail, string transactionId, string paymentMethod);
    Task SendBookingCancellationAsync(int bookingId, string guestName, string guestEmail);
}
