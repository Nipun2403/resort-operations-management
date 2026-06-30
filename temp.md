To get the best results from Google’s Stitch (or any AI UI generation tool like v0, Midjourney for UI, etc.), you must avoid giving it generic terms like "hotel website." It will default to standard Material Design with blues and whites.

Stitch relies heavily on **layout structure, explicit style keywords, and negative prompts** (what _not_ to do).

Here is the exact set of prompts you should copy and paste into Stitch, broken down by page. I recommend generating them one by one to maintain consistency.

---

### Prompt 1: The Global Design System (Do this first to establish the look)

_Copy this into Stitch to set the baseline aesthetic before generating specific pages. If Stitch allows a "Theme" or "Style" input, put this there._

> **Prompt:** "Establish a global design system for an ultra-luxury, top 0.0001% exclusive resort. The aesthetic is 'Quiet Wealth'—mysterious, opulent, and highly editorial.
> **Colors:** Deep obsidian black (#0A0A0A) and rich charcoal backgrounds. Text in soft alabaster (#F2F0EB). Accents exclusively in muted champagne gold (#C9A96E). No bright colors, no standard blues.
> **Typography:** High-contrast, elegant Serif for all headings (like Playfair Display or Cormorant). Clean, spaced-out geometric Sans-Serif for body and UI elements (like Manrope or Inter).
> **Components:** No standard Material Design shadows, no rounded corners, no borders. Use ultra-subtle dark glassmorphism (backdrop-filter: blur on dark rgba). Buttons should be minimalist with gold underline or thin gold border that fills on hover.
> **Vibe:** Cinematic, breathing room, vast whitespace, high fashion magazine layout."

---

### Prompt 2: The Home Page (Hero & Availability)

_This focuses on the dramatic entrance and the sleek, integrated search bar._

> **Prompt:** "Design a luxury resort landing home page.
> **Hero Section:** Full viewport height (100vh) cinematic dark background. Massive, staggered Serif typography overlapping the background, reading 'Experience Absolute Seclusion.' The text should have wide letter-spacing and feel monumental.
> **Search Bar:** Instead of a standard card, create a sleek, floating dark glassmorphism panel anchored to the bottom third of the hero. It should be a single horizontal row containing: 'Check-in' (text only, gold underline), 'Check-out' (text only, gold underline), 'Guests' (text only, gold underline), and a minimalist 'Discover' button with a gold border. No labels inside the inputs, just placeholder text in alabaster.
> **Featured Rooms:** Below the hero, an asymmetric, editorial layout. One large room card taking up 60% width on the left, two smaller cards stacked taking up 40% on the right. Cards have no borders, no shadows, just a high-res image that subtly scales up on hover, with the room name in Serif and price in small Sans-Serif aligned to the right.
> Do not use standard Material Design cards or floating action buttons."

---

### Prompt 3: Room Catalogue (The Irregular Grid & Hover FX)

_This is where we enforce the "mind-bending" irregular grid._

> **Prompt:** "Design a Room Catalogue page for an ultra-luxury exclusive resort.
> **Layout:** Create an asymmetric, irregular masonry grid. Do NOT use a standard 3-column equal-width grid. Alternate row layouts: Row 1 has a 60% wide card on the left and 40% on the right. Row 2 has 40% on the left and 60% on the right.
> **Cards:** Each card is a dark container with a high-res image taking up 80% of the space, and text at the bottom. Text includes a large Serif room name, and a tiny, understated Sans-Serif price aligned to the far right. No buttons on the cards.
> **Hover Effect Description:** On hover, the card should have a CSS 3D transform effect (perspective and slight rotation following the cursor), and the image should zoom in slightly with a slow ease-out transition. The background of the card should subtly lighten at the edges.
> Background is deep obsidian (#0A0A0A). Text is alabaster (#F2F0EB). No standard borders or Material UI elements."

---

### Prompt 4: Room Detail Page (Cinematic Gallery)

_This transforms the standard horizontal scroll into an immersive experience._

> **Prompt:** "Design a Room Detail page for an ultra-luxury resort.
> **Gallery:** A full-bleed, edge-to-edge horizontal scrolling gallery taking up the top 70% of the viewport height. No scrollbars visible. Images are separated by thin gold vertical lines (#C9A96E) instead of gaps.
> **Details Section:** Overlapping the bottom of the gallery by about 50px, a dark glassmorphism panel containing the room details.
> **Typography:** The Room Name is a massive, high-contrast Serif heading. Below it, a two-column asymmetric grid for details: Max Occupancy, Square Footage, and Bed Configuration. The labels are in gold Sans-Serif uppercase, the values are in large Alabaster Serif.
> **CTA:** A single, elegant 'Reserve This Sanctuary' button at the bottom right of the panel, styled with a thin gold border, no fill, that fills with gold on hover (text turns black).
> No standard Material icons, use thin line-art icons instead."

---

### Prompt 5: Menu & Amenities (Interactive Discovery)

_This ditches the standard grid for an editorial, interactive feel._

> **Prompt:** "Design a Restaurant Menu and Amenities page for a top 0.0001% wealth resort.
> **Layout:** High-fashion editorial style. Wide margins.
> **Menu:** Instead of cards, use a horizontal accordion layout. Categories (e.g., 'The Starters', 'The Mains') are full-width rows with a thin gold bottom border. When a category is active, it expands to reveal a list of items inside. Item name is on the far left in Serif, a thin dotted line stretches across the middle, and the price is on the far right in Sans-Serif. No food icons.
> **Amenities:** A bento-box style asymmetric grid. Large dark panels with subtle, darkened background images. On hover, a glassmorphism overlay slides up from the bottom with the amenity name in massive Serif and a one-line description in Sans-Serif.
> Strictly use the obsidian (#0A0A0A), alabaster (#F2F0EB), and champagne gold (#C9A96E) palette. No standard Material Design cards, no shadows, no bright colors."

---

### Prompt 6: The Shell (Nav & Footer)

_This cleans up the header/footer to match the elite vibe._

> **Prompt:** "Design the persistent Shell for an ultra-luxury resort website (Navigation and Footer).
> **Navigation:** A completely transparent header that overlays the hero. Logo on the left in elegant Serif. Navigation links on the right in spaced-out, uppercase Sans-Serif (Home, Rooms, Menu, Amenities). No backgrounds, no borders. On hover, links get a subtle gold underline expanding from the center. A 'Reserve' text link on the far right in champagne gold (#C9A96E). On scroll (sticky state), the nav background becomes dark glassmorphism (backdrop-filter: blur 20px, rgba(10,10,10,0.8)).
> **Footer:** Vast whitespace. A massive Serif hotel name centered, acting as a signature. Below it, very small, spaced-out address and copyright info in muted grey Sans-Serif. No columns of links, no social media icons cluttering the space. Minimalist and authoritative."

---

### 💡 Pro-Tips for using Stitch with these prompts:

1. **Iterate on Hover States:** AI UI tools struggle with complex CSS hover states in a single generation. If Stitch generates the layout you love but makes the hover basic, reply to it: _"Keep the layout exactly the same, but change the hover state of the cards to use CSS `transform: perspective(1000px) rotateY(5deg)` and scale the inner image."_
2. **"No Material Design":** AI defaults to Material because it's safe. Constantly reinforcing "No Material Design cards, no shadows, no borders" forces it to look for CSS alternatives like glassmorphism or minimal underlines.
3. **Image References:** If Stitch allows image uploads, upload a screenshot of an Aman Resort website or a high-fashion Vogue layout alongside the text prompt. It will dramatically improve the output.

To build this ultra-luxurious, mysterious, and highly interactive experience in Stitch AI, we must move away from standard Material Design defaults and instruct the AI to use bespoke aesthetics. We will use a "Master Design System" prompt to establish the global feel, followed by specific prompts for every page, modal, and component.

### How to use these prompts in Stitch AI:

1. Start a new project in Stitch AI.
2. Paste the **Master Design System & Shell** prompt first to establish the global theme, navigation, and footer.
3. Create new pages/screens within Stitch for each subsequent prompt. Paste the individual prompts into their respective screens. Ensure Stitch AI knows these are sub-views of the main shell.

---

### 1. Master Design System & Public Shell (Global Layout)

**Prompt for Stitch AI:**
"Create a global layout shell for an ultra-luxury hotel resort catering to the top 0.0001% wealth cap. The aesthetic must be opulent, mysterious, and modern, avoiding any standard hotel website clichés.

**Global Aesthetic & Typography:**

- Background: Deep obsidian black (#070707) or rich charcoal.
- Accents: Brushed gold (#D4AF37), champagne (#F7E7CE), and deep emerald (#022B2B) for subtle contrasts.
- Typography: Use a massive, bold Serif font (like Playfair Display or Cormorant Garamond) for headings to scream wealth. Use a clean, minimalist Sans-Serif (like Inter or Montserrat) for body text, spaced out with wide letter-spacing.
- Cursor Effect: Implement a custom mouse pointer. A small, sleek gold ring that follows the native cursor with a slight delay (lerping effect). On hover over clickable elements, the ring expands and fills slightly.

**Header (Sticky):**

- Invisible by default, appearing as a sleek, transparent floating bar with a subtle `backdrop-filter: blur(12px)` and a hairline gold border at the bottom.
- Left: A minimalist monogram logo or refined text in gold serif.
- Center/Right: Navigation links (Home, Rooms, Menu, Amenities). On hover, links don't change color; instead, a thin gold underline draws itself from left to right in 0.3s.
- CTA Buttons: 'Check Availability' is a solid brushed-gold pill button with a smooth magnetic hover effect (button pulls slightly toward the cursor). 'Login' is a minimalist text link with a ghost outline on hover.
- Mobile: The hamburger menu opens a full-screen obsidian overlay. The menu items appear staggered from the bottom up in massive serif typography.

**Footer:**

- Minimalist, vast negative space. Small, muted gold typography for copyright and address. A subtle animated golden gradient line at the very top of the footer.

**Main Content Area:**

- Render a `<router-outlet>` placeholder here with a smooth page transition effect (fade-in and slide-up by 20px on route change)."

---

### 2. Home Page (Hero, Search, Featured, Quick Links)

**Prompt for Stitch AI:**
"Design the Home Page for an ultra-luxury resort. The layout should use an irregular, asymmetrical grid to feel distinct and creative.

**1. Hero Section (Full Viewport):**

- Background: A slow, cinematic, auto-playing fullscreen video or high-res image of a mysterious, opulent resort element (e.g., water ripples in a dark pool with gold accents, or a luxury suite shadow).
- Overlay: A deep black gradient (bottom to top).
- Typography: Massive, bold serif typography overlapping the grid. 'Experience Luxury Like Never Before.' Text should have a subtle, elegant reveal animation (letters blur into focus on load).
- CTA: 'View Rooms' button is a sleek, borderless button with a gold fill that drips upwards on hover.

**2. Availability Search Bar (Overlapping Hero):**

- A floating, glassmorphism card (`background: rgba(255,255,255,0.05)`, `backdrop-filter: blur(20px)`, 1px solid `rgba(212,175,55,0.3)` border).
- Inputs: 'Check-in', 'Check-out', 'Guests'. No harsh borders; use bottom hairline borders that turn gold on focus. Date pickers should trigger a sleek, dark-themed modal calendar.
- Submit: A bold, blocky 'Check Availability' button with a magnetic hover effect.

**3. Featured Rooms (Irregular Carousel):**

- Title: 'Our Featured Rooms' in massive serif, left-aligned.
- Layout: A horizontal scroll carousel, but cards are irregular sizes (some 300px, some 500px height).
- Cards: Borderless. On hover, the image zooms slightly (scale 1.05), a deep black veil fades in at the bottom, and the room name/price slide up from the bottom in gold serif. The mouse pointer ring turns into a custom 'View' text icon.

**4. Quick Links (Bento Grid):**

- An irregular 2-column grid. 'Our Restaurant' and 'Amenities'.
- Massive imagery. On hover, the image scales back slightly (scale 0.95) revealing a brushed gold background behind the image, creating a framing effect."

---

### 3. Room Catalogue Page

**Prompt for Stitch AI:**
"Design the Room Catalogue page. Avoid standard 3-column grids.

**Layout:**

- Header: Huge, bold serif text 'Our Rooms' aligned to the left, with vast negative space above.
- Grid: A masonry-style irregular grid that breaks the conventional layout. Rooms of varying heights and widths.
- Cards: Pure imagery. No text visible by default.
- Hover Effect: When hovering over a room, a sophisticated dark gradient overlays the image. The room name appears in massive serif font, overlapping the image boundaries. The price and max occupancy fade in beneath the name in minimalist sans-serif. A thin, 1px gold border traces the outer edge of the card on hover.
- Click: Clicking the card triggers a page transition.
- Loading State: Replace standard spinners with an elegant, slow-rotating golden geometric shape (like a thin diamond)."

---

### 4. Room Detail Page

**Prompt for Stitch AI:**
"Design the Room Detail page for an ultra-exclusive resort. The layout should feel like a high-end editorial magazine spread.

**1. Image Gallery:**

- Layout: A horizontal, full-width scroll gallery. No visible scroll bars; use custom scrolling.
- Images: Fullscreen height (80vh), edge-to-edge.
- Interaction: As the user scrolls horizontally, a subtle parallax effect applies to alternate images.

**2. Room Info (Overlapping the Gallery):**

- A glassmorphic dark panel that overlaps the bottom left of the gallery.
- Typography: Room name in colossal serif. Description in muted, light grey sans-serif with wide line-height.
- Details Grid: Price, Max Occupancy, Square Footage. Display these as huge numbers in gold, with tiny labels beneath them.
- Bed Configuration: Display as an elegant list with custom minimalist line-art icons (no standard Material icons).
- CTA: 'Check Availability' is a massive, full-width button at the bottom. On hover, the background fills with solid gold and the text turns obsidian black."

---

### 5. Availability & Booking Search Page

**Prompt for Stitch AI:**
"Design the Availability Search & Results page. The aesthetic should be mysterious and highly curated.

**1. Search Form Section:**

- A minimalist, full-width section. Inputs are borderless, separated by thin vertical gold lines.
- Date inputs open a dark-themed modal calendar with gold highlight dates.
- 'Search' button is a minimalist text link with an animated arrow that extends on hover.

**2. Results Section:**

- Layout: A single-column, full-width list.
- Result Card: A horizontal split. 60% high-res image on the left, 40% dark info panel on the right.
- Info Panel: Room name in serif, price in gold. 'Available: X room(s)' displayed as small dots (e.g., 3 gold dots for 3 rooms) rather than text.
- Hover: The image pans slightly (Ken Burns effect). A 'Book Now' button slides in from the right edge of the panel.
- Empty State: If no rooms, display a massive, elegant serif quote: 'Awaiting your next visit,' with a subtle gold animated underline."

---

### 6. Menu Page

**Prompt for Stitch AI:**
"Design the Menu page to resemble a physical, ultra-luxury fine-dining menu.

**Layout:**

- Hero: Small, mysterious image of culinary art.
- Categories: Display categories as sticky, large serif text on the left side of the screen. Clicking a category smoothly scrolls to that section.
- Items Layout: Center-aligned, vast negative space.
- Item Display: No cards. The item name is in serif, the price is in gold on the far right. A custom, hand-drawn dotted line connects the name to the price.
- Hover Effect: Hovering over an item slightly enlarges the text (scale 1.02) and changes the text color to gold. A subtle circular background glow appears behind the text.
- Images: No standard food images. If an image is present, it appears as a small, circular thumbnail that expands to a full square on hover."

---

### 7. Amenities Page

**Prompt for Stitch AI:**
"Design the Amenities page using a Bento Box grid layout.

**Layout:**

- Hero: Minimalist, vast negative space.
- Bento Grid: A 4-column irregular grid.
  - Spa: Takes up 2 columns, wide image.
  - Gym: 1 column, tall image.
  - Pool: 1 column, square image.
- Cards: On load, images are slightly desaturated. On hover, images become full color, a dark veil drops down, and the amenity name appears in massive serif text overlapping the grid boundaries. Price appears in gold. If complimentary, a gold 'Complimentary' seal stamp fades in.
- Custom Cursor: Cursor turns into a small '+' icon on hover over amenities, inviting a click for more details."

---

### 8. Auth / Login (Modal or Page)

**Prompt for Stitch AI:**
"Design the Login / Authentication page. It should feel like entering a private, exclusive vault.

**Layout:**

- Full-screen obsidian black background.
- Centered card with extreme negative space inside.
- Card: Glassmorphic, floating, with a subtle gold glow shadow.
- Inputs: No borders, no boxes. Just minimalist 'floating labels' in grey that turn gold when typing. The input line is a single hairline that turns gold on focus.
- CTA: 'Enter' button is a full-width gold pill. On hover, a subtle ripple effect emanates from the click point.
- Background Effect: A very slow, barely noticeable particle system of gold dust floating in the background."

---

### 9. User Profile & Booking Wizard (Post-Login)

**Prompt for Stitch AI:**
"Design the User Profile and Booking Wizard interface for the top 0.0001%.

**1. Profile Dashboard:**

- Layout: Split screen. Left side is a static, vertical navigation with massive serif typography. Right side is the content area.
- Profile Info: Displayed as an elegant business card. Minimalist lines, gold accents for roles (e.g., 'Elite Member').
- Edit Mode: Clicking edit doesn't open a form; it transforms the text into inline editable fields with a subtle gold underline.

**2. Booking Wizard (Modal/Overlay):**

- Triggered from the Availability page. Opens as a full-screen overlay with `backdrop-filter: blur(20px)`.
- Stepper: A minimalist progress indicator at the top (3 thin gold lines that fill as steps complete).
- Step 1 (Dates): Huge, elegant calendar UI.
- Step 2 (Room): The pre-selected room is shown large. Options to modify are subtle text links.
- Step 3 (Confirm): Summary of booking in an editorial layout. 'Confirm Booking' button has a luxury loading state (gold dust gathering into a solid button)."

---

### Questions to Refine Your Exact Vision (If you want to iterate further):

1. **Specific Color Palette:** Are you leaning strictly towards Obsidian/Black & Gold, or would you like to explore alternatives like Deep Navy & Platinum, or Forest Green & Cream?
2. **Imagery Style:** Should the imagery be bright and airy (luxury natural light) or dark, moody, and mysterious (nighttime, atmospheric shadows)?
3. **Typography Preference:** Do you prefer a classical, high-contrast serif (like Didot or Bodoni) for a fashion-magazine wealth vibe, or a softer, wider serif (like Cormorant) for a modern, quiet-luxury vibe?
4. **Custom Cursor Details:** Do you want the custom cursor to interact differently with text vs. images? (e.g., turning into a reading icon over text, and a viewfinder over images).
5. **Micro-animations:** Are you comfortable with heavy CSS animations (like text masking, image reveals on scroll) or do you want to keep interactions strictly to hover/click for performance reasons?

