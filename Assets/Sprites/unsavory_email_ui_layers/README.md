# Unsavory — Email/news UI layers

All exports are PNGs at original image resolution. Sprites use top-left pixel coordinates.

## Reusable inbox row
- `row_panel_blank.png` — isolated painted row panel with no icons, unread markers, text, or gray placeholders; alpha outside card.
- `row_unread_dot.png` — independent teal unread indicator (transparent PNG).
- `row_avatar_slot.png` — optional neutral round sender icon placeholder (transparent PNG). Replace with one of the government/news/employer/social/individual sender icons.
- `inbox_row.ora` — layered OpenRaster file for image editors.

## Message detail
- `detail_background_blank.png` — full screen including status strip, back arrow, section divider, and Email/Bills navigation. No placeholders or text.
- `detail_status_strip.png`, `detail_title_strip.png`, `detail_metadata_strip.png`, `detail_body_panel.png`, `detail_nav_strip.png` — separate independent strips that reassemble vertically into the screen.
- `message_view.ora` — editable OpenRaster stack of the five strips.
- `detail_back_arrow.png`, `detail_signal.png`, `detail_battery.png`, `detail_nav_email_symbol.png`, `detail_nav_bills_symbol.png`, `detail_sender_avatar_slot.png` — additional transparent symbol sprites. Treat these as optional alternatives to the icons baked into the separate strips; do not overlay both unless replacing them in the strip first.
- `detail_preview_no_text.png` — visual preview showing the panel without any fake text bars.

Use `layout.json` for original screen pixel coordinates and suggested dynamic sender, subject, date, body text, and nav placements. The bottom two nav tabs are **Email** and **Bills** only; no compose, delete, or archive controls.
