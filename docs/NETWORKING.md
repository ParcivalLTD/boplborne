# Networking

Grace Run uses **Netcode for GameObjects** with **Unity Transport**, connecting directly by IP. Friends reach each other over a **Hamachi** network, which makes their PCs act like they are on the same LAN. No Unity cloud services, relays or matchmaking.

## Ways to run the game

| Mode | Who | How |
| --- | --- | --- |
| **Host** | The friend hosting, who also plays | Main menu → Host. Runs the server and a player in one game. Recommended. |
| **Client** | Everyone else | Main menu → Join → type the host's Hamachi IPv4 → Join. |
| **Server only** | Optional, a PC that just runs the server | `GraceRun.exe -server -port 7777`. Add `-batchmode -nographics` for no window. A Windows Dedicated Server build profile works too. |

Command-line arguments (read by `CommandLineArgs` in `Bootstrap`): `-server`, `-port <n>`, `-connect <ip>` (client auto-join, handy for testing), `-statusfile <path>` (server only: a status file for monitoring). Running a server around the clock is covered in `docs/SERVER.md`.

## Who decides what (authority table)

Everyone is a friend, so we trust clients where it makes the game feel better.

| Thing | Decided by | Synced with |
| --- | --- | --- |
| Your own player's movement, jump, dodge | **Your PC** (owner) | `NetworkTransform` with owner authority, interpolation on for others |
| Whether your attack hit something | **Your PC** (attacker side) | `ReportHitRpc` to server |
| Whether a boss attack hit you | **Your PC** (victim side), only if you weren't invulnerable | `ReportHitRpc` to server |
| Which weapon a player carries | **Server** (the owner asks with `SelectRpc`, only accepted in the hub and at rest stops) | `NetworkVariable<byte>` on `PlayerWeapon` (catalog id) |
| HP, damage numbers, deaths, revives | **Server** | `NetworkVariable` on `Health` |
| Stamina | **Your PC** (it only gates your own actions) | `NetworkVariable` (owner write) so the HUD shows it |
| Knockback on a player | Server or attacker decides, **target's PC applies** | `[Rpc(SendTo.Owner)]` on the target |
| Boss AI, target, position | **Server** | `NetworkTransform` (server authority) + `CurrentAttack` NetworkVariable |
| Boss attack timing | **Server** picks attack id + start tick | Every client plays the telegraph from that tick |
| Ability cooldowns | Owner checks for instant feedback, **server** re-checks | `AbilityLoadout` NetworkList |
| Ability objects (walls, black holes, bombs) | **Server** spawns | NetworkObject |
| Combo results | **Server** | Effect components |
| Run state, floor, currency, draft cards and picks | **Server** | NetworkVariables / NetworkLists |
| Random numbers | **Server** seed per run | Sent to clients when they need it |
| Particles, sounds, shake, hitstop | **Every PC locally** | Triggered from synced state changes |
| Camera position, zoom, off-screen markers | **Every PC locally** | Read from synced player, gravestone and boss state; nothing is sent |

## Connection rules

- Max **4** players. Connection approval refuses the 5th.
- The client sends its **game version** in the connection payload. A mismatch is refused with a clear message: "Host is on version X, you are on Y."
- **Joining mid-fight is refused** ("Wait for the fight to end"). Joining in the hub or a draft is allowed.
- If a client disconnects, its player despawns and its slot opens.
- If the host leaves, everyone returns to the main menu with "Host left the game."
- Default port **7777 (UDP)**.

## Testing on one PC

1. **Multiplayer Play Mode** (Window → Multiplayer → Multiplayer Play Mode): activate up to 3 extra virtual players (4 total). One clicks Host, the others Join `127.0.0.1`.
2. **Fake lag:** use the Network Simulator from the Multiplayer Tools package. Test with about 100 ms latency and a little packet loss; Hamachi between friends is usually in this range or better.
3. Every networked step in `docs/PROMPTS.md` lists exactly what to test this way.

## Playing with friends over Hamachi

### Once

1. **Everyone** installs LogMeIn Hamachi and signs in. The free plan allows up to 5 members per network.
2. **The host** creates a network (Network → Create a new network), with a name and password.
3. **Friends** join it (Network → Join an existing network) with that name and password.

### Every session

1. Everyone turns Hamachi on and checks they see each other online (green dot).
2. The host starts Grace Run and clicks **Host**.
3. The host reads their **Hamachi IPv4 address** at the top of the Hamachi window and shares it.
4. Friends start Grace Run, click **Join**, enter that address, and join.
5. Everyone must run the **same version** of the game.

### The first time on each PC

Windows Firewall asks whether to allow Grace Run. Allow it on **both private and public networks**: Windows often classifies the Hamachi adapter as public.

### Troubleshooting

| Problem | Fix |
| --- | --- |
| "Version mismatch" | Everyone downloads the same release from GitHub. |
| Join times out | Ping the host's Hamachi IP from a terminal (`ping 25.x.x.x`). No reply: Hamachi problem (both online? same network?). Reply but no join: firewall on the host, allow Grace Run on public networks. |
| Works for some friends, not others | Hamachi shows a yellow triangle or "relayed" connection for them: restart Hamachi on both PCs. |
| Port already in use | Another program uses 7777. Host with `-port 7778` and tell friends (Join field accepts `ip:port`). |
| Rubber-banding | Check Hamachi latency (hover over the friend). Over 150 ms, the friend with the best connection should host. |
