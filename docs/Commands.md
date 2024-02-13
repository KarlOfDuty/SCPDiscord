# Bot commands

## Bot commands

| Command                               | Description                                                                    |
|---------------------------------------|--------------------------------------------------------------------------------|
| `/ban <steamid> <duration> <reason>`  | Bans a player from the server.                                                 |
| `/help`                               | Shows a message with useful information.                                       |
| `/kick <steamid> (reason)`            | Kicks a player from the server.                                                |
| `/kickall (reason)`                   | Kicks all players from the server with a message, useful for server shutdowns. |
| `/list`                               | Lists all online players.                                                      |
| `/listsynced (listoffline)`           | Lists all synced players, includes offline players if set to true.             |
| `/mute <steamid> <duration> <reason>` | Mutes a player.                                                                |
| `/playerinfo <steamid>`               | Shows a short list of player information.                                      |
| `/ra <command>`                       | Runs a remote admin command.                                                   |
| `/server <command>`                   | Runs a server command.                                                         |
| `/syncid <steamid>`                   | Syncs your Discord role to the game.                                           |
| `/syncip <ip>`                        | Syncs your Discord role to the game. (For servers not using steam)             |
| `/unban <steamid/ip>`                 | Unbans a player from the server.                                               |
| `/unmute <steamid>`                   | Unmutes a player.                                                              |
| `/unsync`                             | Unsyncs your Discord account.                                                  |
| `/unsyncplayer <user>`                | Unsyncs a Discord account.                                                     |

## Plugin console commands

Base command:
`scpdiscord <subcommand>` or `scpd <subcommand>`

| Subcommand                                                                             | Description                                                                                 |
|----------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------|
| `debug`                                                                                | Toggles debug console messages.                                                             |
| `grantreservedslot <steamid>`<br/>`grs <steamid>`                                      | Gives a player a reserved slot on the server.                                               |
| `grantvanillarank <steamid/playerid> <rank>`<br/>`gvr <steamid/playerid> <rank>`       | Gives a player a vanilla rank for their current session.                                    |
| `reconnect`'<br/>`rc`                                                                  | Reconnects to the bot.                                                                      |
| `reload`                                                                               | Reloads the plugin, all configs and files and reconnects.                                   |
| `removereservedslot <steamid>`<br/>`rrs <steamid>`                                     | Removes a reserved slot from a player.                                                      |
| `setnickname <player id/steamid> <nickname>`<br/>`nick <player id/steamid> <nickname>` | Sets a player's nickname, useful for the rolesync system if you want to sync discord names. |
| `unsync <discordid>`                                                                   | Manually remove a player from being synced to discord.                                      |
| `validate`                                                                             | Creates a config and language validation report in the console.                             |

## Time

Time is expressed in the format `NumberUnit` where unit is a unit of time and number is the amount of that time unit, for example `6M` represents six months.

Valid time units:

|  Letter   |   Unit    |
|:---------:|:---------:|
|    `s`    |  Seconds  |
|    `m`    |  Minutes  |
|    `h`    |   Hours   |
|    `d`    |   Days    |
|    `w`    |   Weeks   |
|    `M`    |  Months   |
|    `y`    |   Years   |
