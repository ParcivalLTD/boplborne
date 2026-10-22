# How to play Boplborne

**Climb the tower. Bop the bosses.** A co-op boss rush for 1 to 4 friends. This page is for players; it is also the body of the GitHub release notes.

## Download

1. Open the **Releases** page of the repository and download the newest `Boplborne_v<version>_SingleFile.exe` (or the `_Portable.zip`).
2. Double-click it. Windows SmartScreen may warn on the first run because the game is not code-signed: click **More info → Run anyway**.
3. **Everyone in a room must run the same version.** A different version is refused with a message. The game checks for a newer release and offers to update itself.

## Host a game

1. Main menu → **Host game**.
2. Pick **Private** (only people with the code) or **Public** (listed in the **Rooms** page).
3. Open the room. The 6-character **room code** shows in the hub under the start button and is copied to your clipboard. Send it to your friends.
4. When everyone is in the hub, the host (the leader) presses **Start the show!**.

## Join a game

- **Join code:** main menu → **Join code**, type the 6-character code.
- **Rooms:** main menu → **Rooms**, pick a public room that says *Open*.
- You can join in the hub or during a draft or rest stop. A fight in progress refuses new players; wait for it to end and try again.

No VPN, port forwarding or shared network is needed: the game uses Unity Relay. Hamachi or a direct IP (`ip` or `ip:port` in the Join field) still works as a fallback.

## Controls

Default scheme (Settings → Controls setup lets you switch to mouse only or rebind everything):

| Action | Keyboard | Gamepad |
| --- | --- | --- |
| Move | A / D | Left stick |
| Jump (hold = higher, press again in the air for a second jump) | Space | A / Cross |
| Dodge roll (you cannot be hit while rolling) | Left Shift | B / Circle |
| Light attack (up / down swings, aerial swings) | J | X / Square |
| Heavy attack | U | Right trigger |
| Abilities 1 / 2 / 3 | K / L / ; | Y / LB / RB |
| Flask (heal) | R | D-pad up |
| Revive a ghost | E | D-pad down |
| Pause | Esc | Start |

## How a run works

- Each floor has a boss. Learn its tells, roll through the hits, and hit it while its gold weak point shows.
- After a win, everyone drafts an ability. Abilities combine: try them together.
- Rest stops let you refill flasks and spend the gold you earned on upgrades and buffs.
- Friendly fire is on, but a friend's hit never takes you below 1 HP.
- If you go down you float as a ghost; a teammate can revive you. If everyone is down, the run ends.

## Troubleshooting

| Problem | Fix |
| --- | --- |
| "Version mismatch" | Everyone downloads the same release. |
| The room code does not work | Check for typos (it is 6 characters). Rooms close when the host leaves. |
| "A fight is in progress" | Wait for the fight to end, then join again. |
| Join times out | Check your connection and Windows Firewall (allow Boplborne on private and public networks). For Hamachi or direct IP see `docs/NETWORKING.md`. |
| Rubber-banding | The player with the best connection should host. |
| The screen is too dark or grainy | Settings → turn off **Film effects**. |

## Privacy

Online play uses Unity's anonymous sign-in for Relay and the public room list. It stores a random id on your PC and nothing personal. Your nickname (optional) is shown to the other players in your room.
