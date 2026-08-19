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

function headerSlide(title) {
  const s = addSlide_();
  s.addText(title, { x: 0.4, y: 0.2, w: 9.2, h: 0.45, fontSize: 21, fontFace: F.H, color: C.TEXT, bold: true });
  s.addShape(pptx.ShapeType.rect, { x: 0.4, y: 0.62, w: 2.2, h: 0.035, fill: { color: C.GOLD } });
  return s;
}

const pptx = new PptxGenJS();
pptx.defineLayout({ name: "WIDE", width: SW, height: SH });
pptx.layout = "WIDE";
pptx.author = "Aetheris";
pptx.title = "Aetheris — The Unified Hotel Operating System";
pptx.subject = "v2 Light Replica";

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
  s.addText("Security-First Architecture  •  AI-Powered Operations  •  Real-Time Coordination", {
    x: 0.5, y: 2.6, w: 9, h: 0.35, fontSize: 11, fontFace: F.B, color: C.TEXT_SEC, align: "center",
  });
  s.addText("From Magic Byte Validation to LLM-Powered Concierge — A Full-Stack Engineering Showcase", {
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
// SLIDE 3 — BROKEN STATE
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("The Broken State of Hotel Tech");
  s.addText("Most hotel software is a collection of disconnected tools held together by phone calls and spreadsheets.", {
    x: 0.4, y: 0.75, w: 9.2, h: 0.3, fontSize: 11, fontFace: F.B, color: C.TEXT_SEC, italic: true,
  });
  const problems = [
    ["Security Vulnerabilities", "File uploads accept any extension — renamed malware passes through. No content verification. Your server buffers every file."],
    ["No Guest Self-Service", "Guests call the front desk for everything — towels, food, maintenance questions. 24/7 support is impossible."],
    ["Siloed Departments", "Housekeeping doesn't know about checkout until someone calls. Kitchen doesn't see orders until they check the printer. Maintenance loses paper tickets."],
    ["Fragile Engineering", "No idempotency = double-bookings from double-clicks. No audit trail = billing disputes unprovable. No real-time = stale dashboards."],
  ];
  problems.forEach((p, i) => {
    const col = i % 2;
    const row = Math.floor(i / 2);
    const cx = 0.4 + col * 4.8;
    const cy = 1.2 + row * 1.55;
    s.addShape(pptx.ShapeType.rect, {
      x: cx, y: cy, w: 4.4, h: 1.35, fill: { color: "FFF5F5" },
      line: { color: C.RED, width: 0.5 }, rectRadius: 0.08,
    });
    s.addShape(pptx.ShapeType.rect, {
      x: cx, y: cy, w: 0.06, h: 1.35, fill: { color: C.RED },
    });
    s.addText(p[0], {
      x: cx + 0.2, y: cy + 0.08, w: 4.0, h: 0.25, fontSize: 12, fontFace: F.B, color: C.RED, bold: true,
    });
    s.addText(p[1], {
      x: cx + 0.2, y: cy + 0.38, w: 4.0, h: 0.85, fontSize: 9.5, fontFace: F.B, color: C.TEXT_SEC,
    });
  });
  s.addText('"Generic hotel software manages records. Aetheris runs operations."', {
    x: 0.4, y: SH - 0.45, w: 9.2, h: 0.3, fontSize: 12, fontFace: F.H, color: C.GOLD_DK, italic: true, align: "center",
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 4 — PLATFORM OVERVIEW
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("Aetheris — Platform Overview");
  const layers = [
    { label: "Angular 22 (SPA, Signals, Material)", sub: "REST + SignalR", y: 1.05 },
    { label: "ASP.NET Core 10 API (16 Controllers)", y: 1.5 },
    { label: "BLL Services (19 services, 5 workers)", y: 1.95 },
    { label: "Generic Repository Layer (13 repos)", y: 2.4 },
    { label: "EF Core PostgreSQL (18 entities, JSONB)", y: 2.85 },
    { label: "Azure Infrastructure: Blob, Queue, Container Apps, ACR", y: 3.3, highlight: true },
  ];
  layers.forEach((l, i) => {
    s.addShape(pptx.ShapeType.rect, {
      x: 0.4, y: l.y, w: 4.2, h: 0.38, fill: { color: l.highlight ? "FFF8E8" : C.SURFACE },
      line: { color: l.highlight ? C.GOLD : C.BORDER, width: l.highlight ? 1.5 : 0.75 }, rectRadius: 0.05,
    });
    s.addText(l.label, {
      x: 0.5, y: l.y + 0.03, w: 4.0, h: l.sub ? 0.2 : 0.32, fontSize: 9, fontFace: F.M, color: C.TEXT, bold: true,
    });
    if (l.sub) s.addText(l.sub, {
      x: 0.5, y: l.y + 0.2, w: 3.8, h: 0.16, fontSize: 7.5, fontFace: F.M, color: C.TEXT_SEC,
    });
    if (i < 5) s.addText("▼", { x: 2.3, y: l.y + 0.36, w: 0.3, h: 0.12, fontSize: 8, color: C.GOLD, align: "center" });
    if (i === 0 || i === 3) s.addText("❮", { x: 4.65, y: l.y + 0.04, w: 0.25, h: 0.3, fontSize: 14, color: C.GOLD });
  });

  const stats = [
    ["6 User Roles", "Admin, FrontDesk, Guest, Kitchen, Housekeeping, Maintenance"],
    ["7 Angular Route Guards", "Defense in depth — role-based SPA access"],
    ["~60 API Endpoints", "Full REST coverage across 16 controllers"],
    ["Angular 22 + Signals", "Fine-grained reactivity, no Zone.js"],
    ["Static Analysis", "Warnings as Errors — production-grade C#"],
  ];
  stats.forEach((st, i) => {
    const sy = 1.05 + i * 0.55;
    s.addShape(pptx.ShapeType.rect, {
      x: 5.1, y: sy, w: 4.5, h: 0.45, fill: { color: C.SURFACE },
      line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.05,
    });
    s.addText(st[0], {
      x: 5.25, y: sy + 0.02, w: 4.2, h: 0.2, fontSize: 10, fontFace: F.B, color: C.GOLD_DK, bold: true,
    });
    s.addText(st[1], {
      x: 5.25, y: sy + 0.22, w: 4.2, h: 0.2, fontSize: 8, fontFace: F.B, color: C.TEXT_SEC,
    });
  });

  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 3.95, w: 9.2, h: 0.85, fill: { color: "FFF8E8" },
    line: { color: C.GOLD, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText("N-Tier Architecture with Strict Dependency Direction", {
    x: 0.55, y: 4.0, w: 8.9, h: 0.25, fontSize: 11, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  s.addText("API → BLL → Repository → DAL. Layers enforce separation at the .csproj level. EF Core lives only in DAL — swap persistence without touching business logic.", {
    x: 0.55, y: 4.3, w: 8.9, h: 0.4, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 5 — ZERO-TRUST UPLOAD
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("Problem: File Uploads Are the #1 Attack Vector");
  // Problem
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 1.0, w: 4.4, h: 1.0, fill: { color: "FFF5F5" },
    line: { color: C.RED, width: 0.5 }, rectRadius: 0.08,
  });
  s.addText("The Problem", {
    x: 0.55, y: 1.05, w: 4.1, h: 0.2, fontSize: 10, fontFace: F.B, color: C.RED, bold: true,
  });
  s.addText("Extension-only checks are trivial to bypass. A renamed .exe passes. Your server buffers every byte. Orphaned blobs accumulate and cost money.", {
    x: 0.55, y: 1.3, w: 4.1, h: 0.6, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Solution
  s.addText("Solution: Direct-to-Azure Upload with SAS Tokens", {
    x: 5.2, y: 1.0, w: 4.4, h: 0.25, fontSize: 11, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  // Flow
  const flow = [
    "[Browser] ──PUT──▶ [Azure Blob Storage]",
    "    ▲                           ",
    "    │ POST /upload-sas           ",
    "    │ SAS URL (Write+Create)     ",
    "    │ 15min expiry                ",
    "  [ASP.NET Core] ──────────────── ",
  ];
  s.addShape(pptx.ShapeType.rect, {
    x: 5.2, y: 1.3, w: 4.4, h: 1.2, fill: { color: "F8F6F2" },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText(flow.join("\n"), {
    x: 5.3, y: 1.35, w: 4.2, h: 1.1, fontSize: 7.5, fontFace: F.M, color: C.TEXT, lineSpacingMultiple: 0.9,
  });

  // Defense layers
  s.addText("Defense-in-Depth Layers", {
    x: 0.4, y: 2.25, w: 4, h: 0.25, fontSize: 11, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  const defenses = [
    "MIME accept attribute (browser hint)",
    "Extension whitelist (.jpg/.jpeg/.png/.webp)",
    "Size limit: 10MB max",
    "SAS URL: 15-minute expiry",
    "SAS permissions: Write+Create only",
    "Ownership check: only uploader can confirm",
    "Magic byte validation (next slide)",
    "Post-upload size verification",
    "3 cleanup workers",
  ];
  defenses.forEach((d, i) => {
    s.addText(`✅ ${d}`, {
      x: 0.4 + (i < 5 ? 0 : 4.8), y: 2.55 + (i < 5 ? i : i - 5) * 0.3, w: 4.5, h: 0.25, fontSize: 8.5, fontFace: F.B, color: C.TEXT_SEC,
    });
  });

  // Key quote
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 4.4, w: 9.2, h: 0.5, fill: { color: "F0F8F0" },
    line: { color: C.GREEN, width: 0.5 }, rectRadius: 0.06,
  });
  s.addText('"The file never passes through the backend. No server memory or bandwidth consumed."', {
    x: 0.55, y: 4.42, w: 8.9, h: 0.45, fontSize: 11, fontFace: F.H, color: C.GREEN, italic: true, align: "center", valign: "middle",
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 6 — MAGIC BYTE VALIDATION
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("Magic Byte Validation — Beyond Extension Checking");
  // Problem
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 1.0, w: 4.4, h: 0.75, fill: { color: "FFF5F5" },
    line: { color: C.RED, width: 0.5 }, rectRadius: 0.08,
  });
  s.addText("The Problem", {
    x: 0.55, y: 1.05, w: 4.1, h: 0.2, fontSize: 10, fontFace: F.B, color: C.RED, bold: true,
  });
  s.addText("A file named image.jpg with executable content passes every extension check. Most systems stop here.", {
    x: 0.55, y: 1.28, w: 4.1, h: 0.4, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Solution
  s.addShape(pptx.ShapeType.rect, {
    x: 5.2, y: 1.0, w: 4.4, h: 0.75, fill: { color: "F0F8F0" },
    line: { color: C.GREEN, width: 0.5 }, rectRadius: 0.08,
  });
  s.addText("Solution", {
    x: 5.35, y: 1.05, w: 4.1, h: 0.2, fontSize: 10, fontFace: F.B, color: C.GREEN, bold: true,
  });
  s.addText("Read the actual binary header and validate against known file signatures.", {
    x: 5.35, y: 1.28, w: 4.1, h: 0.4, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Magic bytes table
  s.addTable([
    [{ text: "Format", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
     { text: "Magic Bytes (Hex)", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
     { text: "Validation", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } }],
    [{ text: "JPEG", options: { fontSize: 9, fontFace: F.M } },
     { text: "FF D8 FF", options: { fontSize: 9, fontFace: F.M } },
     { text: "First 3 bytes", options: { fontSize: 9, fontFace: F.B } }],
    [{ text: "PNG", options: { fontSize: 9, fontFace: F.M } },
     { text: "89 50 4E 47 0D 0A 1A 0A", options: { fontSize: 9, fontFace: F.M } },
     { text: "Full 8-byte signature", options: { fontSize: 9, fontFace: F.B } }],
    [{ text: "WebP", options: { fontSize: 9, fontFace: F.M } },
     { text: "52 49 46 46 .... 57 45 42 50", options: { fontSize: 9, fontFace: F.M } },
     { text: "RIFF + WEBP marker", options: { fontSize: 9, fontFace: F.B } }],
  ], {
    x: 0.4, y: 2.0, w: 9.2, colW: [1.5, 4.2, 3.5],
    border: { type: "solid", pt: 0.5, color: C.BORDER },
    rowH: 0.3,
    margin: [2, 4, 2, 4],
  });

  // Code snippet
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 3.0, w: 9.2, h: 1.3, fill: { color: "F8F6F2" },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.06,
  });
  const code = [
    '// Reads first 512 bytes from blob. No trusting file extensions.',
    'var header = new byte[512];',
    'await blob.DownloadToAsync(header);',
    '',
    'if (!IsValidMagicBytes(header, declaredExtension))',
    '{',
    '    await blob.DeleteAsync(); // Reject + delete immediately',
    '    session.Status = UploadStatus.Rejected;',
    '    session.RejectionReason = $"Magic byte mismatch for {declaredExtension}";',
    '}',
  ];
  s.addText(code.join("\n"), {
    x: 0.55, y: 3.05, w: 8.9, h: 1.2, fontSize: 8.5, fontFace: F.M, color: C.TEXT, lineSpacingMultiple: 0.9,
  });
  s.addText("This is the same technique security scanners use to detect disguised executables. Enterprise-grade content validation in a hotel system.", {
    x: 0.4, y: 4.5, w: 9.2, h: 0.4, fontSize: 10, fontFace: F.H, color: C.GOLD_DK, italic: true, align: "center",
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 7 — ASYNC & CLEANUP
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("Async Processing & Storage Economics");
  s.addText("Validation shouldn't block HTTP requests. Orphaned blobs cost money.", {
    x: 0.4, y: 0.75, w: 9.2, h: 0.3, fontSize: 11, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Flow diagram
  const flowLines = [
    "[POST /confirm] → [Azure Queue] → [ImageValidationWorker]",
    "                           │",
    "         ┌─────────────────┼─────────────────┐",
    "         ▼                 ▼                 ▼",
    "   Magic Bytes        Size Verify      Reject/Delete",
    "         │                 │                 │",
    "         └─────────────────┼─────────────────┘",
    "                           ▼",
    "              ┌─────────────────────┐",
    "              │ Status: Confirmed   │  Status: Rejected",
    "              │ Blob Kept           │  Blob Deleted + Reason",
    "              └─────────────────────┘",
  ];
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 1.1, w: 9.2, h: 1.6, fill: { color: "F8F6F2" },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText(flowLines.join("\n"), {
    x: 0.5, y: 1.15, w: 9.0, h: 1.5, fontSize: 7, fontFace: F.M, color: C.TEXT, lineSpacingMultiple: 0.85,
  });

  // Worker cards
  const workers = [
    { title: "ImageValidationWorker", sub: "Queue-triggered, 2s poll", desc: "Magic bytes + size check. Valid → Confirmed. Invalid → delete blob + record rejection." },
    { title: "OrphanImageCleanupWorker", sub: "Hourly cron", desc: "Expires stale Pending (>1h) and unattached Confirmed (>24h) sessions. Deletes orphaned blobs." },
    { title: "BlobCleanupWorker", sub: "Hourly + on startup", desc: "Full container scan — cross-references ALL blobs against UploadSessions + entity URL fields." },
  ];
  workers.forEach((w, i) => {
    const cx = 0.35 + i * 3.2;
    s.addShape(pptx.ShapeType.rect, {
      x: cx, y: 2.85, w: 3.0, h: 1.35, fill: { color: C.SURFACE },
      line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.06,
    });
    s.addShape(pptx.ShapeType.rect, {
      x: cx, y: 2.85, w: 3.0, h: 0.04, fill: { color: C.GOLD },
    });
    s.addText(w.title, {
      x: cx + 0.1, y: 2.95, w: 2.8, h: 0.2, fontSize: 9.5, fontFace: F.M, color: C.TEXT, bold: true,
    });
    s.addText(w.sub, {
      x: cx + 0.1, y: 3.15, w: 2.8, h: 0.18, fontSize: 8, fontFace: F.B, color: C.GOLD_DK,
    });
    s.addText(w.desc, {
      x: cx + 0.1, y: 3.35, w: 2.8, h: 0.8, fontSize: 8, fontFace: F.B, color: C.TEXT_SEC,
    });
  });

  // State machine
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 4.35, w: 9.2, h: 0.8, fill: { color: "FFF8E8" },
    line: { color: C.GOLD, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText("State Machine:  Pending → Confirmed → Attached → [Entity retains URL]    |    Pending → Rejected → [Blob deleted]    |    Any → Expired [Auto-cleanup]", {
    x: 0.55, y: 4.38, w: 8.9, h: 0.35, fontSize: 9, fontFace: F.M, color: C.TEXT,
  });
  s.addText("Three overlapping workers ensure zero storage leaks. Defense in depth for your Azure bill.", {
    x: 0.55, y: 4.75, w: 8.9, h: 0.25, fontSize: 9, fontFace: F.B, color: C.GOLD_DK, italic: true,
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 8 — AI CONCIERGE
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("Problem: Guests Need 24/7 Service. Staff Can't Be Everywhere.");
  s.addText("Guests call the front desk for every request — extra towels, food orders, maintenance issues. At 2 AM, there's no one to call.", {
    x: 0.4, y: 0.75, w: 9.2, h: 0.3, fontSize: 11, fontFace: F.B, color: C.TEXT_SEC,
  });

  s.addText("Solution: Aetheris AI Concierge — Powered by OpenAI gpt-4o-mini", {
    x: 0.4, y: 1.1, w: 9.2, h: 0.3, fontSize: 12, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });

  // Architecture
  const archFlow = [
    "[Guest Chat UI] ──▶ [ASP.NET Core API] ──▶ [Concierge Orchestrator]",
    "                                                  │",
    "                     ┌────────────────────────────┼────────────────────────────┐",
    "                     ▼                            ▼                            ▼",
    "              [OpenAI gpt-4o-mini]          [Tool Executor]             [SignalR Broadcast]",
    "              8 function tools           Centralized dispatch          Kitchen / Housekeeping",
    "                                        to existing BLL services       / Maintenance groups",
  ];
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 1.45, w: 9.2, h: 1.2, fill: { color: "F8F6F2" },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText(archFlow.join("\n"), {
    x: 0.5, y: 1.5, w: 9.0, h: 1.1, fontSize: 7, fontFace: F.M, color: C.TEXT, lineSpacingMultiple: 0.85,
  });

  // Tools grid
  s.addText("The 8 Function Tools", {
    x: 0.4, y: 2.85, w: 4, h: 0.25, fontSize: 11, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  // Side-effect tools
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 3.15, w: 4.4, h: 1.6, fill: { color: "FFF8E8" },
    line: { color: C.GOLD, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText("Side-Effect Tools (Need Confirmation)", {
    x: 0.55, y: 3.2, w: 4.1, h: 0.2, fontSize: 9, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  const sideTools = [
    "create_food_order — Place room service",
    "create_housekeeping_request — Towels, cleaning",
    "create_maintenance_ticket — AC, plumbing, TV",
  ];
  sideTools.forEach((t, i) => s.addText(t, {
    x: 0.55, y: 3.5 + i * 0.35, w: 4.1, h: 0.3, fontSize: 8.5, fontFace: F.M, color: C.TEXT_SEC,
  }));

  // Read-only tools
  s.addShape(pptx.ShapeType.rect, {
    x: 5.2, y: 3.15, w: 4.4, h: 1.6, fill: { color: C.SURFACE },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText("Read-Only Tools (Execute Immediately)", {
    x: 5.35, y: 3.2, w: 4.1, h: 0.2, fontSize: 9, fontFace: F.B, color: C.TEXT, bold: true,
  });
  const readTools = [
    "get_booking_info — Check-in/out, room",
    "get_folio_balance — Current bill",
    "get_housekeeping_status — Room status",
    "get_menu_items — Browse menu",
    "get_active_orders — Current orders",
  ];
  readTools.forEach((t, i) => s.addText(t, {
    x: 5.35, y: 3.5 + i * 0.3, w: 4.1, h: 0.25, fontSize: 8.5, fontFace: F.M, color: C.TEXT_SEC,
  }));

  // Key differentiator
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 4.9, w: 9.2, h: 0.4, fill: { color: "FFF8E8" },
    line: { color: C.GOLD, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText("Zero new business logic. The concierge reuses existing OrderService, HousekeepingService, MaintenanceService — the same services the staff dashboards use. When the AI places a food order, the Kitchen dashboard lights up in real-time via SignalR.", {
    x: 0.55, y: 4.92, w: 8.9, h: 0.35, fontSize: 8.5, fontFace: F.B, color: C.TEXT_SEC,
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 9 — PROPOSAL PATTERN
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("The Proposal Pattern — AI That Acts, But Doesn't Assume");
  // Problem
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 0.9, w: 4.4, h: 0.65, fill: { color: "FFF5F5" },
    line: { color: C.RED, width: 0.5 }, rectRadius: 0.06,
  });
  s.addText("The Problem", {
    x: 0.55, y: 0.95, w: 4.1, h: 0.2, fontSize: 10, fontFace: F.B, color: C.RED, bold: true,
  });
  s.addText("LLMs hallucinate. They execute tools confidently but incorrectly. A chatbot that can place orders without confirmation is dangerous.", {
    x: 0.55, y: 1.18, w: 4.1, h: 0.3, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Solution
  s.addShape(pptx.ShapeType.rect, {
    x: 5.2, y: 0.9, w: 4.4, h: 0.65, fill: { color: "FFF8E8" },
    line: { color: C.GOLD, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText("Solution: Two-Step Action Pattern", {
    x: 5.35, y: 0.95, w: 4.1, h: 0.2, fontSize: 10, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  s.addText("Read-only tools execute immediately. Side-effect tools require explicit guest confirmation.", {
    x: 5.35, y: 1.18, w: 4.1, h: 0.3, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Flow diagram
  const flowDiagram = [
    'Step 1: Guest types "I need a burger and extra towels"',
    "                  │",
    "                  ▼",
    "   LLM calls tools → System creates PROPOSALS",
    "   (pending state, 5-minute TTL)",
    "                  │",
    "                  ▼",
    '   Guest sees: "Proposed: Order Burger ×1,',
    '   Housekeeping: Extra towels"',
    "   [Confirm & Execute] button presented in chat",
    "                  │",
    "   Step 2: Guest taps Confirm",
    "                  │",
    "                  ▼",
    "   Proposals executed → BLL services called",
    "   → SignalR alerts fired → AI replies with confirmation",
  ];
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 1.7, w: 4.4, h: 2.5, fill: { color: "F8F6F2" },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText(flowDiagram.join("\n"), {
    x: 0.5, y: 1.75, w: 4.2, h: 2.4, fontSize: 7, fontFace: F.M, color: C.TEXT, lineSpacingMultiple: 0.85,
  });

  // Defense layers
  s.addText("Security Defense Layers", {
    x: 5.2, y: 1.7, w: 4.4, h: 0.25, fontSize: 11, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  const defenses = [
    "Proposal TTL: 5 minutes — auto-expires if not confirmed",
    "Prompt injection sanitizer: Strips ignore previous, system:, assistant: patterns",
    "Guest context scoping: Booking ID, room ID never exposed to LLM — resolved server-side from JWT",
    "Max 5 tool calls per turn: Prevents runaway LLM loops",
    "Per-turn idempotency: Duplicate LLM calls never double-execute",
    "Full audit trail: Every tool call logged with arguments + outcome",
  ];
  defenses.forEach((d, i) => {
    s.addText(`▸ ${d}`, {
      x: 5.2, y: 2.05 + i * 0.35, w: 4.4, h: 0.3, fontSize: 8.5, fontFace: F.B, color: C.TEXT_SEC,
    });
  });

  s.addText('"Read-only tools execute immediately. Side-effect tools require confirmation. The guest stays in control. The AI executes precisely."', {
    x: 0.4, y: 4.55, w: 9.2, h: 0.4, fontSize: 10, fontFace: F.H, color: C.GOLD_DK, italic: true, align: "center",
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 10 — SIGNALR REAL-TIME
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("Problem: Departments Operate in Silos");
  s.addText("Without real-time alerts: Housekeeping doesn't know a room is dirty. Kitchen doesn't see new orders. Maintenance misses urgent tickets.", {
    x: 0.4, y: 0.75, w: 9.2, h: 0.3, fontSize: 11, fontFace: F.B, color: C.TEXT_SEC,
  });

  s.addText("Solution: Role-Based SignalR Broadcasting", {
    x: 0.4, y: 1.1, w: 9.2, h: 0.3, fontSize: 12, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });

  // Event table
  s.addTable([
    [{ text: "Event", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
     { text: "Department Alerted", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
     { text: "Result", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } }],
    ["Guest checks out", "HousekeepingGroup", "Room cleaning task auto-created"].map(t => ({ text: t, options: { fontSize: 8.5, fontFace: F.B } })),
    ["Guest orders food", "KitchenGroup", "Order appears on kitchen dashboard"].map(t => ({ text: t, options: { fontSize: 8.5, fontFace: F.B } })),
    ["Guest reports issue", "MaintenanceGroup", "Ticket created, emergency flag if needed"].map(t => ({ text: t, options: { fontSize: 8.5, fontFace: F.B } })),
  ], {
    x: 0.4, y: 1.5, w: 9.2, colW: [2.8, 3.0, 3.4],
    border: { type: "solid", pt: 0.5, color: C.BORDER },
    rowH: 0.32,
  });

  // Connection code
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 2.7, w: 4.4, h: 1.6, fill: { color: "F8F6F2" },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText("Connection Architecture", {
    x: 0.55, y: 2.75, w: 4.1, h: 0.2, fontSize: 9, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  const connCode = [
    "1. User authenticates → receives JWT",
    "2. Frontend connects:",
    "   HubConnection(\"/notifications?",
    "     access_token={jwt}\")",
    "3. Server reads user role from claims",
    "4. Server: Groups.AddToGroupAsync(",
    "   Context, role + \"Group\")",
    "5. Connection: withAutomaticReconnect()",
    "   — retries 0s, 2s, 10s, 30s",
  ];
  s.addText(connCode.join("\n"), {
    x: 0.55, y: 2.98, w: 4.1, h: 1.2, fontSize: 7.5, fontFace: F.M, color: C.TEXT, lineSpacingMultiple: 0.85,
  });

  // RxJS description
  s.addShape(pptx.ShapeType.rect, {
    x: 5.2, y: 2.7, w: 4.4, h: 1.6, fill: { color: C.SURFACE },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
  });
  s.addText("Reactive Frontend Architecture", {
    x: 5.35, y: 2.75, w: 4.1, h: 0.2, fontSize: 10, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  s.addText("Frontend receives events as RxJS streams → glassmorphism toast notifications with auto-stacking → dashboards refresh in real-time. No polling. No page refreshes. Zero configuration per event type.", {
    x: 5.35, y: 3.05, w: 4.1, h: 1.0, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Alert examples
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 4.55, w: 9.2, h: 0.5, fill: { color: "FFF8E8" },
    line: { color: C.GOLD, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText("[HousekeepingGroup] Task Active    [KitchenGroup] Food Order #204    [MaintenanceGroup] Emergency Ticket    Just now via Notifications Hub", {
    x: 0.55, y: 4.6, w: 8.9, h: 0.4, fontSize: 8.5, fontFace: F.M, color: C.GOLD_DK, lineSpacingMultiple: 1.0, valign: "middle",
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 11 — ENGINEERING FOUNDATIONS
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("Engineering Excellence — The Safety Net");
  const pillars = [
    { num: "01", title: "Idempotency System", desc: "Every mutation carries X-Idempotency-Key. Duplicates detected → original response replayed from DB. Race conditions handled via PK conflict (first writer wins). Cleanup every 6 hours (48h TTL). Prevents: double-bookings, double-charges, duplicate order processing." },
    { num: "02", title: "Automated Audit Logging", desc: "SaveChangesAsync() override captures every entity change — old values + new values stored as PostgreSQL JSONB. Zero developer effort. Covers all 18 entity types. Self-auditing (audit logs audit themselves). Forensic traceability for billing disputes." },
    { num: "03", title: "6-Role RBAC — Defense in Depth", desc: "4-layer security model: Frontend Route Guards → Backend Authorize → BLL Service Checks → Repository Query Scoping. 16 protected controllers, 7 Angular route guards. Role-specific shells for Admin, FrontDesk, Guest, Kitchen, Housekeeping, Maintenance." },
    { num: "04", title: "Code Quality", desc: "29 unit tests (93% BLL coverage via NUnit + Moq). Static analysis: latest-All rules, warnings are errors. 47 EF Core migrations with Up()/Down() reversibility. Generic Repository + Generic CRUD component = 0 duplicated data access or management UI code." },
  ];
  pillars.forEach((p, i) => {
    const col = i % 2;
    const row = Math.floor(i / 2);
    const cx = 0.4 + col * 4.8;
    const cy = 1.0 + row * 1.85;
    s.addShape(pptx.ShapeType.rect, {
      x: cx, y: cy, w: 4.4, h: 1.65, fill: { color: C.SURFACE },
      line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
    });
    s.addShape(pptx.ShapeType.ellipse, {
      x: cx + 0.12, y: cy + 0.1, w: 0.4, h: 0.4, fill: { color: C.GOLD },
    });
    s.addText(p.num, {
      x: cx + 0.12, y: cy + 0.1, w: 0.4, h: 0.4, fontSize: 14, fontFace: F.B,
      color: "FFFFFF", bold: true, align: "center", valign: "middle",
    });
    s.addText(p.title, {
      x: cx + 0.65, y: cy + 0.08, w: 3.55, h: 0.25, fontSize: 12, fontFace: F.B, color: C.TEXT, bold: true,
    });
    s.addText(p.desc, {
      x: cx + 0.12, y: cy + 0.55, w: 4.15, h: 1.0, fontSize: 8.5, fontFace: F.B, color: C.TEXT_SEC,
    });
  });
  s.addText('"These aren\'t nice-to-haves. They\'re what prevent 3 AM production calls."', {
    x: 0.4, y: SH - 0.4, w: 9.2, h: 0.25, fontSize: 11, fontFace: F.H, color: C.GOLD_DK, italic: true, align: "center",
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 12 — BUSINESS IMPACT
// ═══════════════════════════════════════════════
(() => {
  const s = headerSlide("From Problems to Transformation");

  // Problems solved
  s.addText("Problems → Solved", {
    x: 0.4, y: 0.85, w: 4.5, h: 0.25, fontSize: 12, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  const solutions = [
    ["File uploads are dangerous", "Zero-trust pipeline with magic byte validation"],
    ["Guests call for everything", "AI concierge handles 24/7 inquiries, 8 action types"],
    ["Departments don't coordinate", "Real-time SignalR alerts across 3 groups"],
    ["Double-clicks cause chaos", "Idempotency prevents every duplicate"],
    ["No change history", "JSONB audit trail on all 18 entities"],
    ["Manual spreadsheets", "5 automated KPIs (Occupancy, RevPAR, ADR, etc.)"],
  ];
  solutions.forEach((sol, i) => {
    s.addShape(pptx.ShapeType.rect, {
      x: 0.4, y: 1.15 + i * 0.4, w: 4.5, h: 0.35, fill: { color: C.SURFACE },
      line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.04,
    });
    s.addText(sol[0], {
      x: 0.5, y: 1.15 + i * 0.4, w: 1.9, h: 0.35, fontSize: 7.5, fontFace: F.B, color: C.TEXT_SEC, valign: "middle",
    });
    s.addText("→", {
      x: 2.35, y: 1.15 + i * 0.4, w: 0.3, h: 0.35, fontSize: 10, fontFace: F.B, color: C.GOLD, valign: "middle", align: "center",
    });
    s.addText(sol[1], {
      x: 2.6, y: 1.15 + i * 0.4, w: 2.2, h: 0.35, fontSize: 7.5, fontFace: F.B, color: C.GOLD_DK, valign: "middle", bold: true,
    });
  });

  // Roadmap
  s.addText("Roadmap", {
    x: 5.2, y: 0.85, w: 4.4, h: 0.25, fontSize: 12, fontFace: F.B, color: C.GOLD_DK, bold: true,
  });
  const roadmap = [
    "Image transformation pipeline (auto-resize, optimize)",
    "CDN integration (Azure Front Door)",
    "Multilingual AI concierge",
    "Voice input for concierge (Web Speech API → STT)",
    'Proactive AI nudges ("It\'s 7:30 AM — your usual latte?")',
    "Mobile native apps",
  ];
  roadmap.forEach((r, i) => {
    s.addText(`${i + 1}. ${r}`, {
      x: 5.2, y: 1.15 + i * 0.35, w: 4.4, h: 0.3, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
    });
  });

  // Metrics
  const metrics = [
    ["~801", "Files Managed"],
    ["10,000+", "Symbols Integrated"],
    ["300", "Execution Flows"],
    ["6-Tier", "Multi-Role System"],
    ["< 2 Mos", "Speed to Delivery"],
  ];
  metrics.forEach((m, i) => {
    const cx = 0.4 + i * 1.9;
    s.addShape(pptx.ShapeType.rect, {
      x: cx, y: 3.65, w: 1.7, h: 0.95, fill: { color: "FFF8E8" },
      line: { color: C.GOLD, width: 0.75 }, rectRadius: 0.06,
    });
    s.addText(m[0], {
      x: cx, y: 3.68, w: 1.7, h: 0.45, fontSize: 18, fontFace: F.H, color: C.GOLD_DK, bold: true, align: "center",
    });
    s.addText(m[1], {
      x: cx, y: 4.15, w: 1.7, h: 0.4, fontSize: 8.5, fontFace: F.B, color: C.TEXT_SEC, align: "center",
    });
  });
})();

// ═══════════════════════════════════════════════
// SLIDE 13 — Q&A / THANK YOU
// ═══════════════════════════════════════════════
(() => {
  const s = addSlide_();
  s.addShape(pptx.ShapeType.rect, { x: 0, y: 0, w: SW, h: SH, fill: { color: C.GOLD } });
  s.addShape(pptx.ShapeType.rect, { x: 0.04, y: 0.04, w: SW - 0.08, h: SH - 0.08, fill: { color: C.BG } });
  s.addText("Thank You.", {
    x: 0.5, y: 1.0, w: 9, h: 0.8, fontSize: 44, fontFace: F.H, color: C.TEXT, bold: true, align: "center",
  });
  s.addText("Questions?", {
    x: 0.5, y: 1.75, w: 9, h: 0.5, fontSize: 22, fontFace: F.B, color: C.GOLD_DK, align: "center",
  });
  s.addShape(pptx.ShapeType.rect, { x: 3.5, y: 2.3, w: 3, h: 0.03, fill: { color: C.GOLD } });
  s.addText("Let's discuss how Aetheris can transform your hotel operations.", {
    x: 0.5, y: 2.5, w: 9, h: 0.35, fontSize: 13, fontFace: F.B, color: C.TEXT_SEC, align: "center",
  });

  // Contact
  s.addShape(pptx.ShapeType.rect, {
    x: 0.4, y: 3.2, w: 9.2, h: 0.65, fill: { color: C.SURFACE },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
  });
  s.addText("Your Name", {
    x: 0.55, y: 3.25, w: 3, h: 0.25, fontSize: 14, fontFace: F.B, color: C.TEXT, bold: true,
  });
  s.addText("your@email.com", {
    x: 3.6, y: 3.25, w: 4, h: 0.25, fontSize: 10, fontFace: F.B, color: C.GOLD_DK,
  });
  s.addText("github.com/yourhandle", {
    x: 3.6, y: 3.55, w: 4, h: 0.25, fontSize: 10, fontFace: F.B, color: C.GOLD_DK,
  });

  s.addText("Aetheris — Built with Angular 22  •  ASP.NET Core 10  •  PostgreSQL  •  Azure", {
    x: 0.5, y: 4.3, w: 9, h: 0.3, fontSize: 10, fontFace: F.B, color: C.TEXT_SEC, align: "center",
  });
  s.addText("CONFIDENTIAL & PROPRIETARY", {
    x: 0.5, y: 4.9, w: 9, h: 0.25, fontSize: 8, fontFace: F.B, color: C.BORDER, align: "center",
  });
})();

pptx.writeFile({ fileName: "/Users/peewee/personal/repos/Hotel_Management_Full/presentation/v2_light.pptx" })
  .then(() => console.log("DONE: v2_light.pptx created"))
  .catch(e => console.error("ERROR", e));
