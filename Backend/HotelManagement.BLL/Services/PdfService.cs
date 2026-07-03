using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HotelManagement.BLL.Services;

public class PdfService : IPdfService
{
    // ── Colour palette (mirrors the "Obsidian & Champagne" design tokens) ──────
    private const string ColorBackground = "#131411";
    private const string ColorSurfaceHigh = "#2a2a27";
    private const string ColorOnSurface = "#e4e2dd";
    private const string ColorOnSurfaceVariant = "#c4c7c7";
    private const string ColorSecondary = "#e4c285"; // champagne gold
    private const string ColorMuted = "#8e9192";
    private const string ColorBorder = "#444748";

    public byte[] GenerateFolioPdf(BillingFolioDTO folio)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor(ColorBackground);
                page.DefaultTextStyle(ts => ts
                    .FontFamily("Helvetica")
                    .FontColor(ColorOnSurface)
                    .FontSize(10));

                // ── Header ─────────────────────────────────────────────────────
                page.Header().Element(c => ComposeHeader(c, folio.RoomTypeName));

                // ── Content ────────────────────────────────────────────────────
                page.Content().PaddingVertical(24).Column(col =>
                {
                    col.Spacing(20);

                    // Guest info block
                    col.Item().Element(c => ComposeGuestBlock(c, folio));

                    // Separator
                    col.Item().LineHorizontal(0.5f).LineColor(ColorBorder);

                    // Charges breakdown
                    col.Item().Element(c => ComposeCharges(c, folio));

                    // Separator
                    col.Item().LineHorizontal(0.5f).LineColor(ColorBorder);

                    // Total
                    col.Item().Element(c => ComposeTotal(c, folio));

                    // Optional line-item lists
                    if (folio.FoodItems.Any())
                    {
                        col.Item().LineHorizontal(0.5f).LineColor(ColorBorder);
                        col.Item().Element(c => ComposeItemList(c, "Room Service & Food", folio.FoodItems));
                    }

                    if (folio.AmenityItems.Any())
                    {
                        col.Item().LineHorizontal(0.5f).LineColor(ColorBorder);
                        col.Item().Element(c => ComposeItemList(c, "Amenities Subscribed", folio.AmenityItems));
                    }
                });

                // ── Footer ─────────────────────────────────────────────────────
                page.Footer().Element(ComposeFooter);
            });
        }).GeneratePdf();
    }

    // ── Section composers ─────────────────────────────────────────────────────

    private static void ComposeHeader(IContainer container, string roomTypeName)
    {
        var subtitle = string.IsNullOrWhiteSpace(roomTypeName)
            ? "AETHERIS COLLECTION"
            : roomTypeName.ToUpperInvariant();

        container
            .BorderBottom(1).BorderColor(ColorSecondary)
            .PaddingBottom(16)
            .Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item()
                        .Text("AETHERIS COLLECTION")
                        .FontSize(18)
                        .FontColor(ColorSecondary)
                        .LetterSpacing(0.12f)
                        .Bold();

                    col.Item()
                        .PaddingTop(4)
                        .Text(subtitle)
                        .FontSize(8)
                        .FontColor(ColorMuted)
                        .LetterSpacing(0.15f);
                });

                row.ConstantItem(120).AlignRight().Column(col =>
                {
                    col.Item()
                        .Text("BILLING FOLIO")
                        .FontSize(14)
                        .FontColor(ColorOnSurface)
                        .Bold();

                    col.Item()
                        .PaddingTop(4)
                        .Text($"Generated: {DateTime.UtcNow:dd MMM yyyy}")
                        .FontSize(8)
                        .FontColor(ColorMuted);
                });
            });
    }

    private static void ComposeGuestBlock(IContainer container, BillingFolioDTO folio)
    {
        container.Column(col =>
        {
            col.Item()
                .Text("Guest Details")
                .FontSize(9)
                .FontColor(ColorMuted)
                .LetterSpacing(0.1f)
                .Italic();

            col.Item().PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text($"Booking #BK-{folio.BookingId}").FontSize(13).FontColor(ColorSecondary).Bold();
                    c.Item().PaddingTop(4).Text(folio.GuestName).FontSize(12).FontColor(ColorOnSurface);
                });

                row.ConstantItem(140).Column(c =>
                {
                    LabelValue(c, "Nights Booked", folio.NightsStayed.ToString());
                    LabelValue(c, "Payment Status", folio.PaymentStatus);
                });
            });
        });
    }

    private static void ComposeCharges(IContainer container, BillingFolioDTO folio)
    {
        container.Column(col =>
        {
            col.Item()
                .Text("Charges Breakdown")
                .FontSize(9)
                .FontColor(ColorMuted)
                .LetterSpacing(0.1f)
                .Italic();

            col.Item().PaddingTop(8).Column(c =>
            {
                ChargeRow(c, "Room Rate (per night)", $"{folio.RoomBasePrice:C}");
                ChargeRow(c, $"Room Subtotal  ({folio.NightsStayed} night{(folio.NightsStayed != 1 ? "s" : "")})", $"{folio.RoomTotal:C}");
                ChargeRow(c, "Food & Beverage", $"{folio.FoodTotal:C}");
                ChargeRow(c, "Amenities", $"{folio.AmenityTotal:C}");
            });
        });
    }

    private static void ComposeTotal(IContainer container, BillingFolioDTO folio)
    {
        container
            .Background(ColorSurfaceHigh)
            .Padding(12)
            .Row(row =>
            {
                row.RelativeItem()
                    .Text("TOTAL DUE")
                    .FontSize(11)
                    .FontColor(ColorOnSurface)
                    .Bold()
                    .LetterSpacing(0.08f);

                row.ConstantItem(100)
                    .AlignRight()
                    .Text($"{folio.TotalBill:C}")
                    .FontSize(14)
                    .FontColor(ColorSecondary)
                    .Bold();
            });
    }

    private static void ComposeItemList(IContainer container, string title, IEnumerable<string> items)
    {
        container.Column(col =>
        {
            col.Item()
                .Text(title.ToUpperInvariant())
                .FontSize(9)
                .FontColor(ColorMuted)
                .LetterSpacing(0.1f)
                .Italic();

            foreach (var item in items)
            {
                col.Item()
                    .PaddingTop(4)
                    .PaddingLeft(12)
                    .Text($"• {item}")
                    .FontSize(9)
                    .FontColor(ColorOnSurfaceVariant);
            }
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container
            .BorderTop(0.5f).BorderColor(ColorBorder)
            .PaddingTop(10)
            .Row(row =>
            {
                row.RelativeItem()
                    .Text("Thank you for choosing the Aetheris Collection. We hope to welcome you again.")
                    .FontSize(8)
                    .FontColor(ColorMuted)
                    .Italic();

                row.ConstantItem(80)
                    .AlignRight()
                    .Text(x =>
                    {
                        x.Span("Page ").FontSize(8).FontColor(ColorMuted);
                        x.CurrentPageNumber().FontSize(8).FontColor(ColorMuted);
                        x.Span(" / ").FontSize(8).FontColor(ColorMuted);
                        x.TotalPages().FontSize(8).FontColor(ColorMuted);
                    });
            });
    }

    // ── Micro helpers ─────────────────────────────────────────────────────────

    private static void LabelValue(ColumnDescriptor col, string label, string value)
    {
        col.Item().Row(r =>
        {
            r.RelativeItem().Text(label).FontSize(8).FontColor(ColorMuted);
            r.ConstantItem(70).AlignRight().Text(value).FontSize(9).FontColor(ColorOnSurface);
        });
    }

    private static void ChargeRow(ColumnDescriptor col, string label, string amount)
    {
        col.Item()
            .BorderBottom(0.5f)
            .BorderColor(ColorBorder)
            .PaddingVertical(5)
            .Row(r =>
            {
                r.RelativeItem().Text(label).FontSize(10).FontColor(ColorOnSurfaceVariant);
                r.ConstantItem(80).AlignRight().Text(amount).FontSize(10).FontColor(ColorOnSurface);
            });
    }
}
