---
name: playtest
description: Turn playtest notes into a ranked backlog
argument-hint: <notes>
disable-model-invocation: true
---

Turn playtest notes into work. The user's notes: $ARGUMENTS

1. Sort every note into: bug, tuning, missing feature, or idea.
2. For each bug and tuning item, name the system and the data asset or script involved.
3. Rank them: what blocks fun first, polish last. Check each against the pillars in `docs/GDD.md`; mark ideas that serve no pillar as "parked".
4. Add the ranked list to `docs/STATUS.md` under "Playtest backlog" with today's date.
5. Do not fix anything yet. Ask which item to start with.
