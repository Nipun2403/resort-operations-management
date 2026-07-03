using HotelManagement.BLL.DTOs;

namespace HotelManagement.BLL.Interfaces;

public interface IPdfService
{
    /// <summary>Returns a PDF byte-array for the given billing folio.</summary>
    byte[] GenerateFolioPdf(BillingFolioDTO folio);
}
