# Building SCPDiscord

## Dependencies

- dotnet-sdk (9.0)
- protobuf

## Building the plugin and bot

### Via Rider or Visual Studio

Open the SCPDiscord solution and build using the controls in the IDE.

### Manually

Enter the SCPDiscord plugin directory and use the following command in order to build the plugin:
```bash
dotnet build --output ../bin/plugin
```

Enter the SCPDiscord bot directory and use the following commands:
```bash
dotnet publish -r win-x64 -c Release --output ../bin/win-bot
dotnet publish -r linux-x64 -c Release --output ../bin/linux-bot
```

The plugin and bot should now be built in the `bin` directory in the root of the repo.

## Generating the network interface

> [!NOTE]
> This section is only needed if you need to edit the network traffic between the plugin and bot.

The bot and plugin communicate using protobuf messages. These messages are constructed from protobuf schemas located in the schema directory which are then generated into the bot and plugin's interface directories.

If you edit the schema files you need to run the following command in the schema directory to generate the interface classes:
```bash
protoc --csharp_out "../SCPDiscordBot/Interface" --csharp_out "../SCPDiscordPlugin/Interface" --proto_path . *.proto ./BotToPlugin/*.proto ./PluginToBot/*.proto
```
