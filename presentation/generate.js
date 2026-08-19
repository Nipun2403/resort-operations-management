const PptxGenJS = require("pptxgenjs");

const C = {
  BG: "FDFBF7", GOLD: "C8A84E", GOLD_DK: "B8963E",
  TEXT: "1A1A1A", TEXT_SEC: "4A4A4A", BORDER: "E8E4DC",
  SURFACE: "FFFFFF", GREEN: "2D7D46", RED: "C0392B",
};
const F = { H: "Georgia", B: "Helvetica", M: "Menlo" };
const SW = 10, SH = 5.625;

function headerSlide(ppt, title) {
  const s = ppt.addSlide();
  s.background = { fill: C.BG };
  s.addText(title, { x: 0.5, y: 0.25, w: 9, h: 0.5, fontSize: 22, fontFace: F.H, color: C.TEXT, bold: true });
  s.addShape(ppt.ShapeType.rect, { x: 0.5, y: 0.72, w: 2.5, h: 0.04, fill: { color: C.GOLD } });
  return s;
}

function addCard(s, x, y, w, h) {
  s.addShape(s._slideLayout ? "rect" : "rect", {
    x, y, w, h, fill: { color: C.SURFACE },
    line: { color: C.BORDER, width: 0.75 },
    rectRadius: 0.08,
  });
}

const pptx = new PptxGenJS();
pptx.defineLayout({ name: "WIDE", width: SW, height: SH });
pptx.layout = "WIDE";
pptx.author = "Aetheris";
pptx.title = "Aetheris — Agentic AI for Luxury Hospitality";
pptx.subject = "Capstone Presentation";

// ── SLIDE 1: TITLE ──
(() => {
  const s = pptx.addSlide();
  s.background = { fill: C.BG };
  s.addShape(pptx.ShapeType.rect, { x: 0, y: 0, w: SW, h: SH, fill: { color: C.GOLD } });
  s.addShape(pptx.ShapeType.rect, { x: 0.04, y: 0.04, w: SW - 0.08, h: SH - 0.08, fill: { color: C.BG } });
  s.addText("Aetheris", {
    x: 0.5, y: 1.2, w: 9, h: 1, fontSize: 48, fontFace: F.H, color: C.TEXT, bold: true, align: "center",
  });
  s.addText("Agentic AI for Luxury Hospitality", {
    x: 0.5, y: 2.2, w: 9, h: 0.6, fontSize: 22, fontFace: F.B, color: C.GOLD_DK, align: "center",
  });
  s.addText("Internship Capstone Presentation", {
    x: 0.5, y: 3.2, w: 9, h: 0.5, fontSize: 16, fontFace: F.B, color: C.TEXT_SEC, align: "center",
  });
  s.addText("[Your Name]  |  July 24, 2026", {
    x: 0.5, y: 4.5, w: 9, h: 0.4, fontSize: 13, fontFace: F.B, color: C.TEXT_SEC, align: "center",
  });
  s.addNotes("Good morning. Presenting Aetheris — a luxury hospitality platform where guest-facing AI executes real operations, staff dashboards update in real-time without polling, and every change on every entity is auto-audited with zero developer effort.");
})();

// ── SLIDE 2: MARKET GAP ──
(() => {
  const s = headerSlide(pptx, "The Market Gap");
  const cols = [
    { title: "Legacy PMS\n(Opera, Cloudbeds)", items: [
      "On-premise / manual deploy", "FAQ bot / no AI",
      "30-60s polling", "Triggers/manual audit",
      "Extension/MIME check", "Payments only idempotency",
      "Template/themeable",
    ], w: 2.8 },
    { title: "Modern SaaS\n(Mews, SkyTouch)", items: [
      "SaaS only", "Basic chat assistant",
      "WebSocket (limited)", "Limited audit",
      "MIME check", "Partial",
      "Themeable",
    ], w: 2.8 },
    { title: "Aetheris", items: [
      "IaC (Bicep), hybrid-ready", "Agentic AI with proposals",
      "SignalR fan-out, sub-second", "ORM-level CDC, zero-touch",
      "Magic-byte validation", "Every mutation",
      "Custom design system",
    ], w: 2.8, highlight: true },
  ];
  let cx = 0.35;
  cols.forEach((col) => {
    const cy = 1.05, ch = 4.2;
    s.addShape(pptx.ShapeType.rect, {
      x: cx, y: cy, w: col.w, h: ch, fill: { color: col.highlight ? "FFF8E8" : C.SURFACE },
      line: { color: col.highlight ? C.GOLD : C.BORDER, width: col.highlight ? 2 : 0.75 },
      rectRadius: 0.08,
    });
    s.addText(col.title, {
      x: cx + 0.15, y: cy + 0.15, w: col.w - 0.3, h: 0.65, fontSize: 13, fontFace: F.B,
      color: col.highlight ? C.GOLD_DK : C.TEXT, bold: true, lineSpacingMultiple: 0.9,
    });
    col.items.forEach((item, i) => {
      s.addText(`${i < 6 ? "▸ " : ""}${item}`, {
        x: cx + 0.15, y: cy + 0.85 + i * 0.45, w: col.w - 0.3, h: 0.42, fontSize: 10.5,
        fontFace: F.B, color: col.highlight && i < 2 ? C.GOLD_DK : C.TEXT_SEC,
        bold: col.highlight && i < 2,
      });
    });
    cx += col.w + 0.28;
  });
  s.addNotes("Three tiers in the market. Legacy PMS requires months to implement. Modern SaaS improves UX but keeps the same architecture. Aetheris rethinks the stack: AI that acts, real-time by default, observability as architecture — not afterthought.");
})();

// ── SLIDE 3: ARCHITECTURE ──
(() => {
  const s = headerSlide(pptx, "Architecture Overview");
  const diagram = [
    ["CLIENT (Angular 22)", "Guest | FrontDesk | Admin | Ops (6)"],
    ["API LAYER (ASP.NET Core 10)", "Controllers | SignalR Hub | Middleware"],
    ["BLL (Domain Services + DTOs)", "19 Services | 5 Workers | AutoMapper"],
    ["REPOSITORY (Generic + Specific)", "13 Repos | Pagination | Dynamic Order"],
    ["DAL (EF Core + PostgreSQL)", "18 Entities | 47 Migrations | Enums"],
  ];
  const layers = [
    { y: 1.05, label: "CLIENT (Angular 22)", sub: "Guest | FrontDesk | Admin | Ops (6)" },
    { y: 1.75, label: "API LAYER (ASP.NET Core 10)", sub: "Controllers | SignalR Hub | Middleware" },
    { y: 2.45, label: "BLL (Domain Services + DTOs)", sub: "19 Services | 5 Workers | AutoMapper" },
    { y: 3.15, label: "REPOSITORY (Generic + Specific)", sub: "13 Repos | Pagination | Dynamic Order" },
    { y: 3.85, label: "DAL (EF Core + PostgreSQL)", sub: "18 Entities | 47 Migrations | Enums" },
  ];
  const lx = 0.5, lw = 4.2, lh = 0.55;
  layers.forEach((l, i) => {
    s.addShape(pptx.ShapeType.rect, {
      x: lx, y: l.y, w: lw, h: lh, fill: { color: i === 0 ? "FFF8E8" : C.SURFACE },
      line: { color: i === 0 ? C.GOLD : C.BORDER, width: i === 0 ? 1.5 : 0.75 },
      rectRadius: 0.06,
    });
    s.addText(l.label, {
      x: lx + 0.12, y: l.y + 0.04, w: lw - 0.24, h: 0.28, fontSize: 9.5, fontFace: F.M,
      color: C.TEXT, bold: true,
    });
    s.addText(l.sub, {
      x: lx + 0.12, y: l.y + 0.28, w: lw - 0.24, h: 0.24, fontSize: 8, fontFace: F.M,
      color: C.TEXT_SEC,
    });
    if (i < layers.length - 1) {
      s.addText("▼", {
        x: lx + lw / 2 - 0.15, y: l.y + lh + 0.02, w: 0.3, h: 0.2, fontSize: 10, color: C.GOLD, align: "center",
      });
    }
  });
  const specs = [
    ["Pattern:", "Clean N-tier + DDD — API → BLL → Repository → DAL"],
    ["Enforcement:", "Project-reference level — zero circular dependencies"],
    ["Validation:", "GitNexus structural analysis (acyclic)"],
    ["Auth:", "JWT (HMAC-SHA256) + BCrypt + SignalR token passthrough"],
    ["Roles:", "6 RBAC roles → 16 protected controllers"],
    ["Workers:", "5 BackgroundService orchestrators"],
    ["Migrations:", "47 EF Core — schema evolution as code"],
    ["Tests:", "29 NUnit+Moq (93% BLL) + xUnit E2E + Vitest"],
    ["Infra:", "100% Bicep IaC — Azure Container Apps, PostgreSQL, Blob/Queue, SignalR"],
    ["Observability:", "Serilog → Log Analytics, OpenTelemetry → Prometheus"],
  ];
  const rx = 5.0, rw = 4.6;
  s.addShape(pptx.ShapeType.rect, {
    x: rx, y: 1.05, w: rw, h: 4.0, fill: { color: C.SURFACE },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
  });
  specs.forEach((sp, i) => {
    s.addText(sp[0], {
      x: rx + 0.15, y: 1.15 + i * 0.37, w: 1.2, h: 0.25, fontSize: 9, fontFace: F.B,
      color: C.GOLD_DK, bold: true,
    });
    s.addText(sp[1], {
      x: rx + 1.3, y: 1.15 + i * 0.37, w: rw - 1.5, h: 0.25, fontSize: 9, fontFace: F.B,
      color: C.TEXT_SEC,
    });
  });
  s.addNotes("Strict unidirectional dependency flow. Each layer only knows the one below it. EF Core lives only in DAL — swap persistence without touching business logic. Domain services in BLL are pure C# — unit testable without database. Infrastructure is fully codified in Bicep — zero console clicks, drift-free deployments.");
})();

// ── SLIDE 4: DEMO FLOW ──
(() => {
  const s = headerSlide(pptx, "Demo Flow Map");
  const desktops = [
    { label: "DESKTOP 1", role: "GUEST", items: ["Public UI", "ATLAS AI", "Audit/PDF"], time: "60s" },
    { label: "DESKTOP 2", role: "ADMIN", items: ["KPIs", "Analytics", "Room Grid"], time: "75s" },
    { label: "DESKTOP 3", role: "FRONT DESK", items: ["Movements", "Search"], time: "35s" },
    { label: "DESKTOP 4", role: "OPS", items: ["SignalR", "Menu Toggle"], time: "35s" },
  ];
  const cx = 0.35, cw = 2.1, ch = 2.6, gap = 0.18;
  desktops.forEach((d, i) => {
    const x = cx + i * (cw + gap + 0.25);
    s.addShape(pptx.ShapeType.rect, {
      x, y: 1.05, w: cw, h: ch, fill: { color: C.SURFACE },
      line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
    });
    s.addText(d.label, {
      x, y: 1.1, w: cw, h: 0.3, fontSize: 9, fontFace: F.M, color: C.GOLD_DK, bold: true, align: "center",
    });
    s.addText(d.role, {
      x, y: 1.35, w: cw, h: 0.25, fontSize: 10, fontFace: F.B, color: C.TEXT, bold: true, align: "center",
    });
    d.items.forEach((item, j) => {
      s.addText(`▸ ${item}`, {
        x: x + 0.15, y: 1.75 + j * 0.35, w: cw - 0.3, h: 0.3, fontSize: 10, fontFace: F.B, color: C.TEXT_SEC,
      });
    });
    s.addText(d.time, {
      x, y: 1.05 + ch + 0.05, w: cw, h: 0.25, fontSize: 10, fontFace: F.B, color: C.GOLD_DK, bold: true, align: "center",
    });
    if (i < desktops.length - 1) {
      s.addText("▶", {
        x: x + cw + 0.02, y: 1.9, w: 0.2, h: 0.3, fontSize: 16, color: C.GOLD, align: "center",
      });
    }
  });
  s.addShape(pptx.ShapeType.rect, {
    x: 0.5, y: 4.2, w: 9.0, h: 0.04, fill: { color: C.GOLD },
  });
  const timeline = "0:00 ──── 0:30 ──── 1:30 ──────── 2:45 ──────── 3:20 ──────── 3:55 ──────── 5:00";
  const tlLabels = "Intro     Guest          Admin           Front         Ops           Close";
  s.addText(timeline, {
    x: 0.5, y: 4.35, w: 9, h: 0.3, fontSize: 8, fontFace: F.M, color: C.TEXT_SEC, align: "center",
  });
  s.addText(tlLabels, {
    x: 0.5, y: 4.6, w: 9, h: 0.2, fontSize: 8, fontFace: F.M, color: C.GOLD_DK, align: "center",
  });

  s.addNotes("Four desktops, pre-logged, each showing a different role's perspective. I'll swipe between them live — you'll see the same data flow across Guest → Admin → Front Desk → Operations in real-time.");
})();

// ── SLIDE 5: ATLAS AGENTIC AI ──
(() => {
  const s = headerSlide(pptx, "Differentiator 1: ATLAS Agentic AI");
  // Left: flow diagram
  const flowLines = [
    'Guest: "burger, pillows, AC broken"',
    "           │",
    "           ▼",
    '   ┌───────┴───────┐',
    "   ▼               ▼               ▼",
    "┌───────────┐ ┌───────────┐ ┌───────────┐",
    "│create_food│ │create_hs-│ │create_mai-│",
    "│_order     │ │keeping_req│ │ntenance   │",
    "│(Side-eff) │ │(Side-eff) │ │(Side-eff) │",
    "└─────┬─────┘ └─────┬─────┘ └─────┬─────┘",
    "      │             │             │",
    "      └──────┬──────┴──────┬──────┘",
    "             ▼",
    "   ┌─────────────────┐",
    "   │ 3 PROPOSALS     │",
    "   │ 5-min TTL each  │",
    "   │ Idempotency keys│",
    "   │ Require CONFIRM │",
    "   └────────┬────────┘",
    "            ▼",
    "   ┌─────────────────┐",
    "   │ BLL Execution   │",
    "   └────────┬────────┘",
    "            ▼",
    "   ┌─────────────────┐",
    "   │ SignalR Fan-out │",
    "   │ <500ms p99      │",
    "   └─────────────────┘",
  ];
  s.addShape(pptx.ShapeType.rect, {
    x: 0.35, y: 1.05, w: 4.5, h: 4.2, fill: { color: C.SURFACE },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
  });
  s.addText(flowLines.join("\n"), {
    x: 0.45, y: 1.12, w: 4.3, h: 4.05, fontSize: 6.8, fontFace: F.M, color: C.TEXT, lineSpacingMultiple: 0.85,
  });

  const caps = [
    ["8 OpenAI Tools", "3 side-effect, 5 read-only"],
    ["Guest-scoped tenancy", "Booking context via ICurrentUserService, IDs never leave trust boundary"],
    ["Proposal/Confirmation", "Human-in-the-loop, 5-min TTL, auto-expiry"],
    ["Prompt injection defense", "Regex sanitizer pre-LLM (ignore previous, system:, assistant:)"],
    ["Idempotency per turn", "X-Idempotency-Key replays cached response on duplicate"],
    ["Full correlation-ID audit", "Every tool call, proposal, confirmation → ConciergeActionLog"],
    ["Self-auditing", "Audit log audits itself"],
    ["Context awareness", "Room, booking, folio hydrated server-side"],
  ];
  s.addShape(pptx.ShapeType.rect, {
    x: 5.1, y: 1.05, w: 4.6, h: 4.2, fill: { color: C.SURFACE },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
  });
  caps.forEach((c, i) => {
    s.addText(c[0], {
      x: 5.25, y: 1.15 + i * 0.5, w: 4.3, h: 0.22, fontSize: 10.5, fontFace: F.B,
      color: C.TEXT, bold: true,
    });
    s.addText(c[1], {
      x: 5.25, y: 1.38 + i * 0.5, w: 4.3, h: 0.22, fontSize: 8.5, fontFace: F.B,
      color: C.TEXT_SEC,
    });
  });
  s.addNotes("ATLAS isn't a chatbot — it's an agentic execution engine. One message fans out to three domain services via proposals the guest must confirm. No auto-execution. Every tool call is audited with correlation IDs. The LLM never sees booking IDs — only action parameters. This is function calling with guardrails, not retrieval-augmented generation.");
})();

// ── SLIDE 6: REAL-TIME OPS ──
(() => {
  const s = headerSlide(pptx, "Differentiator 2: Real-Time Operations");
  // Left panel - architecture
  const archText = [
    "┌─────────┐   ┌──────────┐   ┌─────────┐",
    "│ ATLAS   │──▶│ Azure    │──▶│ Kitchen │",
    "│ Confirm │   │ SignalR  │   │ Group   │",
    "└─────────┘   │ Service  │   └─────────┘",
    "              │(Managed) │",
    "              └────┬─────┘",
    "         ┌────────┼────────┐",
    "         ▼        ▼        ▼",
    "   ┌────────┐ ┌────────┐ ┌────────┐",
    "   │ House  │ │ Maint  │ │ Admin  │",
    "   │ keep   │ │ enance │ │ Dash   │",
    "   └────────┘ └────────┘ └────────┘",
    "",
    "• No polling • Auto-reconnect",
    "• Exponential backoff",
    "• Sub-second eventual consistency",
    "• 6 role-based groups",
  ];
  s.addShape(pptx.ShapeType.rect, {
    x: 0.35, y: 1.05, w: 4.3, h: 3.0, fill: { color: C.SURFACE },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
  });
  s.addText(archText.join("\n"), {
    x: 0.45, y: 1.12, w: 4.1, h: 2.85, fontSize: 7.5, fontFace: F.M, color: C.TEXT, lineSpacingMultiple: 0.9,
  });

  // Right panel - proof table
  const rows = [
    [
      { text: "Action", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
      { text: "Kitchen", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
      { text: "Housekeeping", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
      { text: "Maintenance", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
    ],
    [
      { text: 'ATLAS: "burger"', options: { fontSize: 8.5, fontFace: F.B } },
      { text: "✅ Order #1047", options: { fontSize: 8.5, fontFace: F.B, color: C.GREEN } },
      { text: "—", options: { fontSize: 8.5, fontFace: F.B } },
      { text: "—", options: { fontSize: 8.5, fontFace: F.B } },
    ],
    [
      { text: 'ATLAS: "pillows"', options: { fontSize: 8.5, fontFace: F.B } },
      { text: "—", options: { fontSize: 8.5, fontFace: F.B } },
      { text: "✅ Task appears", options: { fontSize: 8.5, fontFace: F.B, color: C.GREEN } },
      { text: "—", options: { fontSize: 8.5, fontFace: F.B } },
    ],
    [
      { text: 'ATLAS: "AC broken"', options: { fontSize: 8.5, fontFace: F.B } },
      { text: "—", options: { fontSize: 8.5, fontFace: F.B } },
      { text: "—", options: { fontSize: 8.5, fontFace: F.B } },
      { text: "✅ Ticket appears", options: { fontSize: 8.5, fontFace: F.B, color: C.GREEN } },
    ],
    [
      { text: 'Kitchen taps\n"Preparing"', options: { fontSize: 8.5, fontFace: F.B } },
      { text: "✅ Optimistic UI", options: { fontSize: 8.5, fontFace: F.B, color: C.GREEN } },
      { text: "✅ Synced", options: { fontSize: 8.5, fontFace: F.B, color: C.GREEN } },
      { text: "✅ Synced", options: { fontSize: 8.5, fontFace: F.B, color: C.GREEN } },
    ],
  ];
  s.addTable(rows, {
    x: 0.35, y: 4.2, w: 4.3, colW: [1.1, 1.1, 1.1, 1.0],
    border: { type: "solid", pt: 0.5, color: C.BORDER },
    rowH: [0.3, 0.3, 0.3, 0.3, 0.4],
  });
  // Key metric
  s.addText('<500ms p99 end-to-end', {
    x: 5.0, y: 3.3, w: 4.6, h: 0.35, fontSize: 18, fontFace: F.H, color: C.GOLD_DK, bold: true, align: "center",
  });
  s.addText('(confirm → broadcast → render)', {
    x: 5.0, y: 3.65, w: 4.6, h: 0.25, fontSize: 11, fontFace: F.B, color: C.TEXT_SEC, align: "center",
  });

  s.addNotes("Real-time isn't a feature — it's the default. Azure SignalR Service handles connection management, auto-scaling, reconnection. Six role-based groups. When ATLAS confirms, three departments receive alerts in under 500ms. No polling, no staleness. The kitchen tapping 'Preparing' broadcasts optimistically — instant feedback, server confirmation, then sync.");
})();

// ── SLIDE 7: ZERO-TOUCH OBSERVABILITY ──
(() => {
  const s = headerSlide(pptx, "Differentiator 3: Zero-Touch Observability");
  const cards = [
    { title: "ORM-Level Audit (CDC)", body: [
      "SaveChangesAsync Override",
      "Added → NewValues (JSONB)",
      "Modified → Old + New (JSONB)",
      "Deleted → OldValues (JSONB)",
      "",
      "Enrichment: Actor + Timestamp + PK",
      "Storage: PostgreSQL JSONB",
      "Coverage: 18 entity types",
      "Self-auditing • Zero attributes",
    ]},
    { title: "End-to-End Idempotency", body: [
      "X-Idempotency-Key (UUID)",
      "on EVERY mutation",
      "",
      "Key exists? → Replay cached",
      "New? → Execute → Store",
      "Cleanup: 48h TTL",
      "",
      "Race: PK conflict → cached",
      "POST/PUT/PATCH globally",
    ]},
    { title: "Magic-Byte Image Validation", body: [
      "Upload → SAS URL (15-min)",
      "Extension whitelist",
      "Size limit (10MB)",
      "Queue → ValidationWorker",
      "Read 512B from blob",
      "Validate magic bytes:",
      " JPEG: FF D8 FF",
      " PNG: 89 50 4E ...",
      " WebP: 52 49 46 ...",
      "Reject → Delete + Log",
    ]},
  ];
  const cw = 2.85, gap = 0.2, ch = 4.0;
  cards.forEach((card, i) => {
    const cx = 0.4 + i * (cw + gap);
    s.addShape(pptx.ShapeType.rect, {
      x: cx, y: 1.05, w: cw, h: ch, fill: { color: C.SURFACE },
      line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
    });
    s.addText(card.title, {
      x: cx + 0.12, y: 1.1, w: cw - 0.24, h: 0.3, fontSize: 12, fontFace: F.B,
      color: C.GOLD_DK, bold: true,
    });
    s.addShape(pptx.ShapeType.rect, {
      x: cx + 0.12, y: 1.42, w: 1.2, h: 0.03, fill: { color: C.GOLD },
    });
    s.addText(card.body.join("\n"), {
      x: cx + 0.12, y: 1.55, w: cw - 0.24, h: ch - 0.55, fontSize: 8.5, fontFace: F.M,
      color: C.TEXT_SEC, lineSpacingMultiple: 0.9,
    });
  });
  s.addNotes("Three pillars of observability you didn't see. Audit: every change on every entity captured at the ORM layer — JSONB diff, impossible to bypass. Idempotency: every mutation carries a key — duplicate retry replays exact response, zero double-charges. Image security: we read magic bytes from the blob itself, not extensions. Renamed .exe → .jpg gets caught and deleted. Three background workers keep storage clean.");
})();

// ── SLIDE 8: TECH DEPTH ──
(() => {
  const s = headerSlide(pptx, "Tech Depth (What You Didn't See)");
  const rows = [
    [
      { text: "Platform Hardening", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
      { text: "Security & Auth", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
      { text: "Data & Quality", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
      { text: "Infra & Ops", options: { bold: true, fontSize: 9, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
    ],
    ["47 EF Core migrations", "BCrypt + JWT HMAC-SHA256", "29 NUnit+Moq (93% BLL)", "100% Bicep IaC"].map(t => ({ text: t, options: { fontSize: 8, fontFace: F.B } })),
    ["Schema evolution as code", "SignalR token passthrough", "xUnit WebApplicationFactory E2E", "Azure Container Apps"].map(t => ({ text: t, options: { fontSize: 8, fontFace: F.B } })),
    ["Generic Repository pattern", "6 RBAC roles → 16 controllers", "Vitest frontend tests", "Managed PostgreSQL"].map(t => ({ text: t, options: { fontSize: 8, fontFace: F.B } })),
    ["Dynamic ordering (expr trees)", "Multi-tier rate limiting", "GitNexus acyclic validation", "Blob + Queue Storage"].map(t => ({ text: t, options: { fontSize: 8, fontFace: F.B } })),
    ["Pagination (standardized)", "Global fixed-window (100/10s)", "Serilog → Log Analytics", "SignalR Service"].map(t => ({ text: t, options: { fontSize: 8, fontFace: F.B } })),
    ["Soft deletes (IsActive)", "Image upload token bucket", "OpenTelemetry → Prometheus", "Private endpoints"].map(t => ({ text: t, options: { fontSize: 8, fontFace: F.B } })),
    ["Concurrency tokens (RowVersion)", "ATLAS concierge token bucket", "Health checks endpoint", "Managed identities"].map(t => ({ text: t, options: { fontSize: 8, fontFace: F.B } })),
    ["JSONB audit storage", "Prompt injection sanitizer", "Zero circular dependencies", "Zero-drift deployments"].map(t => ({ text: t, options: { fontSize: 8, fontFace: F.B } })),
  ];
  s.addTable(rows, {
    x: 0.35, y: 1.0, w: 9.3, colW: [2.35, 2.35, 2.3, 2.3],
    border: { type: "solid", pt: 0.5, color: C.BORDER },
    margin: [2, 4, 2, 4],
    rowH: [0.3, 0.35, 0.35, 0.35, 0.35, 0.35, 0.35, 0.35, 0.35],
  });
  s.addNotes("The iceberg under the waterline. 47 migrations — schema as code. 93% BLL coverage with pure unit tests — no database. GitNexus validates architecture acyclicity on every build. Bicep codifies all Azure resources — private endpoints, managed identities, zero console clicks. Rate limiting at three tiers. OpenTelemetry metrics exported to Prometheus. This is production-grade, not a prototype.");
})();

// ── SLIDE 9: FRONT DESK ──
(() => {
  const s = headerSlide(pptx, "Front Desk & Operational Intelligence");
  // Left panel
  s.addShape(pptx.ShapeType.rect, {
    x: 0.35, y: 1.05, w: 4.3, h: 3.4, fill: { color: C.SURFACE },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
  });
  s.addText("Today's Movements", {
    x: 0.5, y: 1.12, w: 4, h: 0.3, fontSize: 13, fontFace: F.B, color: C.TEXT, bold: true,
  });
  const mvRows = [
    [
      { text: "Room", options: { bold: true, fontSize: 9, fontFace: F.B, fill: { color: C.BORDER } } },
      { text: "Guest", options: { bold: true, fontSize: 9, fontFace: F.B, fill: { color: C.BORDER } } },
      { text: "Status", options: { bold: true, fontSize: 9, fontFace: F.B, fill: { color: C.BORDER } } },
      { text: "Requests", options: { bold: true, fontSize: 9, fontFace: F.B, fill: { color: C.BORDER } } },
    ],
    ["301", "Smith", "Arrive", "Late CK"].map(t => ({ text: t, options: { fontSize: 8.5, fontFace: F.M } })),
    ["304", "Jones", "Depart", "—"].map(t => ({ text: t, options: { fontSize: 8.5, fontFace: F.M } })),
    ["307", "Chen", "Arrive", "Pillows"].map(t => ({ text: t, options: { fontSize: 8.5, fontFace: F.M } })),
  ];
  s.addTable(mvRows, {
    x: 0.5, y: 1.5, w: 4.0, colW: [0.8, 1.0, 1.0, 1.2],
    border: { type: "solid", pt: 0.5, color: C.BORDER },
    rowH: [0.28, 0.28, 0.28, 0.28],
  });
  s.addText("Pre-shift situational awareness • Zero surprise rushes", {
    x: 0.5, y: 2.7, w: 4, h: 0.3, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Right panel
  s.addShape(pptx.ShapeType.rect, {
    x: 5.0, y: 1.05, w: 4.65, h: 3.4, fill: { color: C.SURFACE },
    line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
  });
  s.addText("Global Guest Search", {
    x: 5.15, y: 1.12, w: 4.3, h: 0.3, fontSize: 13, fontFace: F.B, color: C.TEXT, bold: true,
  });
  s.addText('Search: "john" → Instant results across:\n  • Email • Name • Phone • Booking ID', {
    x: 5.15, y: 1.5, w: 4.3, h: 0.6, fontSize: 10, fontFace: F.B, color: C.TEXT_SEC,
  });
  s.addText("Click → Context-preserving deep link:\nCheck-in | Extend | Cancel | Folio | History", {
    x: 5.15, y: 2.2, w: 4.3, h: 0.6, fontSize: 10, fontFace: F.B, color: C.GOLD_DK,
  });
  s.addText("Zero context switching • Single-interaction resolution", {
    x: 5.15, y: 3.0, w: 4.3, h: 0.3, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
  });

  // Bottom callout
  s.addText("Legacy PMS: 5 clicks, 3 screens, printed reports. Aetheris: 1 search, 1 click, real-time intelligence.", {
    x: 0.5, y: 4.7, w: 9, h: 0.35, fontSize: 11, fontFace: F.H, color: C.GOLD_DK,
    italic: true, align: "center",
  });

  s.addNotes("Front desk starts their shift with Today's Movements — arrivals, departures, special requests. No printed reports. When the phone rings, global search resolves the guest instantly across email, name, phone, booking ID. One click preserves context to check-in, extend, cancel, or view folio. Built for the overwhelmed front desk.");
})();

// ── SLIDE 10: CLOSE ──
(() => {
  const s = headerSlide(pptx, "Three Strategic Differentiators");
  const cards = [
    { num: "1", title: "EXECUTION > SUGGESTION", body: "ATLAS commits transactions — not just conversations.\nReal orders. Real tickets. Real revenue." },
    { num: "2", title: "REAL-TIME BY DEFAULT", body: "SignalR fan-out everywhere. No polling. No staleness.\nSub-second eventual consistency across 6 roles." },
    { num: "3", title: "ZERO-TOUCH OBSERVABILITY", body: "Every change audited. Every upload validated.\nEvery retry idempotent. Compliance without developer tax." },
  ];
  cards.forEach((card, i) => {
    const cx = 0.35 + i * 3.15;
    s.addShape(pptx.ShapeType.rect, {
      x: cx, y: 1.05, w: 2.95, h: 3.0, fill: { color: C.SURFACE },
      line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
    });
    s.addShape(pptx.ShapeType.ellipse, {
      x: cx + 0.15, y: 1.2, w: 0.5, h: 0.5, fill: { color: C.GOLD },
    });
    s.addText(card.num, {
      x: cx + 0.15, y: 1.2, w: 0.5, h: 0.5, fontSize: 18, fontFace: F.B,
      color: "FFFFFF", bold: true, align: "center", valign: "middle",
    });
    s.addText(card.title, {
      x: cx + 0.15, y: 1.85, w: 2.65, h: 0.35, fontSize: 13, fontFace: F.B,
      color: C.GOLD_DK, bold: true,
    });
    s.addText(card.body, {
      x: cx + 0.15, y: 2.25, w: 2.65, h: 1.2, fontSize: 10.5, fontFace: F.B, color: C.TEXT_SEC,
      lineSpacingMultiple: 1.1,
    });
  });

  // Bottom section
  s.addShape(pptx.ShapeType.rect, {
    x: 0.5, y: 4.3, w: 9, h: 1.0, fill: { color: "FFF8E8" },
    line: { color: C.GOLD, width: 0.75 }, rectRadius: 0.06,
  });
  s.addText("Deployed: hotel-web-demo1.ambitiousmushroom-274454dc.centralindia.azurecontainerapps.io", {
    x: 0.7, y: 4.35, w: 8.6, h: 0.3, fontSize: 9.5, fontFace: F.M, color: C.TEXT,
  });
  s.addText("Credentials provided. Open for technical deep-dive in Q&A.", {
    x: 0.7, y: 4.7, w: 8.6, h: 0.3, fontSize: 10, fontFace: F.B, color: C.TEXT_SEC,
  });
  s.addNotes("Aetheris proves luxury hospitality doesn't need legacy PMS compromise. Agentic AI that executes. Staff dashboards with true real-time consistency. Auditability as a cross-cutting architectural concern. Infrastructure as code, security by default, observability built-in. This is production-grade code — deployed on Azure, load-tested, pen-tested patterns. Architecture scales from boutique to enterprise brand. Three things to remember: Execution over suggestion. Real-time by default. Zero-touch observability. Thank you.");
})();

// ── SLIDE 11: MARKET COMPARISON MATRIX (HIDDEN) ──
(() => {
  const s = headerSlide(pptx, "Appendix: Market Comparison Matrix");
  s.hidden = true;
  const rows = [
    [
      { text: "Capability", options: { bold: true, fontSize: 8, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
      { text: "Legacy PMS", options: { bold: true, fontSize: 8, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
      { text: "Modern SaaS", options: { bold: true, fontSize: 8, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
      { text: "Aetheris", options: { bold: true, fontSize: 8, fontFace: F.B, color: "FFFFFF", fill: { color: C.GOLD } } },
    ],
    ["AI Concierge", "FAQ bot / none", "Basic chat", "Agentic execution with proposals"].map(t => ({ text: t, options: { fontSize: 7.5, fontFace: F.B } })),
    ["Real-Time Ops", "Polling (30-60s)", "WebSocket (some)", "SignalR fan-out, sub-second"].map(t => ({ text: t, options: { fontSize: 7.5, fontFace: F.B } })),
    ["Audit Trail", "Triggers / manual", "Limited", "ORM-level CDC, JSONB"].map(t => ({ text: t, options: { fontSize: 7.5, fontFace: F.B } })),
    ["Image Security", "Extension check", "MIME check", "Magic-byte validation"].map(t => ({ text: t, options: { fontSize: 7.5, fontFace: F.B } })),
    ["Idempotency", "Payments only", "Partial", "Every mutation globally"].map(t => ({ text: t, options: { fontSize: 7.5, fontFace: F.B } })),
    ["Deployment", "On-prem / manual", "SaaS only", "IaC (Bicep), hybrid-ready"].map(t => ({ text: t, options: { fontSize: 7.5, fontFace: F.B } })),
    ["Design System", "Template", "Themeable", "Custom tokens, glass-morphism"].map(t => ({ text: t, options: { fontSize: 7.5, fontFace: F.B } })),
    ["Architecture", "Monolithic", "Modular", "Clean N-tier + DDD, validated"].map(t => ({ text: t, options: { fontSize: 7.5, fontFace: F.B } })),
    ["Testing", "Manual / limited", "Unit tests", "93% BLL + E2E + Vitest"].map(t => ({ text: t, options: { fontSize: 7.5, fontFace: F.B } })),
    ["Observability", "Logs only", "Basic metrics", "OpenTelemetry + Prometheus"].map(t => ({ text: t, options: { fontSize: 7.5, fontFace: F.B } })),
  ];
  s.addTable(rows, {
    x: 0.3, y: 1.0, w: 9.4, colW: [1.6, 2.3, 2.3, 3.2],
    border: { type: "solid", pt: 0.5, color: C.BORDER },
    rowH: 0.35,
    margin: [2, 3, 2, 3],
  });
  s.addNotes("Reference slide for Q&A. Not shown unless asked.");
})();

// ── SLIDE 12: Q&A HOOKS (HIDDEN) ──
(() => {
  const s = headerSlide(pptx, "Appendix: Q&A Hooks by Audience");
  s.hidden = true;
  const grid = [
    { role: "Sales", text: "ATLAS turns 'I want...' into committed revenue in 3 clicks — reduces booking abandonment." },
    { role: "HR", text: "Audit trail = compliance-ready, zero dev tax. Staff dashboards = reduced cognitive load, lower burnout." },
    { role: "Architect", text: "Clean N-tier. Domain services reusable. EF Core only in DAL. Swappable persistence. GitNexus-validated acyclic." },
    { role: "Cloud Engineer", text: "Azure-native: Container Apps, Managed PostgreSQL, Blob+Queue, SignalR Service. Bicep IaC, managed identities, private endpoints." },
    { role: "AI Director", text: "Function calling with proposal/confirmation. Guest-scoped tenancy. Pre-LLM sanitization. Full correlation-ID audit." },
    { role: "Delivery", text: "Generic CRUD component powers 8 management pages. New entity = config, not code. Design system = consistent velocity." },
  ];
  grid.forEach((item, i) => {
    const col = i % 3;
    const row = Math.floor(i / 3);
    const cx = 0.35 + col * 3.15;
    const cy = 1.05 + row * 1.85;
    s.addShape(pptx.ShapeType.rect, {
      x: cx, y: cy, w: 2.95, h: 1.65, fill: { color: C.SURFACE },
      line: { color: C.BORDER, width: 0.75 }, rectRadius: 0.08,
    });
    s.addText(item.role, {
      x: cx + 0.12, y: cy + 0.08, w: 2.7, h: 0.28, fontSize: 11, fontFace: F.B,
      color: C.GOLD_DK, bold: true,
    });
    s.addShape(pptx.ShapeType.rect, {
      x: cx + 0.12, y: cy + 0.38, w: 1, h: 0.03, fill: { color: C.GOLD },
    });
    s.addText(item.text, {
      x: cx + 0.12, y: cy + 0.5, w: 2.7, h: 1.0, fontSize: 9, fontFace: F.B, color: C.TEXT_SEC,
      lineSpacingMultiple: 1.0,
    });
  });
  s.addNotes("Reference for Q&A. Memorize 1-2 per role.");
})();

pptx.writeFile({ fileName: "/Users/peewee/personal/repos/Hotel_Management_Full/presentation/Aetheris-Capstone.pptx" })
  .then(() => console.log("DONE: Aetheris-Capstone.pptx created"))
  .catch(e => console.error("ERROR", e));
