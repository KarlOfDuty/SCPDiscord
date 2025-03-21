# Plugin Setup

## 1. Installing the plugin

#### Option 1 (recommended): Install using the localadmin package manager

Use `p install KarlOfDuty/SCPDiscord` in the server console and restart your server.

#### Option 2: Manual installation

Download SCPDiscord, either a [release](https://github.com/KarlOfDuty/SCPDiscord/releases) or [dev build](https://jenkins.karlofduty.com/blue/organizations/jenkins/CI%2FSCPDiscord/activity/).
Place the plugin library and dependencies directory in the `~/.config/SCP Secret Laboratory/PluginAPI/plugins/<port>` directory:
```
plugins/
    <port>/
        dependencies/
            Google.Protobuf.dll
            Newtonsoft.Json.dll
            System.Memory.dll
        SCPDiscord.dll
```

## 2. Plugin config

The plugin config is automatically created for you the first time you run the plugin. The path is printed in the server console on startup so you know for sure where it is.

[Click here to view default config](https://github.com/KarlOfDuty/SCPDiscord/blob/master/SCPDiscordPlugin/config.yml)

> [!IMPORTANT]
> Keeping the bot and plugin on different devices is not recommended but is possible, you will have to deal with the issues this may result in yourself if you choose to do so.
Simply change the bot ip in the plugin config to correspond with the other device. Make sure to read the security information on the main installation page as this is unsafe.