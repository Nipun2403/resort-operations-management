const PptxGenJS = require("pptxgenjs");

const C = {
  BG: "FDFBF7", GOLD: "C8A84E", GOLD_DK: "B8963E",
  TEXT: "1A1A1A", TEXT_SEC: "4A4A4A", BORDER: "E8E4DC",
  SURFACE: "FFFFFF", GREEN: "2D7D46", RED: "C0392B",
};
const F = { H: "Georgia", B: "Helvetica", M: "Menlo" };
const SW = 10, SH = 5.625;

function addSlide_() {
  const s = pptx.addSlide();
  s.background = { fill: C.BG };
  return s;
}

function addFooter(s) {
  s.addText("CONFIDENTIAL & PROPRIETARY", {
    x: 0.4, y: SH - 0.35, w: 3, h: 0.25, fontSize: 7, fontFace: F.B, color: C.BORDER,
  });
  s.addText("Aetheris", {
    x: SW - 1.2, y: SH - 0.35, w: 0.8, h: 0.25, fontSize: 7, fontFace: F.B, color: C.BORDER,
    align: "right",
  });
}

function headerSlide(title) {
  const s = addSlide_();
  s.addText(title, { x: 0.4, y: 0.2, w: 9.2, h: 0.45, fontSize: 20, fontFace: F.H, color: C.TEXT, bold: true });
  s.addShape(pptx.ShapeType.rect, { x: 0.4, y: 0.62, w: 2.2, h: 0.035, fill: { color: C.GOLD } });
  addFooter(s);
  return s;
}

const pptx = new PptxGenJS();
pptx.defineLayout({ name: "WIDE", width: SW, height: SH });
pptx.layout = "WIDE";
pptx.author = "Aetheris";
pptx.title = "Aetheris — The Unified Hotel Operating System";
pptx.subject = "v1 Light Replica";

// ═══════════════════════════════════════════════
// SLIDE 1 — TITLE
// ═══════════════════════════════════════════════
(() => {
  const s = addSlide_();
  s.addShape(pptx.ShapeType.rect, { x: 0, y: 0, w: SW, h: SH, fill: { color: C.GOLD } });
  s.addShape(pptx.ShapeType.rect, { x: 0.04, y: 0.04, w: SW - 0.08, h: SH - 0.08, fill: { color: C.BG } });
  s.addText("Aetheris", {
    x: 0.5, y: 1.0, w: 9, h: 0.9, fontSize: 44, fontFace: F.H, color: C.TEXT, bold: true, align: "center",
  });
  s.addText("The Unified Hotel Operating System", {
    x: 0.5, y: 1.85, w: 9, h: 0.5, fontSize: 20, fontFace: F.B, color: C.GOLD_DK, align: "center",
  });
  s.addShape(pptx.ShapeType.rect, { x: 3, y: 2.4, w: 4, h: 0.03, fill: { color: C.GOLD } });
  s.addText("SECURITY-FIRST ARCHITECTURE  •  AI-POWERED OPERATIONS  •  REAL-TIME COORDINATION", {
    x: 0.5, y: 2.6, w: 9, h: 0.35, fontSize: 11, fontFace: F.B, color: C.TEXT_SEC, align: "center",
  });
  s.addText('From Magic Byte Validation to LLM-Powered Concierge — A Full-Stack Engineering Showcase', {
    x: 0.5, y: 3.5, w: 9, h: 0.35, fontSize: 12, fontFace: F.B, color: C.TEXT_SEC, align: "center",
  });
  s.addText("CONFIDENTIAL & PROPRIETARY", {
    x: 0.5, y: 4.8, w: 9, h: 0.3, fontSize: 9, fontFace: F.B, color: C.BORDER, align: "center",
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 2 — AGENDA
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("From Problems to Solutions");
  s.addText("Aetheris Platform Architecture & Implementation Agenda", {
    x: 0.4, y: 0.75, w: 9.2, h: 0.3, fontSize: 12, fontFace: F.B, color: C.TEXT_SEC,
  });
  const items = [
    "Why Hotel Software Fails — The Real Problems",
    "Aetheris Platform Architecture",
    "Zero-Trust Image Upload Pipeline",
    "Magic Byte Validation — Content-Level Security",
    "Async Architecture & Multi-Tier Cleanup",
    "AI Concierge — Real Action Execution",
    "The Proposal Pattern — Taming LLM Hallucination",
    "Real-Time Operations — SignalR Ecosystem",
    "Engineering Foundations — Idempotency, Audit, RBAC, Quality",
    "Business Impact",
  ];
  items.forEach((item, i) => {
    const col = i < 5 ? 0 : 1;
    const row = i < 5 ? i : i - 5;
    const cx = col === 0 ? 0.5 : 5.2;
    const cy = 1.2 + row * 0.7;
    s.addShape(pptx.ShapeType.ellipse, {
      x: cx, y: cy + 0.05, w: 0.35, h: 0.35, fill: { color: C.GOLD },
    });
    s.addText(`${i + 1}`, {
      x: cx, y: cy + 0.05, w: 0.35, h: 0.35, fontSize: 12, fontFace: F.B,
      color: "FFFFFF", bold: true, align: "center", valign: "middle",
    });
    s.addText(item, {
      x: cx + 0.45, y: cy, w: 4.0, h: 0.45, fontSize: 11, fontFace: F.B, color: C.TEXT, valign: "middle",
    });
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 3 — BROKEN STATE OF HOTEL TECH
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("The Broken State of Hotel Tech");
  s.addText("Most hotel software is a collection of disconnected tools held together by phone calls and spreadsheets.", {
    x: 0.4, y: 0.75, w: 9.2, h: 0.3, fontSize: 11, fontFace: F.B, color: C.TEXT_SEC,
  });
  const problems = [
    ["Security Vulnerabilities", "File uploads accept any extension — renamed malware passes through. Chatbot gets manipulated easily to give out sensitive data."],
    ["No Guest Self-Service", "Guests call the front desk for everything; 24/7 support is impossible. Front desk is overwhelmed handling all operations."],
    ["Siloed Departments", "Housekeeping doesn't know about checkout until someone calls from front desk."],
    ["Fragile Engineering", "No idempotency equals double-bookings. No audit trail means billing disputes are unprovable."],
    ["Looks Like Enterprise Software from 2010", "Bulky admin panels, flat designs, no brand identity. Guests judge your hotel by your website."],
  ];
  problems.forEach((p, i) => {
    const cy = 1.2 + i * 0.82;
    s.addShape(pptx.ShapeType.rect, {
      x: 0.4, y: cy, w: 9.2, h: 0.72, fill: { color: C.SURFACE },
      line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.06,
    });
    s.addText(p[0], {
      x: 0.55, y: cy + 0.05, w: 2.8, h: 0.25, fontSize: 11, fontFace: F.B, color: C.GOLD_DK, bold: true,
    });
    s.addText(p[1], {
      x: 0.55, y: cy + 0.3, w: 8.9, h: 0.38, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
    });
  });
  s.addText('"Generic hotel software manages records. Aetheris runs operations."', {
    x: 0.4, y: SH - 0.55, w: 9.2, h: 0.3, fontSize: 11, fontFace: F.H, color: C.GOLD_DK, italic: true, align: "center",
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 4 — PLATFORM OVERVIEW
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("Aetheris — Platform Overview");
  s.addText("Enterprise N-Tier C# and Angular Production Architecture", {
    x: 0.4, y: 0.75, w: 9.2, h: 0.3, fontSize: 11, fontFace: F.B, color: C.TEXT_SEC,
  });

  const layers = [
    { label: "Angular 22 Client SPA (Signals)", y: 1.1, sub: "REST & SignalR" },
    { label: "ASP.NET Core 10 API Gateway", y: 1.55 },
    { label: "BLL Services Layer", y: 2.0 },
    { label: "Repository Layer", y: 2.45 },
    { label: "EF Core + PostgreSQL (DAL)", y: 2.9 },
    { label: "Azure Cloud Infrastructure", y: 3.35 },
  ];
  layers.forEach((l, i) => {
    s.addShape(pptx.ShapeType.rect, {
      x: 0.4, y: l.y, w: 4.0, h: 0.38, fill: { color: i === 5 ? "FFF8E8" : C.SURFACE },
      line: { color: i === 5 ? C.GOLD : C.BORDER, width: i === 5 ? 1.5 : 0.75 }, rectRadius: 0.05,
    });
    s.addText(l.label, {
      x: 0.5, y: l.y + 0.03, w: 3.8, h: 0.2, fontSize: 9, fontFace: F.M, color: C.TEXT, bold: true,
    });
    if (l.sub) s.addText(l.sub, {
      x: 0.5, y: l.y + 0.2, w: 3.8, h: 0.16, fontSize: 7.5, fontFace: F.M, color: C.TEXT_SEC,
    });
    if (i < 5) s.addText("▼", {
      x: 2.1, y: l.y + 0.36, w: 0.3, h: 0.12, fontSize: 8, color: C.GOLD, align: "center",
    });
  });

  // Right column - stats
  const stats = [
    { label: "6 User Roles", sub: "Admin • FrontDesk • Guest • Kitchen • Housekeeping • Maintenance" },
    { label: "7 Angular Route Guards", sub: "Role-based SPA access control" },
    { label: "4 Automated Background Workers", sub: "Magic byte validation • Orphan cleanup • Blob cross-reference • Idempotency cleanup" },
  ];
  const ssy = 1.1;
  stats.forEach((st, i) => {
    s.addShape(pptx.ShapeType.rect, {
      x: 4.8, y: ssy + i * 0.85, w: 4.8, h: 0.75, fill: { color: C.SURFACE },
      line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.06,
    });
    s.addText(st.label, {
      x: 4.95, y: ssy + i * 0.85 + 0.05, w: 4.5, h: 0.25, fontSize: 11, fontFace: F.B, color: C.GOLD_DK, bold: true,
    });
    s.addText(st.sub, {
      x: 4.95, y: ssy + i * 0.85 + 0.3, w: 4.5, h: 0.4, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
    });
  });

  // KPI section
  s.addShape(pptx.ShapeType.rect, {
    x: 4.8, y: 3.65, w: 4.8, h: 1.15, fill: { color: "FFF8E8" },
    line: { color: C.GOLD, width: 1 }, rectRadius: 0.06,
  });
  s.addText("KPIs Computed In-Database", {
    x: 4.95, y: 3.7, w: 4.5, h: 0.25, fontSize: 11, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  s.addText("4 PostgreSQL stored procedures drive 7 live KPIs: Occupancy Rate, RevPAR, ADR, Guest Satisfaction, Length of Stay, Cancellation Rate, Housekeeping Turnaround — rendered as ECharts dashboards.", {
    x: 4.95, y: 3.95, w: 4.5, h: 0.8, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 5 — USER DASHBOARD
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("User Dashboard");
  s.addText("Guest-Facing Dashboard with Booking Management, AI Concierge, and Audit Trail", {
    x: 0.4, y: 0.75, w: 9.2, h: 0.3, fontSize: 12, fontFace: F.B, color: C.TEXT_SEC,
  });
  // Simulate dashboard description
  const features = [
    ["Booking Overview", "Active bookings, upcoming stays, and past reservations in a unified timeline view."],
    ["AI Concierge (ATLAS)", "Chat interface for room service, housekeeping, maintenance requests — natural language to real actions."],
    ["Audit Trail", "Every action logged — view complete change history for bookings, requests, and folio."],
    ["Guest Profile", "Personal details, preferences, loyalty status, and stay history."],
  ];
  features.forEach((f, i) => {
    const col = i % 2;
    const row = Math.floor(i / 2);
    const cx = 0.4 + col * 4.8;
    const cy = 1.2 + row * 1.7;
    s.addShape(pptx.ShapeType.rect, {
      x: cx, y: cy, w: 4.4, h: 1.45, fill: { color: C.SURFACE },
      line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
    });
    s.addShape(pptx.ShapeType.rect, {
      x: cx, y: cy, w: 0.06, h: 1.45, fill: { color: C.GOLD },
    });
    s.addText(f[0], {
      x: cx + 0.2, y: cy + 0.1, w: 4.0, h: 0.3, fontSize: 13, fontFace: F.B, color: C.GOLD_DK, bold: true,
    });
    s.addText(f[1], {
      x: cx + 0.2, y: cy + 0.45, w: 4.0, h: 0.85, fontSize: 10, fontFace: F.B, color: C.TEXT_SEC,
    });
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 6 — STAFF DASHBOARD
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("Staff Dashboard");
  s.addText("Ticket-Based Operations: Housekeeping, Maintenance & Kitchen", {
    x: 0.4, y: 0.75, w: 9.2, h: 0.3, fontSize: 12, fontFace: F.B, color: C.TEXT_SEC,
  });
  const roles = [
    { title: "Housekeeping", items: ["Real-time room status board (Clean/Dirty/Inspected)", "Task assignment with priority levels", "Checkout-triggered cleaning alerts via SignalR", "Supply inventory tracking"], icon: "HK" },
    { title: "Maintenance", items: ["Issue tracking with photo attachments", "Priority-based dispatch (Emergency/Routine)", "Parts inventory and vendor tracking", "Preventive maintenance scheduling"], icon: "MT" },
    { title: "Kitchen", items: ["Incoming order stream with real-time updates", "Order preparation status (Received/Preparing/Done)", "Dietary restriction flags on orders", "Plate-up confirmation triggers delivery alert"], icon: "KT" },
  ];
  roles.forEach((r, i) => {
    const cx = 0.35 + i * 3.2;
    s.addShape(pptx.ShapeType.rect, {
      x: cx, y: 1.2, w: 3.0, h: 3.7, fill: { color: C.SURFACE },
      line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
    });
    s.addShape(pptx.ShapeType.ellipse, {
      x: cx + 1.15, y: 1.3, w: 0.7, h: 0.7, fill: { color: C.GOLD },
    });
    s.addText(r.icon, {
      x: cx + 1.15, y: 1.3, w: 0.7, h: 0.7, fontSize: 14, fontFace: F.B,
      color: "FFFFFF", bold: true, align: "center", valign: "middle",
    });
    s.addText(r.title, {
      x: cx + 0.15, y: 2.15, w: 2.7, h: 0.3, fontSize: 14, fontFace: F.B, color: C.TEXT, bold: true, align: "center",
    });
    s.addShape(pptx.ShapeType.rect, {
      x: cx + 0.8, y: 2.48, w: 1.4, h: 0.03, fill: { color: C.GOLD },
    });
    r.items.forEach((item, j) => {
      s.addText(`▸ ${item}`, {
        x: cx + 0.2, y: 2.65 + j * 0.45, w: 2.6, h: 0.4, fontSize: 9.5, fontFace: F.B, color: C.TEXT_SEC,
      });
    });
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 7 — KPIs (ADMIN)
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("KPIs Computed In-Database (Admin)");
  s.addText("7 Live KPIs Powered by 4 PostgreSQL Stored Procedures", {
    x: 0.4, y: 0.75, w: 9.2, h: 0.3, fontSize: 12, fontFace: F.B, color: C.TEXT_SEC,
  });
  const kpis = [
    ["Occupancy Rate", "% of rooms occupied over total available"],
    ["RevPAR", "Revenue Per Available Room"],
    ["ADR", "Average Daily Rate"],
    ["Guest Satisfaction", "Aggregated feedback score"],
    ["Length of Stay", "Average nights per booking"],
    ["Cancellation Rate", "% of bookings cancelled"],
    ["Housekeeping Turnaround", "Avg minutes to turn room"],
  ];
  s.addTable([
    [{ text: "KPI", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
     { text: "Description", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } }],
    ...kpis.map(k => ([
      { text: k[0], options: { fontSize: 9, fontFace: F.B, bold: true, color: C.TEXT } },
      { text: k[1], options: { fontSize: 9, fontFace: F.B, color: C.TEXT_SEC } },
    ])),
  ], {
    x: 0.5, y: 1.1, w: 9, colW: [3, 6],
    border: { type: "solid", pt: 0.5, color: C.BORDER },
    rowH: 0.35,
    margin: [3, 5, 3, 5],
  });
  s.addText("Rendered as dark-themed ECharts dashboards (bar, line, radar, pie) with date presets and category filters", {
    x: 0.4, y: SH - 0.65, w: 9.2, h: 0.3, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC, align: "center",
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 8 — DIRECT-TO-AZURE UPLOADS
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("Direct-to-Azure Uploads");
  s.addText("Securing the #1 Attack Vector with SAS Tokens", {
    x: 0.4, y: 0.75, w: 9.2, h: 0.3, fontSize: 12, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Problem box
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 1.15, w: 4.2, h: 1.2, fill: { color: "FFF5F5" },
    line: { color: C.RED, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText("The Problem", {
    x: 0.55, y: 1.2, w: 3.9, h: 0.22, fontSize: 10, fontFace: F.B, color: C.RED, bold: true,
  });
  s.addText("File Uploads Are the #1 Attack Vector\n— Extension checks are trivial to bypass (renamed .exe passes)\n— Server-buffered bytes expose backend to resource exhaustion", {
    x: 0.55, y: 1.45, w: 3.9, h: 0.8, fontSize: 8.5, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Architecture steps
  s.addText("Direct Upload Architecture (Zero-Byte Server Overhead)", {
    x: 5.0, y: 1.15, w: 4.6, h: 0.25, fontSize: 10, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  const steps = [
    "① Browser ➔ API: POST /upload-sas",
    "② API ➔ Browser: Returns SAS URL",
    "③ Browser ➔ Azure: PUT file directly",
    "④ Browser ➔ API: POST /confirm",
    "⑤ API ➔ Queue: Enqueues to Worker",
  ];
  steps.forEach((st, i) => {
    s.addText(st, {
      x: 5.0, y: 1.45 + i * 0.28, w: 4.6, h: 0.25, fontSize: 8.5, fontFace: F.M, color: C.TEXT,
    });
  });

  // Defense layers
  s.addText("Defense-in-Depth", { x: 0.4, y: 2.6, w: 4, h: 0.25, fontSize: 11, fontFace: F.B, color: C.GOLD_DK, bold: true });
  const layers = [
    "MIME Accept Attribute", "Extension Whitelist",
    "Size Limit: 10MB Max", "SAS URL: 15-Min Expiry",
    "SAS: Write+Create Only", "Ownership Check",
    "Magic Byte Validation", "Post-Upload Verification",
  ];
  layers.forEach((l, i) => {
    const col = i < 4 ? 0 : 1;
    const row = i < 4 ? i : i - 4;
    s.addShape(pptx.ShapeType.rect, {
      x: 0.4 + col * 2.15, y: 2.95 + row * 0.4, w: 2.0, h: 0.32, fill: { color: C.SURFACE },
      line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.05,
    });
    s.addText(l, { x: 0.45 + col * 2.15, y: 2.97 + row * 0.4, w: 1.9, h: 0.28, fontSize: 8.5, fontFace: F.B, color: C.TEXT_SEC, valign: "middle" });
  });

  // Right: Value prop
  s.addText("The file never touches the backend.", {
    x: 5.0, y: 2.6, w: 4.6, h: 0.25, fontSize: 11, fontFace: F.H, color: C.GOLD_DK, bold: true,
  });
  s.addText("The server only generates a time-limited SAS token and returns the upload URL. The browser PUTs the file directly to Azure Blob Storage. No server memory consumed. No bandwidth through your API layer. No credentials exposed to the client.", {
    x: 5.0, y: 2.9, w: 4.6, h: 1.0, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
  });
  s.addText("What the SAS token prevents: If intercepted, the token grants Write+Create on ONE blob only — no Read, no List, no Delete — and expires in 15 minutes.", {
    x: 5.0, y: 3.8, w: 4.6, h: 0.6, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 9 — MAGIC BYTE VALIDATION
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("Magic Byte Validation");
  s.addText("Beyond Extension Checking — Zero-Trust File Security", {
    x: 0.4, y: 0.75, w: 9.2, h: 0.3, fontSize: 12, fontFace: F.B, color: C.TEXT_SEC,
  });
  // Left - problem
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 1.15, w: 4.4, h: 1.2, fill: { color: "FFF5F5" },
    line: { color: C.RED, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText("The Extension Bypass Vulnerability", {
    x: 0.55, y: 1.2, w: 4.1, h: 0.25, fontSize: 10, fontFace: F.B, color: C.RED, bold: true,
  });
  s.addText("A file named image.jpg with executable content passes every traditional extension check. Most legacy systems stop validation here, exposing servers to severe exploits.", {
    x: 0.55, y: 1.5, w: 4.1, h: 0.75, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Right - magic bytes table
  s.addText("Aetheris Header Signature Check", {
    x: 5.2, y: 1.15, w: 4.4, h: 0.25, fontSize: 11, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  s.addTable([
    [{ text: "Format", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
     { text: "Magic Bytes (Hex)", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
     { text: "Status", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } }],
    [{ text: "JPEG", options: { fontSize: 9, fontFace: F.M } },
     { text: "FF D8 FF", options: { fontSize: 9, fontFace: F.M } },
     { text: "ALLOWED", options: { fontSize: 9, fontFace: F.B, color: C.GREEN, bold: true } }],
    [{ text: "PNG", options: { fontSize: 9, fontFace: F.M } },
     { text: "89 50 4E 47...", options: { fontSize: 9, fontFace: F.M } },
     { text: "ALLOWED", options: { fontSize: 9, fontFace: F.B, color: C.GREEN, bold: true } }],
    [{ text: "EXE / ELF", options: { fontSize: 9, fontFace: F.M } },
     { text: "4D 5A / 7F...", options: { fontSize: 9, fontFace: F.M } },
     { text: "BLOCKED", options: { fontSize: 9, fontFace: F.B, color: C.RED, bold: true } }],
  ], {
    x: 5.2, y: 1.5, w: 4.4, colW: [1.3, 1.8, 1.3],
    border: { type: "solid", pt: 0.5, color: C.BORDER },
    rowH: 0.32,
  });

  // Code snippet
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 2.6, w: 9.2, h: 1.6, fill: { color: "F8F6F2" },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText("// Download first 512 bytes & verify\nvar header = await blob.DownloadRangeAsync(0, 512);\nif (!MagicBytes.IsValid(header, out var ext))\n{\n    await blob.DeleteAsync(); // Immediate purge\n    throw new SecurityException(\"Disguised executable\");\n}", {
    x: 0.6, y: 2.65, w: 8.8, h: 1.45, fontSize: 9.5, fontFace: F.M, color: C.TEXT, lineSpacingMultiple: 0.95,
  });
  s.addText("Enterprise-grade content validation in a modern hotel operating system", {
    x: 0.4, y: 4.4, w: 9.2, h: 0.3, fontSize: 10, fontFace: F.H, color: C.GOLD_DK, italic: true, align: "center",
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 10 — ASYNC ARCHITECTURE
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("Async Architecture & Multi-Tier Cleanup");
  s.addText("Validation shouldn't block HTTP requests, and orphaned blobs cost money. Defense in depth for your Azure bill.", {
    x: 0.4, y: 0.75, w: 9.2, h: 0.3, fontSize: 11, fontFace: F.B, color: C.TEXT_SEC,
  });
  const workers = [
    { title: "Queue-Triggered", name: "ImageValidationWorker", desc: "Triggered by Azure Queue for async validation. Fast checks of magic bytes and size constraints run safely out-of-band without stalling HTTP requests.", color: C.GOLD },
    { title: "Hourly Cleanup", name: "OrphanImageCleanup", desc: "Runs on hourly cron to expire stale client upload sessions. Ensures orphaned storage blocks are systematically purged.", color: C.GREEN },
    { title: "Periodic Reconciliation", name: "BlobCleanupWorker", desc: "Performs complete container-wide storage scan. Reconciles physical blobs with database records, isolating untracked files.", color: C.RED },
  ];
  workers.forEach((w, i) => {
    const cx = 0.35 + i * 3.2;
    s.addShape(pptx.ShapeType.rect, {
      x: cx, y: 1.15, w: 3.0, h: 2.0, fill: { color: C.SURFACE },
      line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
    });
    s.addShape(pptx.ShapeType.rect, {
      x: cx, y: 1.15, w: 3.0, h: 0.04, fill: { color: w.color },
    });
    s.addText(w.title, {
      x: cx + 0.12, y: 1.3, w: 2.75, h: 0.2, fontSize: 9, fontFace: F.B, color: w.color, bold: true,
    });
    s.addText(w.name, {
      x: cx + 0.12, y: 1.5, w: 2.75, h: 0.2, fontSize: 11, fontFace: F.M, color: C.TEXT, bold: true,
    });
    s.addText(w.desc, {
      x: cx + 0.12, y: 1.75, w: 2.75, h: 1.3, fontSize: 8.5, fontFace: F.B, color: C.TEXT_SEC,
    });
  });

  // Flow diagram at bottom
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 3.4, w: 9.2, h: 1.3, fill: { color: "FFF8E8" },
    line: { color: C.GOLD, width: 0.75 }, rectRadius: 0.08,
  });
  const flowDiagram = [
    "SUCCESS PATH                           CLEANUP PATH",
    "",
    "PENDING ──▶ CONFIRMED ──▶ ATTACHED ──▶ DB PERSISTED",
    "                                  ",
    "                    REJECTED ──▶ DELETED ──▶ BLOB PURGED",
  ];
  s.addText(flowDiagram.join("\n"), {
    x: 0.6, y: 3.5, w: 8.8, h: 1.1, fontSize: 10, fontFace: F.M, color: C.TEXT, lineSpacingMultiple: 1.0,
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 11 — ATLAS AI CONCIERGE
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("Atlas: The AI Concierge");
  s.addText("Problem: Guests need 24/7 service, but staff can't be everywhere (no one to call at 2 AM).", {
    x: 0.4, y: 0.75, w: 9.2, h: 0.3, fontSize: 11, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Architecture flow
  const archLayers = [
    { label: "GUEST CHAT UI", sub: "Web interface connecting guests to the concierge." },
    { label: "ASP.NET CORE API", sub: "Secure ingress routing and state validation controller." },
    { label: "CONCIERGE ORCHESTRATOR", sub: "Coordinates LLMs, Tool execution, & SignalR alerts." },
  ];
  archLayers.forEach((l, i) => {
    s.addShape(pptx.ShapeType.rect, {
      x: 0.4, y: 1.15 + i * 0.65, w: 4.4, h: 0.55, fill: { color: C.SURFACE },
      line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.06,
    });
    s.addText(l.label, {
      x: 0.55, y: 1.18 + i * 0.65, w: 4.1, h: 0.25, fontSize: 9.5, fontFace: F.M, color: C.GOLD_DK, bold: true,
    });
    s.addText(l.sub, {
      x: 0.55, y: 1.4 + i * 0.65, w: 4.1, h: 0.25, fontSize: 8.5, fontFace: F.B, color: C.TEXT_SEC,
    });
    if (i < archLayers.length - 1) s.addText("▼", {
      x: 2.4, y: 1.7 + i * 0.65, w: 0.3, h: 0.12, fontSize: 8, color: C.GOLD, align: "center",
    });
  });

  // Tools
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 3.15, w: 4.4, h: 1.2, fill: { color: C.SURFACE },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText("SIDE-EFFECT TOOLS (3)", {
    x: 0.55, y: 3.2, w: 4.1, h: 0.2, fontSize: 9, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  s.addText("create_food_order • create_housekeeping_request • create_maintenance_ticket", {
    x: 0.55, y: 3.4, w: 4.1, h: 0.25, fontSize: 8.5, fontFace: F.M, color: C.TEXT_SEC,
  });
  s.addText("READ-ONLY QUERY TOOLS (5)", {
    x: 0.55, y: 3.7, w: 4.1, h: 0.2, fontSize: 9, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  s.addText("get_booking_info • get_folio_balance • get_housekeeping_status • get_menu_items • get_active_orders", {
    x: 0.55, y: 3.9, w: 4.1, h: 0.35, fontSize: 8.5, fontFace: F.M, color: C.TEXT_SEC,
  });

  // Right side - key insight
  s.addShape(pptx.ShapeType.rect, {
    x: 5.2, y: 1.15, w: 4.5, h: 2.0, fill: { color: "FFF8E8" },
    line: { color: C.GOLD, width: 1.5 }, rectRadius: 0.08,
  });
  s.addText("ZERO NEW BUSINESS LOGIC", {
    x: 5.35, y: 1.25, w: 4.2, h: 0.3, fontSize: 14, fontFace: F.B, color: C.GOLD_DK, bold: true, align: "center",
  });
  s.addText("Reusing Hardened Systems", {
    x: 5.35, y: 1.55, w: 4.2, h: 0.25, fontSize: 11, fontFace: F.B, color: C.GOLD_DK, align: "center",
  });
  s.addText("The AI concierge layer is completely stateless. It acts as an orchestrator, securely forwarding structured execution to existing core subsystems:\n\n• OrderService\n• HousekeepingService\n• MaintenanceService", {
    x: 5.35, y: 1.9, w: 4.2, h: 1.1, fontSize: 9.5, fontFace: F.B, color: C.TEXT_SEC, lineSpacingMultiple: 1.0,
  });

  // Security note
  s.addShape(pptx.ShapeType.rect, {
    x: 5.2, y: 3.35, w: 4.5, h: 1.0, fill: { color: "FFF5F5" },
    line: { color: C.RED, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText("Strong Against Manipulation", {
    x: 5.35, y: 3.4, w: 4.2, h: 0.2, fontSize: 10, fontFace: F.B, color: C.RED, bold: true,
  });
  s.addText("Every guest input goes through a sanitization layer that safeguards the LLM model, our system, and our data from malicious intent.", {
    x: 5.35, y: 3.65, w: 4.2, h: 0.55, fontSize: 8.5, fontFace: F.B, color: C.TEXT_SEC,
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 12 — THE PROPOSAL PATTERN
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("The Proposal Pattern");
  s.addText("AI That Acts, But Doesn't Assume", {
    x: 0.4, y: 0.75, w: 9.2, h: 0.3, fontSize: 12, fontFace: F.B, color: C.TEXT_SEC,
  });

  // THE PROBLEM
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 1.1, w: 9.2, h: 0.55, fill: { color: "FFF5F5" },
    line: { color: C.RED, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText("THE PROBLEM", {
    x: 0.55, y: 1.12, w: 8.9, h: 0.22, fontSize: 10, fontFace: F.B, color: C.RED, bold: true,
  });
  s.addText("LLMs hallucinate. They execute tools confidently but incorrectly. Unchecked, automated backend actions destroy systems and trust.", {
    x: 0.55, y: 1.35, w: 8.9, h: 0.22, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Step 1
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 1.85, w: 4.4, h: 1.5, fill: { color: C.SURFACE },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
  });
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 1.85, w: 0.06, h: 1.5, fill: { color: C.GOLD },
  });
  s.addText("STEP 1: PROPOSAL GENERATION", {
    x: 0.6, y: 1.9, w: 4.0, h: 0.25, fontSize: 10, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  s.addText("Guest Request to Pending Proposal", {
    x: 0.6, y: 2.15, w: 4.0, h: 0.2, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC, italic: true,
  });
  s.addText("The guest request prompts the LLM to create explicit, visible proposals with a strict 5-minute TTL. No backend mutations occur at this stage.", {
    x: 0.6, y: 2.4, w: 4.0, h: 0.8, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Step 2
  s.addShape(pptx.ShapeType.rect, {
    x: 5.2, y: 1.85, w: 4.4, h: 1.5, fill: { color: C.SURFACE },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
  });
  s.addShape(pptx.ShapeType.rect, {
    x: 5.2, y: 1.85, w: 0.06, h: 1.5, fill: { color: C.GREEN },
  });
  s.addText("STEP 2: CONFIRMATION & EXECUTION", {
    x: 5.4, y: 1.9, w: 4.0, h: 0.25, fontSize: 10, fontFace: F.B, color: C.GREEN, bold: true,
  });
  s.addText("Explicit Authorization", {
    x: 5.4, y: 2.15, w: 4.0, h: 0.2, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC, italic: true,
  });
  s.addText("The guest must confirm before execution. Real-time SignalR alerts instantly trigger to synchronize guest interface state across active devices.", {
    x: 5.4, y: 2.4, w: 4.0, h: 0.8, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Security layers
  s.addText("SECURITY DEFENSE LAYERS", {
    x: 0.4, y: 3.55, w: 4, h: 0.25, fontSize: 10, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  const defenses = [
    "5-Minute TTL: Auto-expires stale proposals safely",
    "Prompt Injection Sanitizer: Intercepts and neutralizes attacks",
    "Guest Context Scoping: Strictly confines tools to current session",
    "Max 5 Tool Calls: Limits loop and recursion depth per turn",
    "Per-Turn Idempotency: Blocks duplicate execution payloads",
    "Full Audit Trail: Registers every generation and action state",
  ];
  defenses.forEach((d, i) => {
    s.addText(`▸ ${d}`, {
      x: 0.4 + (i < 3 ? 0 : 4.8), y: 3.85 + (i < 3 ? i : i - 3) * 0.3, w: 4.4, h: 0.25, fontSize: 8.5, fontFace: F.B, color: C.TEXT_SEC,
    });
  });

  // Quote
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 4.8, w: 9.2, h: 0.35, fill: { color: "FFF8E8" },
    line: { color: C.GOLD, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText('"Read-only tools execute immediately. Side-effect tools require explicit guest confirmation. The guest always stays in control."', {
    x: 0.55, y: 4.82, w: 8.9, h: 0.3, fontSize: 9, fontFace: F.H, color: C.GOLD_DK, italic: true, align: "center",
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 13 — STRONG AGAINST MANIPULATION
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("Strong Against Manipulation");
  s.addText("AI That Acts, But Doesn't Assume", {
    x: 0.4, y: 0.75, w: 9.2, h: 0.3, fontSize: 12, fontFace: F.B, color: C.TEXT_SEC,
  });

  // THE PROBLEM
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 1.15, w: 4.4, h: 1.6, fill: { color: "FFF5F5" },
    line: { color: C.RED, width: 0.75 }, rectRadius: 0.08,
  });
  s.addText("THE PROBLEM", {
    x: 0.55, y: 1.2, w: 4.1, h: 0.25, fontSize: 12, fontFace: F.B, color: C.RED, bold: true,
  });
  s.addText("Regular Chatbots can be easily Manipulated to give out sensitive user data or perform actions that are beyond its scope.", {
    x: 0.55, y: 1.5, w: 4.1, h: 1.0, fontSize: 10, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Solution
  s.addShape(pptx.ShapeType.rect, {
    x: 5.2, y: 1.15, w: 4.4, h: 1.6, fill: { color: "F0F8F0" },
    line: { color: C.GREEN, width: 0.75 }, rectRadius: 0.08,
  });
  s.addText("Solution", {
    x: 5.35, y: 1.2, w: 4.1, h: 0.25, fontSize: 12, fontFace: F.B, color: C.GREEN, bold: true,
  });
  s.addText("Strong Input Sanitization", {
    x: 5.35, y: 1.5, w: 4.1, h: 0.25, fontSize: 11, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  s.addText("Every guest input goes through a sanitization layer that safeguards the actual LLM model, our system, and our data from malicious intent.", {
    x: 5.35, y: 1.8, w: 4.1, h: 0.7, fontSize: 10, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Defense details
  s.addText("Protection Layers", {
    x: 0.4, y: 3.0, w: 4, h: 0.3, fontSize: 14, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  const layers = [
    ["Pre-LLM Regex Sanitizer", "Blocks prompt injection patterns: 'ignore previous', 'system:', 'assistant:'"],
    ["Guest Context Scoping", "Tool execution scoped to current booking — IDs never leave trust boundary"],
    ["Proposal/Confirmation", "Side-effect tools require explicit guest confirmation before execution"],
    ["Audit Trail", "Every tool call, proposal, and action logged with correlation IDs"],
  ];
  layers.forEach((l, i) => {
    s.addShape(pptx.ShapeType.rect, {
      x: 0.4, y: 3.4 + i * 0.5, w: 9.2, h: 0.42, fill: { color: C.SURFACE },
      line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.05,
    });
    s.addText(l[0], {
      x: 0.55, y: 3.43 + i * 0.5, w: 2.8, h: 0.36, fontSize: 10, fontFace: F.B, color: C.GOLD_DK, bold: true, valign: "middle",
    });
    s.addText(l[1], {
      x: 3.4, y: 3.43 + i * 0.5, w: 6.0, h: 0.36, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC, valign: "middle",
    });
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 14 — SIGNALR ECOSYSTEM
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("Core Architectural Problem & Solution");
  s.addText("Orchestrating Real-Time Operations", {
    x: 0.4, y: 0.75, w: 9.2, h: 0.3, fontSize: 12, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Problem description
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 1.1, w: 9.2, h: 0.45, fill: { color: "FFF5F5" },
    line: { color: C.RED, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText("The Silo Problem: Without real-time alerts, departments operate in isolation. Housekeeping misses dirty room states, and the kitchen misses orders. The SignalR Ecosystem bridges this gap instantly.", {
    x: 0.55, y: 1.12, w: 8.9, h: 0.4, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Event table
  s.addTable([
    [{ text: "EVENT TRIGGER", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
     { text: "DEPARTMENT ALERTED", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
     { text: "REAL-TIME RESULT", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } }],
    ["Guest Checkout", "HousekeepingGroup", "Task auto-assigned"].map(t => ({ text: t, options: { fontSize: 8.5, fontFace: F.B } })),
    ["Food Order Logged", "KitchenGroup", "Instant dashboard refresh"].map(t => ({ text: t, options: { fontSize: 8.5, fontFace: F.B } })),
    ["Emergency Ticket", "MaintenanceGroup", "Dispatches instant alerts"].map(t => ({ text: t, options: { fontSize: 8.5, fontFace: F.B } })),
  ], {
    x: 0.4, y: 1.7, w: 9.2, colW: [2.8, 3.0, 3.4],
    border: { type: "solid", pt: 0.5, color: C.BORDER },
    rowH: 0.32,
  });

  // Code + Architecture
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 2.7, w: 4.2, h: 1.6, fill: { color: "F8F6F2" },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText("// Connect with explicit auth handshake\nconst connection = new HubConnectionBuilder()\n  .withUrl(\"/notifications\", {\n    accessTokenFactory: () => this.jwtToken\n  })\n  .build();\n\nawait connection.start();", {
    x: 0.55, y: 2.78, w: 3.9, h: 1.4, fontSize: 7.5, fontFace: F.M, color: C.TEXT, lineSpacingMultiple: 0.9,
  });

  // Reactive architecture
  s.addShape(pptx.ShapeType.rect, {
    x: 5.0, y: 2.7, w: 4.6, h: 1.6, fill: { color: C.SURFACE },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
  });
  s.addText("Reactive Frontend Architecture", {
    x: 5.15, y: 2.75, w: 4.3, h: 0.25, fontSize: 10, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  s.addText("The client intercepts real-time events as streaming RxJS streams. This triggers glassmorphic UI toast alerts and real-time dashboard state updates. Completely eliminates client-side polling.", {
    x: 5.15, y: 3.05, w: 4.3, h: 0.7, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Alert examples
  s.addText("[HousekeepingGroup] Task Active    [KitchenGroup] Food Order #204    [MaintenanceGroup] Emergency Ticket    Just now via Notifications Hub", {
    x: 0.4, y: 4.6, w: 9.2, h: 0.55, fontSize: 8.5, fontFace: F.M, color: C.GOLD_DK, lineSpacingMultiple: 1.0,
  });
  s.addText("SIGNALR PIPELINE • INTEGRITY VALIDATED", {
    x: 0.4, y: 5.1, w: 9.2, h: 0.2, fontSize: 7, fontFace: F.B, color: C.BORDER, align: "center",
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 15 — ENGINEERING EXCELLENCE
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("Engineering Excellence");
  s.addText("The Safety Net", {
    x: 0.4, y: 0.75, w: 9.2, h: 0.3, fontSize: 12, fontFace: F.B, color: C.TEXT_SEC,
  });

  const pillars = [
    { num: "01", title: "Transaction Safety", name: "Idempotency System", desc: "Uses an X-Idempotency-Key header to handle double-bookings and double-charges." },
    { num: "02", title: "Compliance & Audit", name: "Automated Audit Logging", desc: "Overrides SaveChangesAsync to capture old and new values in PostgreSQL JSONB." },
    { num: "03", title: "Access Control", name: "6-Role RBAC", desc: "Utilizes a 4-layer security model with protected controllers." },
    { num: "04", title: "Testing & Stability", name: "Code Quality", desc: "29 unit tests (93% BLL coverage), static analysis, and 47 EF Core migrations with Up/Down logic." },
  ];
  pillars.forEach((p, i) => {
    const col = i % 2;
    const row = Math.floor(i / 2);
    const cx = 0.4 + col * 4.8;
    const cy = 1.2 + row * 1.6;
    s.addShape(pptx.ShapeType.rect, {
      x: cx, y: cy, w: 4.4, h: 1.4, fill: { color: C.SURFACE },
      line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
    });
    s.addShape(pptx.ShapeType.ellipse, {
      x: cx + 0.15, y: cy + 0.15, w: 0.5, h: 0.5, fill: { color: C.GOLD },
    });
    s.addText(p.num, {
      x: cx + 0.15, y: cy + 0.15, w: 0.5, h: 0.5, fontSize: 16, fontFace: F.B,
      color: "FFFFFF", bold: true, align: "center", valign: "middle",
    });
    s.addText(p.title, {
      x: cx + 0.8, y: cy + 0.1, w: 3.4, h: 0.25, fontSize: 13, fontFace: F.B, color: C.TEXT, bold: true,
    });
    s.addText(p.name, {
      x: cx + 0.8, y: cy + 0.35, w: 3.4, h: 0.2, fontSize: 10, fontFace: F.B, color: C.GOLD_DK, bold: true,
    });
    s.addText(p.desc, {
      x: cx + 0.15, y: cy + 0.75, w: 4.1, h: 0.55, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
    });
  });

  s.addText('"These aren\'t nice-to-haves. They\'re what prevent 3 AM production calls."', {
    x: 0.4, y: SH - 0.5, w: 9.2, h: 0.3, fontSize: 11, fontFace: F.H, color: C.GOLD_DK, italic: true, align: "center",
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 16 — FROM PROBLEMS TO TRANSFORMATION
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("From Problems to Transformation");
  s.addText("System Evolution Matrix & Architectural Roadmap", {
    x: 0.4, y: 0.75, w: 9.2, h: 0.3, fontSize: 12, fontFace: F.B, color: C.TEXT_SEC,
  });

  const matrix = [
    ["CHALLENGE / PROBLEM", "ENGINEERED SOLUTION"].map(t => ({ text: t, options: { bold: true, fontSize: 8, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } })),
    ["Dangerous file uploads", "Zero-trust pipeline migration"].map(t => ({ text: t, options: { fontSize: 8, fontFace: F.B } })),
    ["Manual guest calls", "AI concierge (8 action types)"].map(t => ({ text: t, options: { fontSize: 8, fontFace: F.B } })),
    ["Siloed departments", "Real-time SignalR alerts"].map(t => ({ text: t, options: { fontSize: 8, fontFace: F.B } })),
    ["Double-clicks risk", "Strict idempotency prevention"].map(t => ({ text: t, options: { fontSize: 8, fontFace: F.B } })),
    ["Missing change history", "JSONB audit trails logging"].map(t => ({ text: t, options: { fontSize: 8, fontFace: F.B } })),
  ];
  s.addTable(matrix, {
    x: 0.4, y: 1.1, w: 9.2, colW: [4.3, 4.9],
    border: { type: "solid", pt: 0.5, color: C.BORDER },
    rowH: 0.3,
    margin: [2, 4, 2, 4],
  });

  // Future roadmap
  s.addText("FUTURE ROADMAP", {
    x: 0.4, y: 3.15, w: 4, h: 0.25, fontSize: 10, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  const roadmap = [
    "Voice input processing integration",
    "Proactive AI nudges system architecture",
    "Mobile native applications development",
  ];
  roadmap.forEach((r, i) => {
    s.addText(`${i + 1}. ${r}`, {
      x: 0.4, y: 3.45 + i * 0.3, w: 4.5, h: 0.25, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
    });
  });

  // Stats
  const stats = [
    ["~801", "Files Managed"],
    ["10,000+", "Symbols Integrated"],
    ["300", "Execution Flows"],
    ["6-Tier", "Multi-Role System"],
    ["< 2 Mos", "Speed to Delivery"],
  ];
  stats.forEach((st, i) => {
    const cx = 5.2 + (i >= 3 ? (i - 3) * 1.55 : i * 1.55);
    const cy = 3.2 + (i >= 3 ? 1.0 : 0);
    s.addShape(pptx.ShapeType.rect, {
      x: cx, y: cy, w: 1.4, h: 0.85, fill: { color: "FFF8E8" },
      line: { color: C.GOLD, width: 0.75 }, rectRadius: 0.06,
    });
    s.addText(st[0], {
      x: cx, y: cy + 0.05, w: 1.4, h: 0.4, fontSize: 16, fontFace: F.H, color: C.GOLD_DK, bold: true, align: "center",
    });
    s.addText(st[1], {
      x: cx, y: cy + 0.45, w: 1.4, h: 0.3, fontSize: 8, fontFace: F.B, color: C.TEXT_SEC, align: "center",
    });
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 17 — THANK YOU
// ═══════════════════════════════════════════════
(() => {
  const s = addSlide_();
  s.addShape(pptx.ShapeType.rect, { x: 0, y: 0, w: SW, h: SH, fill: { color: C.GOLD } });
  s.addShape(pptx.ShapeType.rect, { x: 0.04, y: 0.04, w: SW - 0.08, h: SH - 0.08, fill: { color: C.BG } });
  s.addText("Thank You.", {
    x: 0.5, y: 1.2, w: 9, h: 0.8, fontSize: 44, fontFace: F.H, color: C.TEXT, bold: true, align: "center",
  });
  s.addText("Questions?", {
    x: 0.5, y: 1.95, w: 9, h: 0.5, fontSize: 22, fontFace: F.B, color: C.GOLD_DK, align: "center",
  });
  s.addShape(pptx.ShapeType.rect, { x: 3.5, y: 2.5, w: 3, h: 0.03, fill: { color: C.GOLD } });
  s.addText("Let's discuss how Aetheris can transform your hotel operations.", {
    x: 0.5, y: 2.7, w: 9, h: 0.35, fontSize: 13, fontFace: F.B, color: C.TEXT_SEC, align: "center",
  });

  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 3.5, w: 9.2, h: 0.75, fill: { color: C.SURFACE },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
  });
  s.addText("Presented By", {
    x: 0.55, y: 3.55, w: 3, h: 0.2, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
  });
  s.addText("Nipun Sharma", {
    x: 0.55, y: 3.75, w: 3, h: 0.3, fontSize: 14, fontFace: F.B, color: C.TEXT, bold: true,
  });
  s.addText("nipunsharma@presidio.com", {
    x: 3.6, y: 3.75, w: 4, h: 0.3, fontSize: 10, fontFace: F.B, color: C.GOLD_DK,
  });
  s.addText("Aetheris — Built with Angular 22 • ASP.NET Core 10 • PostgreSQL • Azure", {
    x: 0.5, y: 4.5, w: 9, h: 0.3, fontSize: 10, fontFace: F.B, color: C.TEXT_SEC, align: "center",
  });
  s.addText("CONFIDENTIAL & PROPRIETARY", {
    x: 0.5, y: 5.0, w: 9, h: 0.25, fontSize: 8, fontFace: F.B, color: C.BORDER, align: "center",
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 18 — DIRECT-TO-AZURE UPLOADS (reprise)
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("Direct-to-Azure Uploads");
  s.addText("Securing the #1 Attack Vector with SAS Tokens", {
    x: 0.4, y: 0.75, w: 9.2, h: 0.3, fontSize: 12, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Problem
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 1.1, w: 9.2, h: 0.55, fill: { color: "FFF5F5" },
    line: { color: C.RED, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText("The Problem : File Uploads Are the #1 Attack Vector", {
    x: 0.55, y: 1.12, w: 8.9, h: 0.22, fontSize: 10, fontFace: F.B, color: C.RED, bold: true,
  });
  s.addText("Extension checks are trivial to bypass (renamed .exe passes). Server-buffered bytes expose backend to resource exhaustion.", {
    x: 0.55, y: 1.35, w: 8.9, h: 0.22, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Architecture steps
  s.addText("Direct Upload Architecture (Zero-Byte Server Overhead)", {
    x: 0.4, y: 1.8, w: 9.2, h: 0.25, fontSize: 11, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  const steps = [
    "① Browser ➔ API: POST /upload-sas (file size, name, type)",
    "② API ➔ Browser: Returns SAS URL (Write+Create, 15m expiry)",
    "③ Browser ➔ Azure: PUT file directly (bypasses server completely)",
    "④ Browser ➔ API: POST /confirm (notifies upload complete)",
    "⑤ API ➔ Queue: Enqueues to ImageValidationWorker",
  ];
  steps.forEach((st, i) => {
    s.addText(st, {
      x: 0.5, y: 2.1 + i * 0.28, w: 9.0, h: 0.25, fontSize: 8.5, fontFace: F.M, color: C.TEXT,
    });
  });

  // Value prop
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 3.65, w: 4.4, h: 0.6, fill: { color: "F0F8F0" },
    line: { color: C.GREEN, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText("The file never touches the backend. The server only generates a time-limited SAS token and returns the upload URL. The browser PUTs the file directly to Azure Blob Storage. No server memory consumed.", {
    x: 0.55, y: 3.7, w: 4.1, h: 0.5, fontSize: 8, fontFace: F.B, color: C.TEXT_SEC,
  });

  // SAS protection
  s.addShape(pptx.ShapeType.rect, {
    x: 5.2, y: 3.65, w: 4.4, h: 0.6, fill: { color: "FFF8E8" },
    line: { color: C.GOLD, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText("What the SAS token prevents: If intercepted, the token grants Write+Create on ONE blob only — no Read, no List, no Delete — and expires in 15 minutes.", {
    x: 5.35, y: 3.7, w: 4.1, h: 0.5, fontSize: 8, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Defense layers
  s.addText("Defense-in-Depth Layers", {
    x: 0.4, y: 4.4, w: 9.2, h: 0.25, fontSize: 10, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  const layers = [
    "MIME Accept Attribute", "Extension Whitelist", "Size Limit: 10MB Max",
    "SAS URL: 15-Min Expiry", "SAS: Write+Create Only", "Ownership Check",
    "Magic Byte Validation", "Post-Upload Verification", "3 Cleanup Workers",
  ];
  layers.forEach((l, i) => {
    s.addShape(pptx.ShapeType.rect, {
      x: 0.4 + (i % 3) * 3.1, y: 4.7 + Math.floor(i / 3) * 0.35, w: 2.9, h: 0.3, fill: { color: C.SURFACE },
      line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.04,
    });
    s.addText(l, {
      x: 0.45 + (i % 3) * 3.1, y: 4.72 + Math.floor(i / 3) * 0.35, w: 2.8, h: 0.26, fontSize: 8, fontFace: F.B, color: C.TEXT_SEC, valign: "middle",
    });
  });
})();

// ═══════════════════════════════════════════════
// WRITE FILE
// ═══════════════════════════════════════════════
pptx.writeFile({ fileName: "/Users/peewee/personal/repos/Hotel_Management_Full/presentation/v1_light.pptx" })
  .then(() => console.log("DONE: v1_light.pptx created"))
  .catch(e => console.error("ERROR", e));
