# Unsavory — Narrative Toasts v3 (20 Workdays, Morning-Only) Implementation Specification

## Purpose and source of truth

Implement the attached `toasts_narrative_schema_20_workdays.txt` (JSON content) in the **existing Unsavory codebase**. Inspect the real scene flow, data loader, mail/bills, economy, work-shift progression, health, commute, save state, input routing, and modal UI before coding. Do not create a standalone game, substitute imagined mechanics, or treat earlier v2 instructions as valid when they conflict with this document.

This version corrects a **breaking structural mismatch** in v2: the game has **20 scheduled workdays, not 30**, and **home is always a morning scene**. There is no home night/evening or post-shift phase. `schema_version: 3` reflects those breaking corrections.

## Actual game loop (must be preserved)

```text
New Game
   ↓
Home — first morning (employment email + bills are available)
   ↓
Interact with phone, fruit bowl, pills, bed, or door
   ├── Door → choose drive/transit → Rocket Taco → work shift → next morning at Home
   └── Bed → choose to sleep/stay home, skipping today's workday → next morning at Home
   ↓
Repeat for up to 20 scheduled workdays, with existing loss/failure outcomes
   ↓
After final workday: enter terminal Home morning with end-of-run mail
   ↓
Player reads email for layoff reveal
```

**Important boundaries:**

- Only **two scene-entry events** are supported: `home` (morning) and `work` (arrival at Rocket Taco). Delete all references to `home_wake`, `after_shift`, `before_shift`, `phase` and ordinary evening sleep.
- `day` indexes **scheduled workdays 1 through 20**, not real-world dates or an assumed 21-day/30-day calendar. Week 1 = 1–5; Week 2 = 6–10; Week 3 = 11–15; Week 4 = 16–20. Do **not** create weekend home scenes merely because weekends exist on a calendar.
- The final Home morning may retain day 20 or advance the underlying counter depending on how the actual code works; **gate terminal dialogue on `run_end`**, not on day 21. Do not create a scheduled workday 21.
- The employment mail and bills are already present on the opening morning. Toasts should not consume, hide, read, or replace them. Allow the player to access them normally.
- The **bed's sleep action during Home morning is skipping that day's shift**, not an evening “sleep until the next workday” action. Only use `skip_shift` as the bed's actionable confirmation, mapped to the game's actual skip/sleep mechanic. It must advance the workday **once** and apply only the existing game's pay, health, and time rules.
- The front door's choices report to work. There is no return-home-after-shift scene before the next morning.
- Preserve premature endings, evictions, and whatever other game-over behavior exists. The normal final layoff is revealed by reading the end-of-run email, **not** by entry toast or modal.

## Narrative purpose and pacing

Narrative text has four layers:

1. **Scene entry:** one atmospheric `world` beat plus at most one `state` beat. `special` entries (first visit, ending, tutorial) are exclusive. Home lines always read as **morning**, never after work.
2. **Hover:** what the object means in the player's current circumstances; no side effects.
3. **Modal:** the cost of committing to the action; buttons call existing mechanics, not JSON-authored economy/health logic.
4. **After action:** short outcome text only when it can be shown reliably, without competing with an immediate transition.

The four week boundaries provide fallback pacing: 1–5 getting established; 6–10 changing world / illness if it actually occurs; 11–15 intensified routine and bills; 16–20 corporate future vs. worker's short-term survival. The schema does **not** dictate when the player becomes ill or when acquisitions or new products are revealed. Those facts must come from actual game systems and mail/dialogue flags. The attached lines are a starting point for editing, not new canonical story events.

## Canonical data shape

```json
{
  "schema_version": 3,
  "schema_contract": {"workday_count": 20},
  "scene_entry": {
    "home": {"special": [], "world": [], "state": []},
    "work": {"special": [], "world": [], "state": []}
  },
  "hover": {
    "phone": [], "bed": [], "door": [], "pills": [], "fruit_bowl": []
  },
  "modals": {"bed": [], "door": [], "pills": []},
  "after_action": {"skip_shift": [], "buy_pills": [], "take_pill": []}
}
```

Load the `.txt` as JSON if that is the project's actual expected extension. No migration to `.json` is needed if the existing resource path is `.txt`. Do not leave older data or old events simultaneously active.

## Rule evaluator

- **First matching rule wins within each ordered array**. An absent `when` matches all contexts. Predicates within `when` are ANDed. Missing context keys must **not** be coerced to 0/false or accidentally match.
- Support the game's comparator strings (`==1`, `>=16`, `<30`, `>0`, `<=2`) and literal strings (`insurance`, `payment`). Never use `eval`.
- Use the same evaluator for optional `choice.enabled_when`, and validate enablement again when the action fires. Show disabled choices as disabled; they must be inaccessible to mouse, keyboard, and gamepad.
- Support `text`, `lines`, `title`, `choices`, and safe `{variable}` interpolation from authoritative state, without interpreting expressions in placeholders.
- Give every rule a stable `id`. `once:"run"` must be recorded only **after display** and persisted in the save, reset at New Game.
- Reject or warn clearly on unsupported actions, invalid `when` syntax, unresolved text placeholders, duplicate IDs, and malformed modal data.

## Entry event routing

For either `scene_entry.home` or `scene_entry.work` on a real transition (never a redraw):

1. Evaluate `special` first. If matched, show it alone. Examples: first Home intro, a terminal Home morning, first work tutorial.
2. Otherwise independently select the first `world` and first `state` rules. Show **world first, then state**, without toast overlap or duplicating a line in rapid succession.
3. Do not allow the initial mail setup, pressing Cancel, hover re-entry, saved-scene restoration, or a modal repaint to trigger the entry text again.
4. Do not show ordinary work-entry lines for catering/special shift flows if the game suppresses them today.
5. At `run_end == 1`, only say that an email is waiting; never say “laid off” or otherwise spoil the final mail. At eviction or other terminal values, respect the game's actual codes and transitions.

Before applying v3, **verify the meaning of `day`, `run_end`, `first_visit`, `tutorial_done`, and `catering` in the source**. If they differ from the example schema, adapt the context provider and report the mapping, not the mechanics.

## Hover behavior

Route the original `hover_phone`, `hover_bed`, `hover_door`, `hover_pills` to `hover.phone`, `hover.bed`, `hover.door`, `hover.pills`, plus the new `hover.fruit_bowl`. Hover must never advance a day or consume a resource. Preserve controller focus/hover conventions.

Fruit bowl text reflects `fruit_count` as it currently exists in the inventory/groceries mechanic. The v3 file does **not** introduce a new fruit consumption rule or a fruit modal.

## Contextual modals

### Bed — morning only

- Interaction opens an explicit **skip-work/sleep-through-shift** confirmation. Distinguish sick from healthy and rent-due from ordinary mornings in copy.
- `skip_shift` applies the existing missing-work behavior (time advance, no wages, health consequences if any), **once**. The next visual destination is the **next morning Home** or an existing game-over path; not an evening Home.
- `cancel` closes the modal without modifying game state. Never include `sleep` as a separate evening action.

### Door — reporting to work

- If car is usable, offer Drive and Public Transit (if both are supported in the game). When car is unavailable, offer the viable transit path only; show the actual `insurance` vs. `payment` cause when known.
- Distinguish illness in the modal copy. Don't claim commute time or lateness that gameplay hasn't computed.
- `drive_to_work` and `transit_to_work` use the existing commute/work transition. An action must execute once, recheck `car_ok`, and not create a second workday.
- If the existing game does not permit a player to choose transit voluntarily, **report the incompatibility** and implement the smallest consistent UI behavior; do not silently rewrite transportation gameplay to match example copy.

### Pills — buy or consume

- Branch on `pills`, health, and `can_afford_pills`. The context's `{pill_price}` comes from the actual current price, not a number embedded in the schema.
- Use `take_pill` / `buy_pills` via the existing inventory and health/economy code; never deduct twice. Check inventory and affordability at commit, not just when the modal opens. Disabled Buy More must not be clickable.
- No day or scene advance is caused by medicine interactions unless one already exists in the code.

### All modals

- Use the existing UI, keyboard, mouse, and controller navigation with sane focus and cancel/back behavior.
- Close/disable immediately upon confirming a transition to prevent duplicate button presses, rapid re-entry, double wages or double medication spend.
- Data chooses **copy and action identifiers**, while game logic remains authoritative for consequences.

## After-action notifications and transitions

- `buy_pills` and `take_pill` may show immediate feedback on the current morning screen after successful actions.
- `skip_shift` generally **transitions directly to the next morning**. Do not flash an after-action toast that disappears with the current scene. Either queue the text for delivery on the next Home morning, or let the next Home `state` show a `skipped_previous_shift` response derived from the actual completed action. Avoid double-delivering both. Only introduce the smallest necessary run-scoped handoff if the engine cannot represent this already.
- No `after_action.sleep`, evening return, or post-shift home event.

## Context mapping

| Key | Expected source / meaning |
| --- | --- |
| `day` | Current/upcoming **workday** 1..20, while playable. Terminal home uses `run_end`. |
| `run_end` | Existing terminal outcome indicator, validated against actual code. |
| `first_visit` | Initial new-game Home intro (only once/run). |
| `health` | Existing health quantity; examples assume sick `<30` (confirm the real threshold). |
| `rent_pending`, `missed_bills`, `bill_due` | Existing billing system; expose read-only. |
| `unread_mail` | Existing phone/mail state; opening morning may already contain mail. |
| `pills`, `pill_price`, `can_afford_pills` | Existing medication stock, price, real funds; derive affordability. |
| `car_ok`, `car_unavailable_reason`, `late` | Real transportation and late-arrival outcomes. |
| `fruit_count` | Real visible fruit stock; no made-up starting inventory. |
| `phone_service` | Real comms state, if supported. |
| `tutorial_done`, `catering` | Existing tutorial/special-shift rules. |
| `acquisition_known`, `new_vision_known` | Optional knowledge flags, true only when revealed via actually encountered boss/mail content. |
| `skipped_previous_shift` | Optional narrative handoff from actual last skipped workday, scoped/consumed as appropriate. |

Missing optional context should cause dependent rules to fail safely, not be treated as true. Do not gate the final email on acquiring a knowledge flag.

## Migration and compatibility

1. Locate the canonical toast resource, entry triggers, modal system, and current save version. Keep the file extension and interface expected by the project when possible.
2. Replace older flat keys and earlier v2 `home_wake`/`phase` routes with v3 `home`, `work`, `hover`, and `modals`. Do not fire a redundant morning Home toast from more than one route.
3. Keep all economy, wages, rent, sickness, commute, staffing, tutorials, catering, mail and boss dialogue logic unchanged, except minimal adapters necessary to invoke existing operations.
4. Persist `once:run` IDs and any necessary pending-next-morning feedback. Reset both at New Game; don't replay on reload or scene redraw.
5. If the current logic uses a different counter offset or end state, adapt the narrative context at the edge and document it; **do not alter gameplay day advancement** just to make toasts work.
6. Validate the JSON at load time with actionable errors naming the rule ID.

## Acceptance tests / QA

1. **Day 1:** New Game opens Home in the morning with intro, employment email and bills accessible; no invented night phase, no double toast on mail/menu open.
2. **Transitions:** Day 1 door → work 1 → Home morning day 2; repeat. No work-to-evening-home transition or extra sleep prompt.
3. **Skip:** Day 3 bed confirm → no work scene for day 3 → Home morning for next workday, one day advance, no wages (assuming current game's skipped-shift policy). No double transition on repeated input.
4. **20-day limit:** Workdays 1,5,6,10,11,15,16,19,20 have valid fallbacks and no day 21–30 entries. At the final completed shift, the next Home is terminal with the email available; do not expose a playable 21st shift.
5. **End reveal:** `run_end==1` Home entry points toward unread mail but does not say the player was laid off. Actual email retains the reveal. Eviction branch distinct.
6. **Two channels:** Home morning on day 6 with unpaid bills displays the world line and one state line in sequence, without suppressing the historical beat.
7. **Conditions:** Health below/at 30 chooses sick/healthy variants according to the game's actual sickness threshold. Bed and door both give sensible sick copy, without inventing recovery amounts.
8. **Driving:** Car available → Drive and Transit (subject to supported mechanics). Car unavailable from insurance/payment → reason-specific transit option; drive blocked by mouse/keyboard/controller. Lateness only as real commute calculated it.
9. **Pills:** Stock 0 vs >0, sufficient vs insufficient funds, take/buy, button disabled, no double purchase/use, no invented price.
10. **Fruit:** Counts 0,1,2,3+ choose appropriate hover, no action on hover and no extra consumption rule.
11. **Knowledge:** Acquisition/new-product text requires actual observed reveal state; day 16 or day 20 by itself never leaks this information.
12. **Persistence:** New Game resets run-once flags; load/re-enter doesn't replay home/work entry; skip handoff text cannot be duplicated across saves.
13. **Other flows:** Tutorial and catering behavior remain intact; early game-over/eviction still works; terminal end handled by run_end rather than day count alone.

## Deliverables to Claude user

- Working code in the actual project; no standalone mockup.
- The canonical revised JSON content installed as the project expects (likely `toasts.txt`), with tests for the matcher and transition integration.
- Short notes: edited files, old-to-new event mapping, any optional flags implemented, action handlers wired, test outcomes, and any unresolved mechanics.

**Definition of done:** A full **20-workday** run (including optional skipped workdays) uses a single morning Home entry per day, work arrival only when the player reports to work, contextual hover/modal text, safe action dispatch, correct special/ending handling, and no invented post-shift home or day-21 work content.
