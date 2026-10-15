# Playtesting

## Before the session (host)

- [ ] Everyone has the **same build** (check the version on the main menu).
- [ ] Everyone is online in the Hamachi network (green dots).
- [ ] Host starts the game and clicks Host; share the Hamachi IPv4.
- [ ] Someone records the screen (OBS) or at least takes notes. Funny moments matter as much as bugs.

## During

Don't explain the game first. Watch where people get confused; that's the most valuable data.

Write down, with a timestamp if recording:

- Deaths that felt **unfair** ("I dodged that!"). Note which attack and who.
- Moments everyone **laughed or yelled**. Note what caused it.
- Combos that happened **without being explained**.
- Anything that felt laggy or jumped around (rubber-banding).
- Moments people were **bored or waiting**.

## After (5 minutes, everyone)

Ask each player and write the answers down:

1. Do you want to play another run right now? (yes / maybe / no)
2. Best moment?
3. Most frustrating moment?
4. Which boss attack felt unfair?
5. Which ability was the most fun, and which did nobody pick?
6. Anything confusing?

## Then

Paste all notes into Claude Code with `/playtest <notes>`. The agent sorts them into bugs, tuning, missing features and ideas, ranks them, and adds them to `docs/STATUS.md`.

## Template

```
Date:
Build version:
Players:
Run result (floor reached, win/lose):
Another run? (per player):
Best moments:
Frustrations:
Unfair deaths (attack, who):
Combos that happened:
Lag / desync:
Bugs:
Ideas:
```
