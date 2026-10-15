---
name: explain
description: Explain part of the project in plain language
argument-hint: <file or system>
disable-model-invocation: true
---

Explain part of the project to the user, who is learning Unity. The file or system: $ARGUMENTS

1. Say in two sentences what it does in the game.
2. Walk through how it works in plain language, following the flow of one real example (e.g. "you press Dodge, then...").
3. Point out where its tunable values live and what changing them does.
4. Mention which machine runs each part (server, owner, every client) if it is networked.
5. Keep it short. No code changes.
