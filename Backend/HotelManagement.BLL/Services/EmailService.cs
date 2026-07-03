using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace HotelManagement.BLL.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    // ── Public interface ──────────────────────────────────────────────────────

    public async Task SendBookingConfirmationAsync(BookingDTO booking)
    {
        var subject = $"Booking Confirmed – Aetheris Collection (#BK-{booking.Id})";
        var html = BuildBookingConfirmationHtml(booking);
        await SendAsync(booking.GuestEmail, booking.GuestName, subject, html);
    }

    public async Task SendPaymentReceiptAsync(BillingFolioDTO folio, string guestEmail, string transactionId, string paymentMethod)
    {
        var subject = $"Payment Receipt – Aetheris Collection (#BK-{folio.BookingId})";
        var html = BuildPaymentReceiptHtml(folio, transactionId, paymentMethod);
        await SendAsync(guestEmail, folio.GuestName, subject, html);
    }

    public async Task SendBookingCancellationAsync(int bookingId, string guestName, string guestEmail)
    {
        var subject = $"Booking Cancelled – Aetheris Collection (#BK-{bookingId})";
        var html = BuildCancellationHtml(bookingId, guestName);
        await SendAsync(guestEmail, guestName, subject, html);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task SendAsync(string toAddress, string toName, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(toAddress))
        {
            _logger.LogWarning("Email skipped: recipient address is empty for subject '{Subject}'.", subject);
            return;
        }

        try
        {
            var host        = _config["Email:SmtpHost"]    ?? "localhost";
            var port        = int.Parse(_config["Email:SmtpPort"] ?? "1025");
            var username    = _config["Email:SmtpUser"]    ?? string.Empty;
            var password    = _config["Email:SmtpPass"]    ?? string.Empty;
            var fromAddress = _config["Email:SenderEmail"] ?? "noreply@aetheris.com";
            var fromName    = _config["Email:SenderName"]  ?? "Aetheris";

            // Port 465 = implicit SSL; 587 = STARTTLS; anything else = auto-negotiate
            var secureOpt = port == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            using var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(new MailboxAddress(toName, toAddress));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, secureOpt);

            if (!string.IsNullOrWhiteSpace(username))
                await client.AuthenticateAsync(username, password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {EmailTo} | Subject: {EmailSubject}", toAddress, subject);
        }
        catch (Exception ex)
        {
            // Non-fatal: log but don't bubble up — email failure must not break the main flow.
            _logger.LogError(ex, "Failed to send email to {EmailTo} | Subject: {EmailSubject}", toAddress, subject);
        }
    }

    // ── HTML templates ────────────────────────────────────────────────────────

    private static string Wrap(string title, string body) => $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <style>
    body {{ margin:0; padding:0; background:#131411; color:#e4e2dd; font-family:'Helvetica Neue',Arial,sans-serif; }}
    .wrapper {{ max-width:600px; margin:40px auto; background:#1f201d; border:1px solid rgba(228,194,133,0.15); border-radius:8px; overflow:hidden; }}
    .header {{ background:#0e0e0c; padding:32px; text-align:center; border-bottom:1px solid rgba(228,194,133,0.2); }}
    .header h1 {{ margin:0; font-size:22px; letter-spacing:0.12em; font-weight:300; color:#e4c285; text-transform:uppercase; }}
    .header p {{ margin:8px 0 0; font-size:12px; color:#8e9192; letter-spacing:0.1em; }}
    .content {{ padding:32px; }}
    .content h2 {{ font-size:18px; font-weight:400; color:#e4e2dd; margin-top:0; }}
    .row {{ display:flex; justify-content:space-between; padding:8px 0; border-bottom:1px solid rgba(255,255,255,0.05); font-size:14px; }}
    .row .label {{ color:#8e9192; }}
    .row .value {{ color:#e4e2dd; font-weight:500; }}
    .total {{ display:flex; justify-content:space-between; padding:14px 0; font-size:16px; font-weight:600; color:#e4c285; margin-top:8px; }}
    .badge {{ display:inline-block; padding:4px 12px; border:1px solid rgba(228,194,133,0.4); border-radius:4px; font-size:12px; letter-spacing:0.08em; text-transform:uppercase; color:#e4c285; margin-top:12px; }}
    .footer {{ background:#0e0e0c; padding:20px 32px; text-align:center; font-size:11px; color:#444748; border-top:1px solid rgba(228,194,133,0.1); }}
    .footer a {{ color:#e4c285; text-decoration:none; }}
  </style>
</head>
<body>
  <div class=""wrapper"">
    <div class=""header"">
      <h1>Aetheris Collection</h1>
      <p>{title}</p>
    </div>
    <div class=""content"">
      {body}
    </div>
    <div class=""footer"">
      © Aetheris Collection &nbsp;·&nbsp; <a href=""#"">Privacy Policy</a> &nbsp;·&nbsp; <a href=""#"">Contact</a>
    </div>
  </div>
</body>
</html>";

    private static string BuildBookingConfirmationHtml(BookingDTO booking)
    {
        var roomsSummary = booking.Rooms != null && booking.Rooms.Any()
            ? string.Join(", ", booking.Rooms.Select(r => r.RoomNumber ?? $"Type #{r.RoomTypeId}"))
            : "To be assigned";

        var body = $@"
      <h2>Your Reservation is Confirmed</h2>
      <p style=""color:#c4c7c7;margin-bottom:24px;font-size:14px;"">
        Dear {System.Net.WebUtility.HtmlEncode(booking.GuestName)}, we look forward to welcoming you.
      </p>
      <div class=""row""><span class=""label"">Booking ID</span><span class=""value"">#BK-{booking.Id}</span></div>
      <div class=""row""><span class=""label"">Check-In</span><span class=""value"">{booking.CheckInDate:dd MMM yyyy}</span></div>
      <div class=""row""><span class=""label"">Check-Out</span><span class=""value"">{booking.CheckOutDate:dd MMM yyyy}</span></div>
      <div class=""row""><span class=""label"">Guests</span><span class=""value"">{booking.GuestCount}</span></div>
      <div class=""row""><span class=""label"">Rooms</span><span class=""value"">{System.Net.WebUtility.HtmlEncode(roomsSummary)}</span></div>
      <div class=""row""><span class=""label"">Payment Status</span><span class=""value"">Pending</span></div>
      <span class=""badge"">Confirmed</span>
      <p style=""margin-top:24px;font-size:13px;color:#8e9192;"">
        Please settle the balance at the front desk upon arrival. If you have any questions, do not hesitate to contact our concierge.
      </p>";

        return Wrap("Booking Confirmation", body);
    }

    private static string BuildPaymentReceiptHtml(BillingFolioDTO folio, string transactionId, string paymentMethod)
    {
        var foodSection = folio.FoodItems.Any()
            ? $@"<p style=""font-size:12px;color:#8e9192;margin:16px 0 4px;text-transform:uppercase;letter-spacing:0.08em;"">Room Service</p>
                 <ul style=""margin:0;padding-left:18px;font-size:13px;color:#c4c7c7;"">
                   {string.Join("", folio.FoodItems.Select(f => $"<li>{System.Net.WebUtility.HtmlEncode(f)}</li>"))}
                 </ul>"
            : "";

        var amenitySection = folio.AmenityItems.Any()
            ? $@"<p style=""font-size:12px;color:#8e9192;margin:16px 0 4px;text-transform:uppercase;letter-spacing:0.08em;"">Amenities</p>
                 <ul style=""margin:0;padding-left:18px;font-size:13px;color:#c4c7c7;"">
                   {string.Join("", folio.AmenityItems.Select(a => $"<li>{System.Net.WebUtility.HtmlEncode(a)}</li>"))}
                 </ul>"
            : "";

        var body = $@"
      <h2>Payment Receipt</h2>
      <p style=""color:#c4c7c7;margin-bottom:24px;font-size:14px;"">
        Dear {System.Net.WebUtility.HtmlEncode(folio.GuestName)}, your payment has been received. Thank you for staying with us.
      </p>
      <div class=""row""><span class=""label"">Booking ID</span><span class=""value"">#BK-{folio.BookingId}</span></div>
      <div class=""row""><span class=""label"">Nights Stayed</span><span class=""value"">{folio.NightsStayed}</span></div>
      <div class=""row""><span class=""label"">Room Charges</span><span class=""value"">{folio.RoomTotal:C}</span></div>
      <div class=""row""><span class=""label"">Food &amp; Beverage</span><span class=""value"">{folio.FoodTotal:C}</span></div>
      <div class=""row""><span class=""label"">Amenities</span><span class=""value"">{folio.AmenityTotal:C}</span></div>
      <div class=""total""><span>Total Paid</span><span>{folio.TotalBill:C}</span></div>
      <div class=""row""><span class=""label"">Payment Method</span><span class=""value"">{System.Net.WebUtility.HtmlEncode(paymentMethod)}</span></div>
      <div class=""row""><span class=""label"">Transaction ID</span><span class=""value"">{System.Net.WebUtility.HtmlEncode(transactionId)}</span></div>
      {foodSection}
      {amenitySection}
      <span class=""badge"">Paid</span>
      <p style=""margin-top:24px;font-size:13px;color:#8e9192;"">
        A detailed PDF receipt is available for download from your booking history. We hope to see you again soon.
      </p>";

        return Wrap("Payment Receipt", body);
    }

    private static string BuildCancellationHtml(int bookingId, string guestName)
    {
        var body = $@"
      <h2>Booking Cancelled</h2>
      <p style=""color:#c4c7c7;margin-bottom:24px;font-size:14px;"">
        Dear {System.Net.WebUtility.HtmlEncode(guestName)}, your booking has been successfully cancelled.
      </p>
      <div class=""row""><span class=""label"">Booking ID</span><span class=""value"">#BK-{bookingId}</span></div>
      <p style=""margin-top:24px;font-size:13px;color:#8e9192;"">
        If a refund applies, it will be processed within 5–7 business days. Should you wish to rebook,
        our reservation team is always at your service.
      </p>
      <span class=""badge"">Cancelled</span>";

        return Wrap("Booking Cancellation", body);
    }
}
