# Installation

### 1. [Register a bot in Discord](RegisterBotApplication.md)
### 2. [Set up the bot application](SetUpBotApplication.md)
### 3. [Set up the plugin](SetUpPlugin.md)

## Support

If you have followed the above steps you should now have a working Discord bot, otherwise contact me in Discord.

## Security information

> [!WARNING]
> **The bot has no authorization of incoming connections**, this means you cannot allow the plugin's port through your firewall or anyone will be able to send fake messages to it.
> 
> If you really need to run the SCP:SL server on one system and the bot on another connected over the internet you should only allow connections from the SCP:SL server's IP.
> 
> Here is an example for systems using UFW:
> ```bash
> sudo ufw allow from 111.111.111.111 to any port 8888
> ```
> 
> This allows only the IP `111.111.111.111` to connect to the bot on port `8888` as long as your default setting is to deny all, which it is by default.

> [!WARNING]
> If you ever reveal the bot token to anyone or post it somewhere where it can be read from the internet you need to reset it immediately.
> It gives others full access to the bot's Discord account so they can use it to do whatever they want with your Discord server, including deleting it.
> 
> It is also highly recommended to make sure the bot only has the necessary Discord permissions and no more in order to limit the damage if you accidentally post your bot token somewhere.
