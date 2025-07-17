# Plugin Setup

## 1. Installing the plugin

### Option 1 (recommended): Install using the localadmin package manager

> [!Caution]
> The LocalAdmin package manager doesn't work since the switch to LabAPI. You cannot currently use this method as it installs the plugin to the wrong directory.

~Use `p install KarlOfDuty/SCPDiscord` in the server console and restart your server.~

### Option 2: Manual installation

Download SCPDiscord, either a [release](https://github.com/KarlOfDuty/SCPDiscord/releases) or [dev build](https://jenkins.karlofduty.com/blue/organizations/jenkins/SCPDiscord/activity/).
Place the plugin and dependencies directory in the following directories:
```
.config/
    SCP Secret Laboratory/
        LabAPI/
            plugins/
                <port>/
                    SCPDiscord.dll
            dependencies/
                <port>/
                    Google.Protobuf.dll
                    Newtonsoft.Json.dll
                    System.Memory.dll
```

## 2. Set up the plugin config

The plugin config is automatically created for you the first time you run the plugin. The path is printed in the server console on startup so you know for sure where it is.

Open the plugin config and edit it as you wish, then restart the server. In the future you can also use the `scpd reload` command, but that requures that the plugin started up correctly first.

[You can click here to view default config](https://github.com/KarlOfDuty/SCPDiscord/blob/main/SCPDiscordPlugin/config.yml)

> [!IMPORTANT]
> Keeping the bot and plugin on different devices is not recommended but is possible, you will have to deal with the issues this may result in yourself if you choose to do so.
Simply change the bot ip in the plugin config to correspond with the other device. Make sure to read the security information on the main installation page as this is unsafe.
