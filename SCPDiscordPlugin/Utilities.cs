using System;
using System.Collections.Generic;
using System.Linq;
using PluginAPI.Core;

namespace SCPDiscord
{
    public static class Utilities
	{
        public static string SecondsToCompoundTime(long seconds)
        {
            if (seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds));
            if (seconds == 0) return "0 sec";

            TimeSpan span = TimeSpan.FromSeconds(seconds);
            int[] parts = { span.Days / 365, span.Days % 365 / 31,  span.Days % 365 % 31, span.Hours, span.Minutes, span.Seconds };
            string[] units = { " year", " month", " day", " hour", " minute", " second" };

            return string.Join(", ",
                from index in Enumerable.Range(0, units.Length)
                where parts[index] > 0
                select parts[index] + (parts[index] == 1 ? units[index] : units[index] + "s"));
        }

		public static string TicksToCompoundTime(long ticks)
		{
			return SecondsToCompoundTime(ticks / TimeSpan.TicksPerSecond);
		}

		public static string GetParsedUserID(string userID)
		{
			if (!string.IsNullOrWhiteSpace(userID))
			{
				int charLocation = userID.LastIndexOf('@');

				if (charLocation > 0)
				{
					return userID.Substring(0, charLocation);
				}
			}
			return null;
		}

		public static bool GetPlayerName(string steamID, ref string name)
		{
			foreach (Player player in Player.GetPlayers<Player>())
			{
				if (player.GetParsedUserID() == steamID)
				{
					name = player.Nickname;
					return true;
				}
			}
			return false;
		}

		public static bool KickPlayer(string steamID, string message = "Kicked from server")
		{
			foreach (Player player in Player.GetPlayers<Player>())
			{
				if (player.GetParsedUserID() == steamID)
				{
					player.Ban(message, 0);
					return true;
				}
			}
			return false;
		}

		public static bool IsPossibleSteamID(string steamID)
		{
			return steamID.Length >= 17 && ulong.TryParse(steamID.Replace("@steam", ""), out ulong _);
		}

		public static string EscapeDiscordFormatting(string input)
		{
			input = input.Replace("`", "\\`");
			input = input.Replace("*", "\\*");
			input = input.Replace("_", "\\_");
			input = input.Replace("~", "\\~");
			return input;
		}

		public static LinkedList<string> ParseListIntoMessages(List<string> listItems)
		{
			LinkedList<string> messages = new LinkedList<string>();
			foreach (string listItem in listItems)
			{
				if (messages.Last?.Value?.Length + listItem?.Length < 2048)
				{
					messages.Last.Value += listItem + "\n";
				}
				else
				{
					messages.AddLast(listItem?.Trim());
				}
			}

			return messages;
		}

		public static DateTime ParseCompoundDuration(string duration, ref string humanReadableDuration, ref long durationSeconds)
		{
			//Check if the amount is a number
			if (!int.TryParse(new string(duration.Where(char.IsDigit).ToArray()), out int amount))
			{
				return DateTime.MinValue;
			}

			char unit = duration.Where(char.IsLetter).ToArray()[0];
			TimeSpan timeSpanDuration = new TimeSpan();

			// Parse time into a TimeSpan duration and string
			if (unit == 's')
			{
				humanReadableDuration = amount + " second";
				timeSpanDuration = new TimeSpan(0, 0, 0, amount);
			}
			else if (unit == 'm')
			{
				humanReadableDuration = amount + " minute";
				timeSpanDuration = new TimeSpan(0, 0, amount, 0);
			}
			else if (unit == 'h')
			{
				humanReadableDuration = amount + " hour";
				timeSpanDuration = new TimeSpan(0, amount, 0, 0);
			}
			else if (unit == 'd')
			{
				humanReadableDuration = amount + " day";
				timeSpanDuration = new TimeSpan(amount, 0, 0, 0);
			}
			else if (unit == 'w')
			{
				humanReadableDuration = amount + " week";
				timeSpanDuration = new TimeSpan(7 * amount, 0, 0, 0);
			}
			else if (unit == 'M')
			{
				humanReadableDuration = amount + " month";
				timeSpanDuration = new TimeSpan(30 * amount, 0, 0, 0);
			}
			else if (unit == 'y')
			{
				humanReadableDuration = amount + " year";
				timeSpanDuration = new TimeSpan(365 * amount, 0, 0, 0);
			}

			// Pluralize string if needed
			if (amount != 1)
			{
				humanReadableDuration += 's';
			}

			durationSeconds = (long)timeSpanDuration.TotalSeconds;
			return DateTime.UtcNow.Add(timeSpanDuration);
		}
	}
}
