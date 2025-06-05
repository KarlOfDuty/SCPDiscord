% scpdiscord(1) | User Commands
%
% "2025-05-25"

# NAME

scpdiscord - A very customisable Discord bot + SCP:SL plugin combo

# SYNOPSIS

**scpdiscord** [**-c** *CONFIG*] [**--leave**=*ID,ID,...*]  
**scpdiscord** [**-h** | **--help**]  
**scpdiscord** [**-v** | **--version**]

# DESCRIPTION

**SCPDiscord** is a very customisable Discord bot + SCP:SL plugin combo
which lets you monitor and manage your SCP:SL servers from Discord.

# OPTIONS

**-c**, **--config**=*PATH*
:   Specify an alternative configuration file to use.

**-l**, **--log-file**=*PATH*
:   Log console output to a file. Logs all log messages regardless of log level in the config.

**--leave**=*ID[,ID...]*
:   Make the bot leave specific Discord servers using their server IDs.

**-h**, **--help**
:   Show help message and exit.

**-v**, **--version**
:   Show version information and exit.

# CONFIGURATION

The bot configuration uses the YAML format (<https://yaml.org>).

A default configuration file is created automatically if none exists when the bot starts.

It is fully commented and includes documentation for each configuration option.

# FILES

*/etc/scpdiscord/config.yml*
:   System-wide configuration file, used when running as a service.

*/var/log/scpdiscord*
:   System-wide log directory, used when running as a service.

# EXIT STATUS

- **0** Success

- **1** Error

# EXAMPLES

Start the bot with the configuration file in the current working directory:

    scpdiscord

Specify a custom configuration file and log directory:

    scpdiscord --config /path/to/config.yml --log-file /path/to/log-file

Run the bot as the scpdiscord user using default system paths:

    sudo -u "scpdiscord" scpdiscord --config /etc/scpdiscord/config.yml --log-file-path /var/log/scpdiscord

Start the bot using the included systemd service:

    sudo systemctl start scpdiscord

Make the bot leave specific servers:

    scpdiscord --leave 123456789012345678,987654321098765432

# COPYRIGHT

Copyright © 2025 Karl Essinger

This software is licensed under the GNU General Public License version 3.0 (GPLv3).

On Debian systems, the complete text of the GNU General Public License v3.0
can be found in /usr/share/common-licenses/GPL-3.

Otherwise, see <https://www.gnu.org/licenses/gpl-3.0.html>.

# BUGS

Report bugs at the project's issue tracker:  
<https://github.com/KarlOfDuty/SCPDiscord/issues>

# AUTHOR

Karl Essinger <xkaess22@gmail.com>