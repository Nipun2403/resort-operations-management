Here are the highly refined, ready-to-paste prompts for Stitch AI, incorporating your exact aesthetic choices: the Obsidian/Gold palette, dark and moody imagery, Cormorant typography, the velocity-based square cursor, and heavy scroll-triggered micro-animations.

### How to use these in Stitch AI:

Start a new project. Feed the **Master Shell** prompt first to establish the global design system, then create new screens/pages for the subsequent prompts, ensuring Stitch AI links them to the main shell.

---

### 1. Master Shell & Global Design System

**Prompt for Stitch AI:**
"Create a global layout shell for an ultra-luxury hotel resort catering to the top 0.0001%. The aesthetic must be opulent, mysterious, and completely distinct from standard hotel websites.

**Global Aesthetic:**

- Colors: Deep obsidian black (#050505) background, brushed gold (#D4AF37) accents, muted champagne (#F7E7CE) for secondary text.
- Typography: Use 'Cormorant Garamond' for all headings (massive, ultra-light weight 300, with wide letter-spacing). Use 'Inter' for body text (muted grey, #A0A0A0).
- Imagery: All images should be dark, moody, and cinematic (shadows, nighttime lighting, deep contrasts).
- Custom Cursor: Replace the default cursor with a small 12px solid obsidian square with a 1px gold border. When the mouse moves fast, the square scales up to 30px and stretches horizontally based on velocity. When hovering over clickable elements, it morphs into a hollow gold circle.
- Scroll Animations: Text should use clip-path reveal animations (text masks upwards from the bottom as it enters the viewport).

**Header (Sticky):**

- Transparent by default with a heavy `backdrop-filter: blur(16px)` and a subtle 1px gradient gold bottom border.
- Left: Hotel monogram in gold serif.
- Right: Navigation links (Home, Rooms, Menu, Amenities). Hover effect: Links don't change color; a thin gold underline draws itself from left to right. 'Check Availability' is a solid gold pill button. 'Login' is minimalist text.
- Mobile: Full-screen obsidian overlay menu. Links appear in massive Cormorant font, staggered from top to bottom.

**Footer:**

- Vast negative space. A slow-moving, subtle golden gradient glow at the top border. Text is tiny, muted gold."

---

### 2. Home Page

**Prompt for Stitch AI:**
"Design the Home Page using the Master Design System (Obsidian black, gold accents, Cormorant font, custom square cursor). The layout must be an irregular, asymmetrical grid.

**1. Hero Section (Full Viewport):**

- Background: A dark, moody, cinematic video/image (e.g., a dimly lit luxury infinity pool at night with subtle gold reflections).
- Overlay: Deep black vertical gradient.
- Typography: 'Experience Luxury Like Never Before' in massive Cormorant Garamond. The text should have a blur-to-focus reveal animation on page load.
- CTA: 'View Rooms' button is a borderless text link with a 1px gold underline that extends on hover.

**2. Availability Search Bar (Overlapping Hero):**

- A floating, dark glassmorphism card (`background: rgba(10,10,10,0.6)`, `backdrop-filter: blur(20px)`, 1px solid `rgba(212,175,55,0.2)`).
- Inputs: 'Check-in', 'Check-out', 'Guests'. No harsh borders; use bottom hairline borders that turn gold on focus.
- Submit: A sleek gold square button that fills with champagne gold on hover.

**3. Featured Rooms (Irregular Carousel):**

- Title: 'Curated Sanctuaries' in Cormorant, left-aligned, with a clip-path scroll reveal.
- Layout: A horizontal scroll carousel. Cards are irregular sizes (some 300px, some 600px height).
- Cards: Dark, borderless images. On hover, the image brightens slightly, a deep black veil fades in, and the room name slides up from the bottom in gold serif. The custom cursor morphs into a 'View' text label.

**4. Quick Links (Bento Grid):**

- A 2-column irregular grid. Dark moody imagery.
- Hover Effect: On hover, the image scales back slightly (scale 0.95) revealing a brushed gold background behind the image, creating a framing effect."

---

### 3. Room Catalogue Page

**Prompt for Stitch AI:**
"Design the Room Catalogue page using the Master Design System (Obsidian black, gold, Cormorant).

**Layout:**

- Header: Huge, bold serif text 'Our Sanctuaries' aligned to the left. Vast negative space.
- Grid: A masonry-style irregular grid that breaks conventional layouts.
- Cards: Pure dark, moody imagery. No text visible by default.
- Hover Effect: When hovering, a sophisticated dark gradient overlays the image. The room name appears in massive Cormorant font, overlapping the image boundaries. The price and max occupancy fade in beneath in minimalist sans-serif. A thin, 1px gold border traces the outer edge of the card.
- Loading State: Replace standard spinners with an elegant, slow-rotating golden geometric diamond."

---

### 4. Room Detail Page

**Prompt for Stitch AI:**
"Design the Room Detail page using the Master Design System (Obsidian black, gold, Cormorant). The layout should feel like a high-end dark editorial magazine spread.

**1. Image Gallery:**

- Layout: A horizontal, full-width scroll gallery. No visible scroll bars.
- Images: Fullscreen height (80vh), edge-to-edge. Dark, moody photography.
- Interaction: As the user scrolls horizontally, a subtle parallax effect applies to alternate images.

**2. Room Info (Overlapping the Gallery):**

- A glassmorphic obsidian panel that overlaps the bottom left of the gallery.
- Typography: Room name in colossal Cormorant Garamond. Description in muted grey sans-serif.
- Details Grid: Price, Max Occupancy, Square Footage. Display these as huge numbers in gold, with tiny labels beneath them.
- Bed Configuration: Display as an elegant list with custom minimalist gold line-art icons.
- CTA: 'Check Availability' is a massive, full-width button. On hover, the background fills with solid gold and the text turns obsidian black."

---

### 5. Availability & Booking Search Page

**Prompt for Stitch AI:**
"Design the Availability Search & Results page using the Master Design System (Obsidian black, gold, Cormorant).

**1. Search Form Section:**

- A minimalist, full-width section. Inputs are borderless, separated by thin vertical gold lines.
- Date inputs open a dark-themed modal calendar with gold highlight dates.
- 'Search' button is a minimalist text link with an animated arrow that extends on hover.

**2. Results Section:**

- Layout: A single-column, full-width list.
- Result Card: A horizontal split. 60% dark moody image on the left, 40% obsidian info panel on the right.
- Info Panel: Room name in Cormorant, price in gold. 'Available: X room(s)' displayed as small gold squares (e.g., 3 gold squares for 3 rooms) rather than text.
- Hover: The image pans slightly (Ken Burns effect). A 'Book Now' button slides in from the right edge of the panel.
- Empty State: If no rooms, display a massive, elegant Cormorant quote: 'Awaiting your next visit,' with a subtle gold animated underline."

---

### 6. Menu Page

**Prompt for Stitch AI:**
"Design the Menu page using the Master Design System (Obsidian black, gold, Cormorant). It should resemble a physical, ultra-luxury fine-dining menu in the dark.

**Layout:**

- Hero: Small, mysterious dark image of culinary art.
- Categories: Display categories as sticky, massive Cormorant text on the left side of the screen. Clicking a category smoothly scrolls to that section.
- Items Layout: Center-aligned, vast negative space.
- Item Display: No cards. The item name is in Cormorant, the price in gold on the far right. A custom, hand-drawn dotted gold line connects the name to the price.
- Hover Effect: Hovering over an item slightly enlarges the text (scale 1.02) and changes the text color to gold. A subtle dark circular background glow appears behind the text.
- Images: No standard food images. If an image is present, it appears as a small, circular thumbnail that expands to a full square on hover."

---

### 7. Amenities Page

**Prompt for Stitch AI:**
"Design the Amenities page using the Master Design System (Obsidian black, gold, Cormorant) using a Bento Box grid layout.

**Layout:**

- Hero: Minimalist, vast negative space.
- Bento Grid: A 4-column irregular grid.
  - Spa: Takes up 2 columns, wide dark moody image.
  - Gym: 1 column, tall dark image.
  - Pool: 1 column, square dark image.
- Cards: On load, images are dark and slightly desaturated. On hover, images brighten slightly, a deep black veil drops down, and the amenity name appears in massive Cormorant text overlapping the grid boundaries. Price appears in gold. If complimentary, a gold 'Complimentary' seal stamp fades in.
- Custom Cursor: The velocity square cursor turns into a small '+' icon on hover over amenities, inviting a click."

---

### 8. Auth / Login Page

**Prompt for Stitch AI:**
"Design the Login / Authentication page using the Master Design System (Obsidian black, gold, Cormorant). It should feel like entering a private, exclusive vault.

**Layout:**

- Full-screen obsidian black background.
- Centered card with extreme negative space inside.
- Card: Glassmorphic obsidian, floating, with a subtle gold glow shadow.
- Inputs: No borders, no boxes. Just minimalist 'floating labels' in grey that turn gold when typing. The input line is a single hairline that turns gold on focus.
- CTA: 'Enter' button is a full-width gold pill. On hover, a subtle ripple effect emanates from the click point.
- Background Effect: A very slow, barely noticeable particle system of gold dust floating in the dark background."

---

### 9. User Profile & Booking Wizard

**Prompt for Stitch AI:**
"Design the User Profile and Booking Wizard interface using the Master Design System (Obsidian black, gold, Cormorant).

**1. Profile Dashboard:**

- Layout: Split screen. Left side is a static, vertical navigation with massive Cormorant typography. Right side is the content area.
- Profile Info: Displayed as an elegant digital business card. Minimalist lines, gold accents for roles (e.g., 'Elite Member').
- Edit Mode: Clicking edit doesn't open a form; it transforms the text into inline editable fields with a subtle gold underline.

**2. Booking Wizard (Modal/Overlay):**

- Triggered from the Availability page. Opens as a full-screen overlay with `backdrop-filter: blur(20px)`.
- Stepper: A minimalist progress indicator at the top (3 thin gold lines that fill as steps complete).
- Step 1 (Dates): Huge, elegant dark-themed calendar UI.
- Step 2 (Room): The pre-selected room is shown large. Options to modify are subtle text links.
- Step 3 (Confirm): Summary of booking in an editorial layout. 'Confirm Booking' button has a luxury loading state (gold dust gathering into a solid button)."

