using HotelManagement.BLL.DTOs;
namespace HotelManagement.BLL.Interfaces;
public interface IBillingService
{
    Task<BillingFolioDTO> GenerateFolioAsync(int bookingId);
    Task<object> GetGlobalBillingAsync(string? paymentStatus, DateTime? startDate, DateTime? endDate, string? search, bool detailed, int pageNumber, int pageSize, string? sortBy = null, bool sortDescending = false);
    Task ProcessPaymentAsync(int bookingId, PaymentRequestDTO dto);
    Task ProcessServicePaymentAsync(int bookingId, PaymentRequestDTO dto);
    Task<HotelManagement.Repository.Models.PaginatedResult<ReceiptDTO>> GetReceiptsAsync(DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, string? sortBy = null, bool sortDescending = false);
    Task<ReceiptDTO> GetReceiptByIdAsync(int receiptId);
}
