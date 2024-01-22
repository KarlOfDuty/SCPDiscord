# Building SCPDiscord

## Dependencies
- dotnet-sdk (8.0)
- mono
- protobuf

## Building the plugin

### Via Rider or Visual Studio
Load the SCPDiscordPlugin project and build using the controls in the IDE.

Note: Rider doesn't support mixing mono and dotnet projects in the same solution so you have to open the projects individually, not the solution.

### Manually
Enter the SCPDiscord plugin directory and use the following command in order to build the plugin:
```bash
msbuild SCPDiscordPlugin.csproj -restore -p:PostBuildEvent=
```

## Building the bot

### Via Rider or Visual Studio
Load the SCPDiscordPlugin project and build using the controls in the IDE.

### Manually
Enter the SCPDiscord bot directory and use the following commands:
```bash
dotnet build --output bin/linux-x64 --configuration Release --runtime linux-x64
dotnet build --output bin/win-x64 --configuration Release --runtime win-x64
```

## Generating the network interface

The bot and plugin communicate using protobuf messages. These messages are constructed from protobuf schemas located in the schema directory which are then generated into the bot and plugin's interface directories.

If you edit the schema files you need to run the following command in the schema directory to generate the interface classes:
```bash
protoc --csharp_out "../SCPDiscordBot/Interface" --csharp_out "../SCPDiscordPlugin/Interface" --proto_path . *.proto ./BotToPlugin/*.proto ./PluginToBot/*.proto
```
