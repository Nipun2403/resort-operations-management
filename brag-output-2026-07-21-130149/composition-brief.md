# Hyperframes Composition Brief: AETHERIS

## Objective
Create a short launch-style brag video for AETHERIS — an ultra-luxury private estate management platform.

## Output
- Composition directory: `brag-output-2026-07-21-130149/composition/`
- Rendered video: `brag-output-2026-07-21-130149/brag.mp4`
- Format: landscape — 1920x1080
- Duration: 20 seconds

## Source Material
- Project root: `/Users/peewee/personal/repos/Hotel_Management_Full/`
- Frontend: Angular 22 standalone-component SPA with dark/gold theme
- Backend: .NET 10 ASP.NET Core API with PostgreSQL, SignalR, JWT auth
- Product name: AETHERIS
- Tagline / strongest claim: "The Silent Peak of Luxury" / "Private Estate & Refuge"
- Key UI moments to recreate: glassmorphism booking bar, The Private Corridor guest portal, Atlas AI concierge, admin/front desk/kitchen/housekeeping/maintenance operational dashboards
- Copy that must appear verbatim:
  - "Behind every silent peak…is a command center."
  - "The Silent Peak of Luxury"
  - "PRIVATE ESTATE & REFUGE"
  - "THE PRIVATE CORRIDOR"
  - "AETHERIS"
  - "Private Estate & Refuge. Command Center Included."

## Creative Direction
- Tone preset: cinematic with mission-control energy
- Creative direction: "Behind the velvet rope — luxury surface meets operational depth"
- Interpretation: Guest-facing scenes (1-3) are slow, polished, elegant with crossfades and Playfair Display serif. Scene 4 (Mission Control) shifts to faster cuts, harder transitions, more energetic pacing. Outro returns to elegance.
- Angle: Behind every serene luxury experience is an invisible command center. The video opens with the polished public face, then reveals the operational engine: six mission-control portals orchestrating the stay.
- Hook: Full-screen dark. "Behind every silent peak…" fades in gold serif. Pause. "…is a command center."
- Outro: AETHERIS wordmark in gold. "Private Estate & Refuge. Command Center Included."
- Avoid:
  - Generic SaaS language ("streamline your workflow")
  - Abstract filler visuals
  - Making the operations portals feel chaotic — they should feel controlled and premium

## Visual Identity
- Background: #131411
- Text: #e4e2dd
- Accent: #e4c285 (gold)
- Surface container: #1f201d
- Glass background: rgba(26, 26, 26, 0.7)
- Glass border: rgba(228, 194, 133, 0.2)
- Display font: 'Playfair Display', serif
- Body font: 'Manrope', sans-serif
- Visual references: gold-on-dark serif typography, glassmorphism cards with golden borders, material symbols outlined icons, dark surface containers with subtle outlines

## Storyboard
Use the storyboard in `brag-output-2026-07-21-130149/brag-plan.md` as the creative contract.

Scene summary:
1. Hook — 0-3s — "Behind every silent peak…" / "…is a command center." — gold serif on black
2. The Face — 3-7s — Hero headline + booking bar (ARRIVAL/DEPARTURE/GUESTS cards + CTA)
3. The Guest Journey — 7-11s — "THE PRIVATE CORRIDOR" with booking card, menu grid, Atlas chat
4. Mission Control — 11-17s — 5 portal cards: ADMIN, FRONT DESK, KITCHEN, HOUSEKEEPING, MAINTENANCE
5. Outro — 17-20s — AETHERIS wordmark + tagline, fade to black

## Audio
- Audio role: warm cinematic bed with energy ramp into mission-control sequence
- Audio arc: soft entrance → warm establishment → building energy → controlled pulse → resolved finale
- Music: `happy-beats-business-moves-vol-12-by-ende-dot-app.mp3`
- Music treatment: fade in at 0s at volume 0.15, rise to 0.30 by end of scene 1. Hold at ~0.30 through scenes 2-4. Gentle fade-out starting at 17.5s over final 2.5s.
- Music cue guidance: preset at assets/music/cues/happy-beats-business-moves-vol-12-by-ende-dot-app.music-cues.md (110 BPM, 117.36s). Strong cue targets: 8.74s (scene 3 transition), 13.11s (mission control entrance), 17.47s (outro). Beat grid spacing ~0.54s — snap sequential portal cards to every other beat for readability.
- Audio-reactive treatment: subtle; use music RMS/bass to make hero glow and card presence breathe. No waveform/equalizer visuals.
- Audio-coupled moments:
  - Scene 2 — booking cards slide in, each with card-slide SFX
  - Scene 3 — three guest portal cards appear with card-slide sounds; last card gets success accent
  - Scene 4 — each portal card arrives with interface/switch or click SFX; final card gets stronger impact
  - Scene 5 — deep resonant bell (impactBell_heavy_000) on logo landing; music fades out
- SFX selection guidance: prefer low/medium HF-risk sounds. Use casino/card-slide-* for card reveals, interface/click_* or interface/switch_* for portal card entries, impact/impactBell_heavy_000 for logo payoff. All SFX at 0.65-0.80 volume.
- SFX analysis guidance: `sfx-analysis.md` in the brag skill assets. Prefer low high-frequency-risk files.
- Exact SFX choice: Hyperframes should choose filenames, timestamps, density, and volume based on the implemented animation.
- Audio files: copy the chosen music and any Hyperframes-selected SFX into `brag-output-2026-07-21-130149/composition/assets/`

## Hyperframes Instructions
Build the composition in `brag-output-2026-07-21-130149/composition/`.

Requirements:
- Show real brand copy from AETHERIS verbatim (headlines, labels, portal names)
- Keep all text readable in the final render — follow the reading-time floor from the plan
- Keep the video within 15-25 seconds (target 20s)
- Include music + SFX layer
- Major reveals may move toward nearby strong cues within ~0.15s
- Sequential portal cards in scene 4: snap to every other beat (~1.08s spacing at 110 BPM) for readability — each card label must hold long enough to read
- Use SFX to support motion and interaction
- Honor planned music treatment (fade-in, fade-out, beat-aligned reveals)
- Consider audio-reactive workflow for subtle glow/breathe on hero text and cards
- Run `hyperframes check` before render — it is brag's single pre-render gate
