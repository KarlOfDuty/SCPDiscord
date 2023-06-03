# SCPDiscord [![Build Status](https://jenkins.karlofduty.com/job/NWAPI/job/SCPDiscord/job/master/badge/icon)](https://jenkins.karlofduty.com/blue/organizations/jenkins/NWAPI%2FSCPDiscord/activity) [![Release](https://img.shields.io/github/release/KarlofDuty/SCPDiscord.svg)](https://github.com/KarlOfDuty/SCPDiscord/releases) [![Downloads](https://img.shields.io/github/downloads/KarlOfDuty/SCPDiscord/total.svg)](https://github.com/KarlOfDuty/SCPDiscord/releases) [![Discord Server](https://img.shields.io/discord/430468637183442945.svg?label=discord)](https://discord.gg/C5qMvkj) [![Codacy Badge](https://app.codacy.com/project/badge/Grade/8144e5bff03c4912b08fd189b4b7f668)](https://www.codacy.com/manual/xkaess22/SCPDiscord?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=KarlOfDuty/SCPDiscord&amp;utm_campaign=Badge_Grade)

SCPDiscord links features from your SCP:SL server console to Discord channels, such as printing out server events to Discord and using server commands through Discord. It is incredibly customisable as you can read more about below.

## Features

* Ability to log any Northwood API event to Discord. All are optional and can be toggled individually.

* Complete multi-channel support letting you post different events to different channels. All can be individually re-routed.

* Support for MultiAdmin managed servers, each sub-server can post to the same channel, different channels or even different Discord servers.

* You can for instance have one public channel for each of your servers where things like player joins, kills, round starts and round ends are posted. 
You could then add one channel for each server visible only to moderators, showing things like admin actions and logging who attacks who on each server to check for teamkillers.

* Ability to run server commands from discord channels, with command permissions limited to specific roles of your choice.

* The ability to completely customise every single message from the plugin and use different languages. More info [here](docs/Languages.md).

* Player count is displayed in the bot activity field. The bot's status changes from red when the scp server is off but the bot server is on, yellow when there are no players on a server and green when a server has players.

* You can sync ranks from discord to the game, letting you automate things like supporter rewards and moderator positions.

* You can also grant reserved slots automatically for specific discord roles, and automatically remove them if they lose their role.

## Download

You can download the latest release [here](https://github.com/KarlOfDuty/SCPDiscord/releases) or the latest dev build [here](https://jenkins.karlofduty.com/blue/organizations/jenkins/NWAPI%2FSCPDiscord/activity);

## Guides

- [Installation and configuration](docs/Installation.md)

- [Editing messages and languages](docs/Languages.md)

- [Commands](docs/Commands.md)
