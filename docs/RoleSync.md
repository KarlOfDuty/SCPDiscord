# RoleSync Setup

SCPDiscord features a system called RoleSync which checks users Discord roles and sets their in-game rank and reserved slot automatically.

It is also able to automatically run commands when the players join.

In order for the plugin to determine what a user's Discord account is, they must have run the `/syncid` command in Discord first.

If a player is synced the plugin will then ask the bot for all the player's role-IDs when they join.

If any of them match the ones you have specified, the actions you specified on it will be executed.

> [!TIP]
> In this guide a `role` refers to a Discord role and a `rank` refers to an in-game rank.

## Configuration Examples

For those who just want an easy setup you can look at these quick examples.

For these examples we have a Discord server with the following roles and their IDs:
```yaml
admin:     111111111111111111
moderator: 222222222222222222
staff:     333333333333333333
```

> [!IMPORTANT]
> The IDs can match both role and user IDs, and if you set the ID to 0 or don't include any IDs it will match all players.

**Example 1: Users must have ALL of the roles "admin, moderator, staff" to get an ingame rank. As long as they have the staff role they get a reserved slot.**
```yaml
rolesync:
  staff-role:
    ids: [ 333333333333333333 ]
    reserved-slot: true
    sub-roles:
      moderator-role:
        ids: [ 222222222222222222 ]
        sub-roles:
          admin-role:
            ids: [ 111111111111111111 ]
            remoteadmin-rank: "admin"
            commands:
                - "/pbc <var:player-id> 3 You have all three roles!"
```

**Example 2: If the user has ONE OR MORE of the roles, they get a reserved slot.**
```yaml
rolesync:
  any-staff-role:
    ids: [ 333333333333333333, 222222222222222222, 111111111111111111 ]
    commands:
        - "/pbc <var:player-id> 3 You have at least one of the staff roles!"
```

**Example 3: If the user is an admin they get the admin rank, otherwise if they are a moderator they get the moderator rank. If they are both they will get the admin rank as the first one is always picked and only one rank can be picked.**
> [!TIP]
> If you have used RoleSync in SCPDiscord before 3.4.0, this example will give you the exact same behaviour.
> 
> You should update to the new format and remove the `/grantvanillarank` and `/grantreservedslot` commands, replacing them with the new `remoteadmin-rank` and `reserved-slot` options.
```yaml
rolesync:
  admin-role:
    ids: [ 111111111111111111 ]
    remoteadmin-rank: "admin"
    commands:
      - "/pbc <var:player-id> 3 You are an admin!"
  moderator-role:
    ids: [ 222222222222222222 ]
    remoteadmin-rank: "moderator"
    commands:
      - "/pbc <var:player-id> 3 You are a moderator!"
```

**Example 4: Same as Example 3 but the users must also have the staff role regardless if they are a moderator or admin.**
```yaml
rolesync:
  staff-role:
    ids: [ 333333333333333333 ]
    sub-roles:
      admin-role:
        ids: [ 111111111111111111 ]
        remoteadmin-rank: "admin"
        commands:
          - "/pbc <var:player-id> 3 You are an admin!"
      moderator-role:
        ids: [ 222222222222222222 ]
        remoteadmin-rank: "moderator"
        commands:
          - "/pbc <var:player-id> 3 You are a moderator!"
```

## Configuration Specification

Each RoleSync config role can have the following parameters:
```yaml
role-name:
  ids: []
  reserved-slot: true
  remoteadmin-rank: "rankname"
  commands:
    - "command 1"
    - "command 2"
  sub-roles:
    # you can place more roles here that only trigger if the user has the above role...

# you can place more roles here that only trigger if the user does NOT have the above role...
```
Any role listed as another role's requires that the user also has its parent role (See Example 1).

Any role placed after another role on the same level will only be checked if the user does not have the role listed before it (See Example 3).

They do not have to have all of the parameters but must have at least one.

| Parameter          | Description                                                                                                                                         |
|--------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------|
| `role-name`        | This can be anything, this name is only used in logs.                                                                                               |
| `ids`              | This is a list of role or user IDs. The player must have at least one of these to be considered a member of the role.                               |
| `reserved-slot`    | If set to true the player will get a reserved slot when they join. If this is not set the player's reserved slot will be removed on join.           |
| `remoteadmin-rank` | Set this to the name of a rank in your `config_remoteadmin.txt` and the plugin will give the player this rank when they join.                       |
| `commands`         | A list of commands to run when a player joins. Variables in the commands are supported to for instance include the player's user ID in the command. |
| `sub-roles`        | Roles that will only be checked if the user has the parent role.                                                                                    |

### Available command variables

These variables are available in the RoleSync command configuration.

You can add them to a command with the syntax `<var:variable>`. For example: `/pbc <var:player-id> 3 Hello!` sends a hello message to the joining player.

| Variable              | Sample               | Description                                                                    |
|-----------------------|----------------------|--------------------------------------------------------------------------------|
| `player-userid`       | `76561198022373616`  | The player's Steam ID.                                                         |
| `player-id`           | `14`                 | The player's current player ID.<br><br>This is different every time they join. |
| `player-name`         | `Karl of Duty `      | The player's Steam display name.                                               |
| `player-ipaddress`    | `192.168.2.150`      | The player's IP-address (mainly for non-steam servers).                        |
| `discord-displayname` | `Karl of Duty`       | The player's nickname in the Discord server specified in the bot config.       |
| `discord-username`    | `karlofduty`         | The player's Discord username.                                                 |
| `discord-userid`      | `170904988724232192` | The player's Discord user ID.                                                  |
