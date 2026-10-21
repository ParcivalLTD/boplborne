# Running a dedicated server

A dedicated server is the normal game started with `-server`: no window, no local player, just the server. Friends join it by IP at any time, so it can run around the clock without anyone having to host.

## What it looks after by itself

- **Frame rate:** capped (60 by default) so a headless build does not eat a whole CPU core.
- **Empty server:** when nobody has been connected for 30 seconds, an unfinished run is dropped and the server goes back to the hub, so the next player starts a fresh run. A quick reconnect inside that time keeps the run. Joining mid-fight is refused as usual.
- **Log:** one `[Server]` line per join and leave, and a status line every 5 minutes: `[Server] players 2/4, Fighting, floor 3, up 1h 02m`.
- **Status file:** with `-statusfile <path>` a small JSON file is rewritten every 15 seconds (players, phase, floor, uptime, last update time). A monitor only needs to check that `Updated` is recent.
- **Restarts:** `tools/run-server.ps1` starts the server again whenever it exits and writes a log per run.

All numbers (grace time, status interval, frame rate) are in `Data/Config/ServerData.asset`.

## Run it

1. Build the game (**Grace Run → Build Windows**) and copy the build folder to the server PC.
2. From the repo (or copy `tools/run-server.ps1` next to `Boplborne.exe`):

```powershell
.\tools\run-server.ps1 -Exe C:\Boplborne\Boplborne.exe -Port 7777
```

Logs go to `server-logs\` next to the script: `server-<time>.log` per run (the newest 20 are kept), `supervisor.log` for starts and exits, and `status.json`. Stop with Ctrl+C.

Without the script, the plain command is:

```powershell
Boplborne.exe -batchmode -nographics -server -port 7777 -logFile server.log -statusfile status.json
```

A server that exits within 20 seconds five times in a row waits two minutes before trying again, so a broken build does not spin.

## Let friends reach it

Players join with `-connect` or the **Join game** screen, using the server's address, and everyone must run the **same game version** as the server (a mismatch is refused with a message).

| Where the server is | What to do |
| --- | --- |
| Same Hamachi network as your friends | Nothing to open. Friends join the server PC's Hamachi IP. |
| A PC at home | Allow **UDP port 7777** (the `-port` you chose) in Windows Firewall and forward it on your router to the server PC. Friends join your public IP. |
| A rented Windows VPS | Allow the same UDP port in Windows Firewall and in the provider's firewall or security group. |

Windows Firewall rule (run once, as administrator):

```powershell
New-NetFirewallRule -DisplayName "Boplborne server" -Direction Inbound -Protocol UDP -LocalPort 7777 -Action Allow
```

## Start it at boot

Use Task Scheduler so it runs without anyone logged in:

1. **Create Task**, tick **Run whether user is logged on or not** and **Run with highest privileges**.
2. Trigger: **At startup**.
3. Action: program `powershell.exe`, arguments `-NoProfile -ExecutionPolicy Bypass -File C:\Boplborne\tools\run-server.ps1 -Exe C:\Boplborne\Boplborne.exe`.
4. Settings: untick **Stop the task if it runs longer than**.

## Limits

- Up to 4 players, one run at a time. There is no password: anyone who knows the address and has the same version can join. Do not publish the address if you want a private group.
- Updating the game means copying the new build and restarting the script. Connected players are dropped.
- The server was checked here with a headless build joining and leaving, and with an abandoned run being reset. A long unattended run (days) has not been tried yet; watch `supervisor.log` and the status file for the first week.
