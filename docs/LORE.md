# Boplborne — lore and story harness

The game is called **Boplborne**. (Docs, the Unity menu and code namespaces still say "Grace Run"; a rename is a separate task.)

## Premise

The Boplborne are small round pilgrims in hats. The tower's great **Grace Bell** once rang out a light that let the fallen rise again. It was silenced long ago and the tower has been dark since. Each boss holds a piece of it; the **Crowned Hollow** at the top sits on the rest.

Tone: whimsical 1930s cartoon, a little melancholy. Captions are short, dry and kind. No gore, no grimdark.

## What the game's parts mean

| Game element | Meaning |
| --- | --- |
| Gravestone and ghost | Grace is failing, so a fallen pilgrim still floats and a friend can call them back |
| Grace Totem, flasks | Scraps of bell-light, bottled or planted |
| Hats (meta unlocks) | Each hat belonged to a pilgrim who climbed before. Wearing one carries their climb on |
| The Bell Twins (floor 3) | The tower's own bell-ringers, gone mad. Their ringing is why the bell is wrong |
| The Gatekeeper (floor 1) | Keeper of the key-ring; will not let anyone pass |
| The Crowned Hollow (final) | **A pilgrim who reached the top.** It put on every hat it found, until nothing of the pilgrim was left under them. That is why it borrows attacks from this run's bosses and copies one ability from each player: it is the climb itself, hollowed out |

### The twist, and how it is seeded

The Hollow was a pilgrim like the players. Hints, in order, so nothing is spoiled early:
1. Floor 2 caption mentions a "lost hat" on the stairs.
2. Floor 5 (Mirror Duelist): "Everyone looks like somebody up here."
3. Floor 7 caption: "Someone has been up here before you. Someone with a lot of hats."
4. Final intro card shows the Hollow's silhouette wearing the party's own hat colours.
5. After the win, the Hollow's crown falls off and a plain small pilgrim is left, who hands over the clapper. Then the bell rings.

## How the story is shown (decision 28)

The story is a full-screen scene, not an overlay. When the leader presses Start in the hub (or Next floor at a rest stop, or a draft ends into the next floor) the server enters the `Story` phase. Every PC plays the story full screen, tells the server when it is through (or skips), and waits on a "Waiting for the others..." screen. The countdown and the boss fight start only when every connected player is through (timeout `FightData.StoryTimeoutSeconds`, 90 s, so a stuck PC cannot freeze the run). Floor 1 plays the opening and then the ascent; later floors play the ascent only. Joining is refused during the story.

## Opening (floor 1 only, about 10 s, skippable with Enter / click, Backspace skips all)

Five held-frame story cards in title-card style (the existing `TitleCardView` look: plum marquee, orange ribbon caption). Played after the leader presses Start in the hub, right before the ascent into floor 1.

| # | Picture | Caption |
| --- | --- | --- |
| 1 | Dusk, a tiny party walks toward a huge tower under a cracked sky | `BOPLBORNE` (title) / "The bell has been quiet a long time." |
| 2 | The gate: huge doors, a bell with no clapper above them | "Nobody remembers who took the clapper." |
| 3 | The doors are locked; a key-ring hangs far out of reach | "Locked. Naturally." |
| 4 | The party squeezes through a cat-flap beside the door | "There is always a smaller way in." |
| 5 | The door swings shut behind them; the Hub (ground floor) behind | "No way back but up." |

## Ascent screen (before every floor, about 3 s, skippable)

One reusable full-screen scene: a tall tower cross-section, one window per floor, a lantern marker that climbs from the old floor to the new one. Beaten floors go dark with a small trophy (a bell shard, a web scrap, ...). Shown on every PC in the story phase of every floor.

| Going to floor | Caption |
| --- | --- |
| 1 | "Floor 1. The key-holder is awake." |
| 2 | "Floor 2. Something up here is knitting. A lost hat lies on the stairs." |
| 3 | "Floor 3. Do you hear that? Neither do the bells." |
| 4 | "Floor 4. The weather is indoors." |
| 5 | "Floor 5. Everyone looks like somebody up here." |
| 6 | "Floor 6. Tick. Tock. Tick. Tick. Tock." |
| 7 | "Floor 7. Someone has been up here before you. Someone with a lot of hats." |
| Final | "The top. The crown is waiting." |

## Ending and defeat

- **Win:** the Hollow falls, its crown falls off, a plain small pilgrim hands over the clapper, the bell rings, and a closing shot shows every hat of the run's players in a row. Card: "CURTAIN CALL" / "The bell rings. The light is back."
- **Defeat:** the lantern goes out and the tower closes over. Card: "TAKE TWO" (the existing defeat card) with a "To be continued…" frame.

## Later (not in this build plan)

- Hat names and a one-line story per hat, shown on unlock.
- A real animated clip for floor 4 and the final floor.
- Rename Grace Run to Boplborne in docs, menus and namespaces.
