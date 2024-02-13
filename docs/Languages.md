# Switching to another included language

There are several languages included in the plugin dll. The current languages are:
* English (default)
* Ukrainian
* Russian
* Chinese (Simplified)
* Italian

You can choose which language you want to use by setting the config entry `settings.language` to the name of the language file in all lower-case. You can find the language files in `SCPDiscord/Languages` in your plugin directory.

# Overriding specific messages

Use the `overrides.yml` file to override specific entries from the normal language files.

Here is an example where the player report language entry has been copied from `english.yml` to `overrides.yml` and changed to add a discord mention to a staff member:

```yaml
  onplayerreport:
    # player - Player
    # target - Player
    # reason - string
    message: "**<var:player-name> (<var:player-userid>) reported <var:target-name> (<var:target-userid>) for breaking server rules:**\n```<var:reason>``` <@170904988724232192>"
    regex: []
    cancel_regex: []
```

This overrides only the bot status and uses the language file for everything else. You can keep the regenerate language files config option on as the overrides file is not affected by it. This makes sure your language files don't get outdated when you update the plugin.

There is also an `emote-overrides.yml` file which works the exact same way but for the `emotes.yml` file.

If you want to find out more about how the language files work you can read the contribution guide below.

# Editing or adding a new language (usually for contributing to the plugin)

**Make sure the file encoding is UTF-8, other encodings like UTF-8 BOM do not work.**

In notepad++:

![](img/nppNewlines.png)

Also make sure to turn off language regeneration in the config or your changes will be overwritten!

If you are making a new language you can copy the english.yml file and name the new file something in all lowercase. Otherwise you can just edit the english file directly as long as you have turned off overwriting the language files in the config. Below is a more in depth guide, but you can just edit the existing words instead if you wish.

If you want to you can submit your language file to the default languages by either making a pull request here on GitHub or by sending it to me either in an issue here or in Discord.

### Messages
These are the messages that are sent to Discord, you can add variables to them with the <var:name> syntax. If you for instance want to add an IP-address to the OnConnect event message you can put `"Player is connecting with IP address <var:ipaddress>."` which will become something like `Player is connecting with IP address 127.0.0.1.` in Discord.

All available variables are listed as comments on each event.

Player variables are a bit special. If a message lists one or more player variables like this:
```yaml
# attacker - Player
# target - Player
```
it means each of these contain a number of standard variables like `attacker-name`, `attacker-userid`, `attacker-role` and so on (same for the target player).

The top of the language file should have a list of all available player variables, but if you want to be sure you can also check the list in [APIExtensions.cs](../SCPDiscordPlugin/APIExtensions.cs).

### Regex
This will not be necessary for most users.

Aside from the messages themselves, the language file contains several different regex options. You can add any number of regular expression to each entry by separating them with a comma, check the `english.yml` file for examples of this.

If you are unfamiliar with regex, don't worry, you can basically think of it as replacing the left side with what is on the right. Regex does support much more advanced syntax and if it does not behave the way you expect it to you may have accidentally used it.

If you are interested in learning how regex works use this link to start learning some simple patterns: [RegExr](https://regexr.com/)

### Cancel Regex
These are regex patterns where the entire message will get cancelled if there is a match.

### Order of operations

As this config is basically just doing lots of replacements on the same message before sending it, the order of operations may matter to you.

The replacements are executed in the following order:

1. The unparsed message is read from the config.
2. All variables are added to the message except names
3. The `global_regex` replacements are executed, I use this to fix names of variables such as classes and items.
4. The message specific `regex` is executed, I use this for simple logic replacements such as:
```yaml
# This replaces isopen:<var:open> with close if the door is open
# and with open if the door is closed.
message: "<var:player-name> (<var:player-userid>) tried to isopen:<var:open> a locked door."
regex: ["isopen:True":"close", "isopen:False":"open"]
```
5. The message specific `cancel_regex` matching is performed and the message is cancelled if any of them match the message contents.
6. The variables representing names are added in, this is to make sure players don't use names with words that get accidentally or deliberately replaced by your above regex replacements.
7. The `user_regex` is executed in order to remove forbidden parts from names, this only affects user names.
8. The `final_regex` is executed after everything else and applies to the entire message.