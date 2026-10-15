---
paths:
  - "Assets/_Project/Scripts/**"
---

# Networking authority (Netcode for GameObjects)

Before writing gameplay code, check who owns it in the authority table of `docs/NETWORKING.md`.

- Server owns: HP, damage results, deaths and revives, boss AI and position, ability object spawns, the draft, run state, currency, randomness (one seed from the server).
- Owner client owns: its own player movement (owner-auth NetworkTransform), its own attack hit detection, and detection of boss hits on itself.
- Knockback on another player: send it to that player's owner with an `[Rpc(SendTo.Owner)]` call on the target; the owner applies the impulse.
- Boss attacks: the server picks the attack and broadcasts `attackId` plus the server tick it starts. Every client plays the telegraph from that tick, so all four screens agree.
- Cosmetics (particles, screen shake, hitstop, sounds) run locally and never wait for the network.
- Everything must work in three modes: Host, Client, Server-only. Every report says how to check it with Multiplayer Play Mode.
- Max 4 players. Joining mid-fight is refused; joining in the hub or a draft is allowed.
