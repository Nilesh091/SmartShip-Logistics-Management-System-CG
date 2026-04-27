---
version: alpha
name: SmartShip Service Console

description: API-first documentation interface with neutral system UI styling and a branded transactional email layer.

colors:
  primary: "#3B4151"
  on-primary: "#FFFFFF"
  secondary: "#4990E2"
  on-secondary: "#FFFFFF"
  accent-brand: "#2E86C1"
  accent-success: "#49CC90"
  accent-warning: "#FCA130"
  accent-danger: "#F93E3E"
  accent-info: "#61AFFE"
  accent-patch: "#50E3C2"
  accent-head: "#9012FE"
  accent-options: "#0D5AA7"
  background: "#FAFAFA"
  surface: "#FFFFFF"
  surface-muted: "#F4F4F4"
  surface-elevated: "#FFFFFF"
  border-subtle: "#EBEBEB"
  border-strong: "#D9D9D9"
  text-primary: "#3B4151"
  text-secondary: "#777777"
  text-muted: "#999999"
  text-inverse: "#FFFFFF"
  topbar: "#1B1B1B"
  code-surface: "#333333"
  code-text: "#FFFFFF"
  email-canvas: "#F4F6F7"
  email-surface: "#FFFFFF"
  email-otp-surface: "#EEF2F7"
  email-footnote: "#777777"

typography:
  display:
    fontFamily: sans-serif
    fontSize: 36px
    fontWeight: "700"
    lineHeight: 44px
  heading-lg:
    fontFamily: sans-serif
    fontSize: 24px
    fontWeight: "400"
    lineHeight: 32px
  heading-md:
    fontFamily: sans-serif
    fontSize: 20px
    fontWeight: "600"
    lineHeight: 28px
  body-md:
    fontFamily: sans-serif
    fontSize: 14px
    fontWeight: "400"
    lineHeight: 22px
  body-sm:
    fontFamily: sans-serif
    fontSize: 12px
    fontWeight: "400"
    lineHeight: 18px
  label-md:
    fontFamily: sans-serif
    fontSize: 14px
    fontWeight: "700"
    lineHeight: 20px
  label-sm:
    fontFamily: sans-serif
    fontSize: 12px
    fontWeight: "600"
    lineHeight: 16px
    letterSpacing: 0.05em
  code:
    fontFamily: monospace
    fontSize: 12px
    fontWeight: "600"
    lineHeight: 18px

rounded:
  xs: 1px
  sm: 3px
  DEFAULT: 4px
  md: 6px
  lg: 8px
  full: 9999px

radii:
  xs: 1px
  sm: 3px
  DEFAULT: 4px
  md: 6px
  lg: 8px
  full: 9999px

spacing:
  xxs: 4px
  xs: 8px
  sm: 10px
  md: 12px
  lg: 16px
  xl: 20px
  xxl: 24px
  section: 30px
  hero-gap: 40px

shadows:
  subtle: "0 1px 2px rgba(0, 0, 0, 0.1)"
  card: "0 0 3px rgba(0, 0, 0, 0.19)"
  hover: "0 0 5px rgba(0, 0, 0, 0.3)"
  modal: "0 10px 30px rgba(0, 0, 0, 0.2)"
  email-card: "0 2px 8px rgba(0, 0, 0, 0.1)"

elevation:
  level-0:
    backgroundColor: "{colors.background}"
    shadow: none
  level-1:
    backgroundColor: "{colors.surface}"
    shadow: "{shadows.subtle}"
  level-2:
    backgroundColor: "{colors.surface-elevated}"
    shadow: "{shadows.card}"
  level-3:
    backgroundColor: "{colors.surface-elevated}"
    shadow: "{shadows.modal}"

motion:
  duration-fast: 150ms
  duration-medium: 250ms
  duration-slow: 500ms
  easing-standard: ease-in-out
  easing-emphasis: cubic-bezier(0.165, 0.84, 0.44, 1)

components:
  topbar:
    backgroundColor: "{colors.topbar}"
    textColor: "{colors.text-inverse}"
    typography: "{typography.heading-md}"
    padding: "{spacing.sm} 0"
  section-container:
    backgroundColor: "{colors.surface}"
    rounded: "{rounded.DEFAULT}"
    padding: "{spacing.xxl}"
  button-primary:
    backgroundColor: "{colors.secondary}"
    textColor: "{colors.on-secondary}"
    typography: "{typography.label-md}"
    rounded: "{rounded.DEFAULT}"
    height: 40px
    padding: "{spacing.xs} {spacing.xxl}"
  button-success:
    backgroundColor: "{colors.accent-success}"
    textColor: "{colors.on-primary}"
    typography: "{typography.label-md}"
    rounded: "{rounded.DEFAULT}"
  button-danger:
    backgroundColor: "{colors.accent-danger}"
    textColor: "{colors.on-primary}"
    typography: "{typography.label-md}"
    rounded: "{rounded.DEFAULT}"
  input-field:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.text-primary}"
    typography: "{typography.body-md}"
    rounded: "{rounded.DEFAULT}"
    padding: "{spacing.xs} {spacing.sm}"
  opblock-get:
    backgroundColor: "rgba(97, 175, 254, 0.1)"
    textColor: "{colors.text-primary}"
    rounded: "{rounded.DEFAULT}"
  opblock-post:
    backgroundColor: "rgba(73, 204, 144, 0.1)"
    textColor: "{colors.text-primary}"
    rounded: "{rounded.DEFAULT}"
  opblock-put:
    backgroundColor: "rgba(252, 161, 48, 0.1)"
    textColor: "{colors.text-primary}"
    rounded: "{rounded.DEFAULT}"
  opblock-delete:
    backgroundColor: "rgba(249, 62, 62, 0.1)"
    textColor: "{colors.text-primary}"
    rounded: "{rounded.DEFAULT}"
  code-block:
    backgroundColor: "{colors.code-surface}"
    textColor: "{colors.code-text}"
    typography: "{typography.code}"
    rounded: "{rounded.DEFAULT}"
    padding: "{spacing.sm}"
  email-card:
    backgroundColor: "{colors.email-surface}"
    textColor: "{colors.text-primary}"
    rounded: "{rounded.lg}"
    padding: "25px"
  email-otp-chip:
    backgroundColor: "{colors.email-otp-surface}"
    textColor: "#000000"
    typography: "{typography.heading-md}"
    rounded: "{rounded.md}"
    padding: "{spacing.md}"
---

## Overview

The visual identity is pragmatic and documentation-centric: neutral canvases, clear typography, and strongly color-coded interaction states. The overall tone prioritizes operational clarity over expressive branding.

A secondary visual layer appears in transactional email content, where a restrained brand accent and slightly softer card styling are used to reinforce trust and readability.

## Colors

The core interface is built on grayscale neutrals with a dark top bar and white working surfaces.

- Primary text and UI foregrounds rely on deep slate tones for readability.
- The shell background is an off-white neutral to reduce glare.
- Interaction emphasis is mostly semantic rather than decorative.

Semantic colors map to action types and status:

- GET-like informational actions use blue.
- POST-like success or creation actions use green.
- PUT-like update actions use amber.
- DELETE-like destructive actions use red.

Transactional email introduces one branded blue accent for headings while preserving conservative neutral surfaces.

## Typography

Typography follows a utility profile:

- Sans-serif for all navigational, form, and documentation text.
- Monospace for code snippets, paths, and payload-focused content.
- Heavier weights are used sparingly to signal labels and section titles.

Hierarchy is carried more by scale and spacing than by font variety.

## Layout & Spacing

Spacing uses a compact-to-comfortable scale anchored around 8px-derived steps.

- Tight controls and inline metadata use 8px to 12px spacing.
- Section-level containers use 20px to 30px padding/margins.
- Critical callout areas and grouped content blocks can expand to 40px rhythm.

This creates a dense but legible documentation surface that still supports scanning.

## Elevation & Depth

Depth is subtle and functional.

- Most content sits on flat white surfaces.
- Low-contrast shadows provide separation for cards, controls, and overlays.
- Modal contexts increase blur radius and spread to establish clear focus without dramatic contrast shifts.

## Shapes

Corner treatment is consistently small-to-medium rounded.

- Controls and cards primarily use 4px radii.
- OTP and emphasis chips use slightly larger radii for prominence.
- Fully rounded tokens are reserved for badges and pill patterns.

The shape language remains utilitarian, avoiding highly stylized geometry.

## Components

Component behavior emphasizes explicit state communication.

- Primary buttons are high-contrast and direct.
- Semantic action containers use tinted backgrounds with matching method colors.
- Input fields remain neutral with minimal ornamentation.
- Code surfaces invert to dark backgrounds for readability of technical payloads.
- Email cards retain a lightweight, trustworthy card aesthetic with modest depth.

## Do's and Don'ts

Do:

- Preserve semantic action colors and keep them consistent across interactive states.
- Keep body text in high-contrast slate on light surfaces.
- Use monospace only for machine-oriented or technical content.
- Maintain restrained shadows and avoid stacked heavy effects.

Don't:

- Replace the neutral foundation with saturated page backgrounds.
- Introduce decorative gradients or high-chroma accents into core documentation screens.
- Mix multiple display typefaces or ornamental fonts.
- Use oversized radii that conflict with the compact operational UI style.
