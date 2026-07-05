# Specsheet: Global Theme & Design Tokens

## 1. Purpose
- Establish the “Obsidian & Champagne” design system as a set of SCSS files and CSS custom properties.
- Provide the typography, colors, spacing, glassmorphism mixins, and global resets that every subsequent page refactor will rely on.
- No functional logic or services are modified.

## 2. Files to Create / Modify
| File | Action |
|------|--------|
| `src/styles.scss` | Replace with new global styles, import `_theme.scss`, apply resets. |
| `src/assets/fonts/` | *(optional)* If fonts are self‑hosted, place here; otherwise we use Google Fonts CDN. |
| `src/index.html` | Update `<head>` to include Google Fonts links and Material Symbols. |
| **New:** `src/styles/theme/_colors.scss` | Colour custom properties. |
| **New:** `src/styles/theme/_typography.scss` | Typography custom properties and mixins. |
| **New:** `src/styles/theme/_spacing.scss` | Spacing custom properties. |
| **New:** `src/styles/theme/_glassmorphism.scss` | Glassmorphism mixin. |
| **New:** `src/styles/theme/_mixins.scss` | Shared mixins (underline‑reveal, etc.). |
| **New:** `src/styles/theme/_index.scss` | Forward all theme partials. |

## 3. Design Tokens – Exact Values (from `design.md`)

### 3.1 Colours (`_colors.scss`)
```scss
:root {
  --color-surface: #131411;
  --color-surface-dim: #131411;
  --color-surface-bright: #393936;
  --color-surface-container-lowest: #0e0e0c;
  --color-surface-container-low: #1b1c19;
  --color-surface-container: #1f201d;
  --color-surface-container-high: #2a2a27;
  --color-surface-container-highest: #353532;
  --color-on-surface: #e4e2dd;
  --color-on-surface-variant: #c4c7c7;
  --color-outline: #8e9192;
  --color-outline-variant: #444748;
  --color-primary: #c9c6c5;
  --color-on-primary: #313030;
  --color-primary-container: #0a0a0a;
  --color-on-primary-container: #7b7979;
  --color-secondary: #e4c285;
  --color-on-secondary: #412d00;
  --color-secondary-container: #5d4514;
  --color-on-secondary-container: #d5b478;
  --color-background: #131411;
  --color-on-background: #e4e2dd;
  --color-tertiary: #c8c6c5;
  --color-on-tertiary: #313030;
  --color-tertiary-container: #0a0a0a;
  --color-on-tertiary-container: #7a7979;
  --color-error: #ffb4ab;
  --color-on-error: #690005;
  --color-error-container: #93000a;
  --color-on-error-container: #ffdad6;
  // Glassmorphism
  --glass-bg: rgba(26, 26, 26, 0.7);
  --glass-border: rgba(228, 194, 133, 0.2);
}
```

### 3.2 Typography (`_typography.scss`)
```scss
:root {
  --font-headline: 'Playfair Display', serif;
  --font-body: 'Manrope', sans-serif;

  --fs-display-lg: 72px;
  --fs-display-lg-mobile: 40px;
  --fs-headline-md: 32px;
  --fs-headline-sm: 24px;
  --fs-body-lg: 18px;
  --fs-body-md: 16px;
  --fs-label-caps: 12px;

  --lh-display-lg: 1.1;
  --lh-display-lg-mobile: 1.2;
  --lh-headline-md: 1.3;
  --lh-headline-sm: 1.4;
  --lh-body-lg: 1.6;
  --lh-body-md: 1.6;
  --lh-label-caps: 1.0;

  --ls-display-lg: -0.02em;
  --ls-display-lg-mobile: -0.01em;
  --ls-body-lg: 0.02em;
  --ls-body-md: 0.01em;
  --ls-label-caps: 0.2em;
}

// Mixins
@mixin font-display-lg {
  font-family: var(--font-headline);
  font-size: var(--fs-display-lg);
  font-weight: 400;
  line-height: var(--lh-display-lg);
  letter-spacing: var(--ls-display-lg);
}
@mixin font-display-lg-mobile {
  font-family: var(--font-headline);
  font-size: var(--fs-display-lg-mobile);
  font-weight: 400;
  line-height: var(--lh-display-lg-mobile);
  letter-spacing: var(--ls-display-lg-mobile);
}
@mixin font-headline-md {
  font-family: var(--font-headline);
  font-size: var(--fs-headline-md);
  font-weight: 400;
  line-height: var(--lh-headline-md);
}
@mixin font-headline-sm {
  font-family: var(--font-headline);
  font-size: var(--fs-headline-sm);
  font-weight: 400;
  line-height: var(--lh-headline-sm);
}
@mixin font-body-lg {
  font-family: var(--font-body);
  font-size: var(--fs-body-lg);
  font-weight: 300;
  line-height: var(--lh-body-lg);
  letter-spacing: var(--ls-body-lg);
}
@mixin font-body-md {
  font-family: var(--font-body);
  font-size: var(--fs-body-md);
  font-weight: 300;
  line-height: var(--lh-body-md);
  letter-spacing: var(--ls-body-md);
}
@mixin font-label-caps {
  font-family: var(--font-body);
  font-size: var(--fs-label-caps);
  font-weight: 500;
  line-height: var(--lh-label-caps);
  letter-spacing: var(--ls-label-caps);
  text-transform: uppercase;
}
```

### 3.3 Spacing (`_spacing.scss`)
```scss
:root {
  --space-unit: 8px;
  --gutter: 32px;
  --margin-desktop: 80px;
  --margin-mobile: 24px;
  --section-gap: 160px;
  --container-max: 1440px;
}
```

### 3.4 Glassmorphism Mixin (`_glassmorphism.scss`)
```scss
@mixin glass-panel {
  background: var(--glass-bg);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid var(--glass-border);
}
```

### 3.5 Additional Mixins (`_mixins.scss`)
```scss
@mixin gold-underline {
  position: relative;
  padding-bottom: 4px;
  &::after {
    content: '';
    position: absolute;
    bottom: 0;
    left: 0;
    width: 100%;
    height: 1px;
    background-color: var(--color-secondary);
    transition: width 0.5s ease;
  }
  &:hover::after { width: 0%; }
}
@mixin hover-scale-img {
  img {
    transition: transform 1.2s cubic-bezier(0.2, 0, 0.2, 1);
  }
  &:hover img { transform: scale(1.05); }
}
@mixin underline-reveal {
  position: relative;
  overflow: hidden;
  &::after {
    content: '';
    position: absolute;
    bottom: 0;
    left: 0;
    width: 100%;
    height: 1px;
    background: var(--color-secondary);
    transform: scaleX(0);
    transform-origin: right;
    transition: transform 0.6s cubic-bezier(0.19, 1, 0.22, 1);
  }
  &:hover::after {
    transform: scaleX(1);
    transform-origin: left;
  }
}
```

### 3.6 Global Reset & Scrollbar (`styles.scss`)
```scss
@import 'theme/index';

*, *::before, *::after {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}
html {
  -webkit-text-size-adjust: 100%;
}
body {
  background-color: var(--color-background);
  color: var(--color-on-background);
  @include font-body-md;
  overflow-x: hidden;
  -webkit-font-smoothing: antialiased;
}
// Hide scrollbar
::-webkit-scrollbar { display: none; }
* { -ms-overflow-style: none; scrollbar-width: none; }
// Selection
::selection {
  background: rgba(228, 194, 133, 0.3);
  color: var(--color-secondary);
}
// Utility
.sr-only { // for screen reader only
  position: absolute; width: 1px; height: 1px; overflow: hidden; clip: rect(0,0,0,0); border: 0;
}
```

## 4. Font Loading (`index.html`)
Add inside `<head>`:
```html
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
<link href="https://fonts.googleapis.com/css2?family=Manrope:wght@300;500&family=Playfair+Display:ital,wght@0,400;1,400&display=swap" rel="stylesheet">
<link href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:wght,FILL@100..700,0..1&display=swap" rel="stylesheet">
```

## 5. Self‑Review Checklist
- [ ] Theme partials compile without errors.
- [ ] CSS custom properties available globally.
- [ ] Google Fonts and Material Symbols load correctly.
- [ ] Body background is `#131411`, text is `#e4e2dd`.
- [ ] Glassmorphism mixin applies correct styles.
- [ ] No impact on existing component logic.

---
