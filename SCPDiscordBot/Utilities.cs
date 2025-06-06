using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SCPDiscord.Interface;

namespace SCPDiscord;

public static class Extensions
{
  private static readonly DateTimeOffset UnixEpoch =
    new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

  public static long ToUnixTimeMicroseconds(this DateTimeOffset timestamp)
  {
    TimeSpan duration = timestamp - UnixEpoch;
    // There are 10 ticks per microsecond.
    return duration.Ticks / 10;
  }
}

public static class Utilities
{
  public static DiscordEmbed GetDiscordEmbed(EmbedMessage embedMessage)
  {
    DiscordEmbedBuilder embed = new DiscordEmbedBuilder
    {
      Color = GetDiscordColour(embedMessage.Colour),
      Description = embedMessage.Description
    };

    if (embedMessage.HasTitle)
    {
      embed.WithTitle(embedMessage.Title);
    }

    if (embedMessage.HasUrl)
    {
      embed.WithUrl(embedMessage.Url);
    }

    if (embedMessage.HasTimestamp)
    {
      embed.WithTimestamp(DateTimeOffset.FromUnixTimeSeconds(embedMessage.Timestamp));
    }

    if (embedMessage.Footer != null)
    {
      embed.WithFooter(embedMessage.Footer.Text, embedMessage.Footer.IconURL);
    }

    if (embedMessage.HasImageURL)
    {
      embed.WithImageUrl(embedMessage.ImageURL);
    }

    if (embedMessage.Thumbnail != null)
    {
      embed.WithThumbnail(embedMessage.Thumbnail.Url, embedMessage.Thumbnail.Height, embedMessage.Thumbnail.Width);
    }

    if (embedMessage.Author != null)
    {
      embed.WithAuthor(embedMessage.Author.Name, embedMessage.Author.Url, embedMessage.Author.IconURL);
    }

    foreach (EmbedMessage.Types.DiscordEmbedField embedField in embedMessage.Fields)
    {
      embed.AddField(embedField.Name, embedField.Value, embedField.Inline);
    }

    return embed;
  }

  public static List<Page> GetPaginatedMessage(PaginatedMessage message)
  {
    List<Page> listPages = new List<Page>();
    foreach (EmbedMessage embed in message.Pages)
    {
      listPages.Add(new Page("", new DiscordEmbedBuilder(GetDiscordEmbed(embed))));
    }

    return listPages;
  }

  public static DiscordColor GetDiscordColour(EmbedMessage.Types.DiscordColour colour)
  {
    return colour switch
    {
      EmbedMessage.Types.DiscordColour.None            => DiscordColor.None,
      EmbedMessage.Types.DiscordColour.Black           => DiscordColor.Black,
      EmbedMessage.Types.DiscordColour.White           => DiscordColor.White,
      EmbedMessage.Types.DiscordColour.Gray            => DiscordColor.Gray,
      EmbedMessage.Types.DiscordColour.DarkGray        => DiscordColor.DarkGray,
      EmbedMessage.Types.DiscordColour.LightGray       => DiscordColor.LightGray,
      EmbedMessage.Types.DiscordColour.VeryDarkGray    => DiscordColor.VeryDarkGray,
      EmbedMessage.Types.DiscordColour.Blurple         => DiscordColor.Blurple,
      EmbedMessage.Types.DiscordColour.Grayple         => DiscordColor.Grayple,
      EmbedMessage.Types.DiscordColour.DarkButNotBlack => DiscordColor.DarkButNotBlack,
      EmbedMessage.Types.DiscordColour.NotQuiteBlack   => DiscordColor.NotQuiteBlack,
      EmbedMessage.Types.DiscordColour.Red             => DiscordColor.Red,
      EmbedMessage.Types.DiscordColour.DarkRed         => DiscordColor.DarkRed,
      EmbedMessage.Types.DiscordColour.Green           => DiscordColor.Green,
      EmbedMessage.Types.DiscordColour.DarkGreen       => DiscordColor.DarkGreen,
      EmbedMessage.Types.DiscordColour.Blue            => DiscordColor.Blue,
      EmbedMessage.Types.DiscordColour.DarkBlue        => DiscordColor.DarkBlue,
      EmbedMessage.Types.DiscordColour.Yellow          => DiscordColor.Yellow,
      EmbedMessage.Types.DiscordColour.Cyan            => DiscordColor.Cyan,
      EmbedMessage.Types.DiscordColour.Magenta         => DiscordColor.Magenta,
      EmbedMessage.Types.DiscordColour.Teal            => DiscordColor.Teal,
      EmbedMessage.Types.DiscordColour.Aquamarine      => DiscordColor.Aquamarine,
      EmbedMessage.Types.DiscordColour.Gold            => DiscordColor.Gold,
      EmbedMessage.Types.DiscordColour.Goldenrod       => DiscordColor.Goldenrod,
      EmbedMessage.Types.DiscordColour.Azure           => DiscordColor.Azure,
      EmbedMessage.Types.DiscordColour.Rose            => DiscordColor.Rose,
      EmbedMessage.Types.DiscordColour.SpringGreen     => DiscordColor.SpringGreen,
      EmbedMessage.Types.DiscordColour.Chartreuse      => DiscordColor.Chartreuse,
      EmbedMessage.Types.DiscordColour.Orange          => DiscordColor.Orange,
      EmbedMessage.Types.DiscordColour.Purple          => DiscordColor.Purple,
      EmbedMessage.Types.DiscordColour.Violet          => DiscordColor.Violet,
      EmbedMessage.Types.DiscordColour.Brown           => DiscordColor.Brown,
      EmbedMessage.Types.DiscordColour.HotPink         => DiscordColor.HotPink,
      EmbedMessage.Types.DiscordColour.Lilac           => DiscordColor.Lilac,
      EmbedMessage.Types.DiscordColour.CornflowerBlue  => DiscordColor.CornflowerBlue,
      EmbedMessage.Types.DiscordColour.MidnightBlue    => DiscordColor.MidnightBlue,
      EmbedMessage.Types.DiscordColour.Wheat           => DiscordColor.Wheat,
      EmbedMessage.Types.DiscordColour.IndianRed       => DiscordColor.IndianRed,
      EmbedMessage.Types.DiscordColour.Turquoise       => DiscordColor.Turquoise,
      EmbedMessage.Types.DiscordColour.SapGreen        => DiscordColor.SapGreen,
      EmbedMessage.Types.DiscordColour.PhthaloBlue     => DiscordColor.PhthaloBlue,
      EmbedMessage.Types.DiscordColour.PhthaloGreen    => DiscordColor.PhthaloGreen,
      EmbedMessage.Types.DiscordColour.Sienna          => DiscordColor.Sienna,
      _                                                => DiscordColor.None
    };
  }

  public static string ReadManifestData(string embeddedFileName)
  {
    Assembly assembly = Assembly.GetExecutingAssembly();
    string resourceName = assembly.GetManifestResourceNames().First(s => s.EndsWith(embeddedFileName, StringComparison.CurrentCultureIgnoreCase));

    using Stream stream = assembly.GetManifestResourceStream(resourceName);
    if (stream == null)
    {
      throw new InvalidOperationException("Could not load manifest resource stream.");
    }

    using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
    return reader.ReadToEnd();
  }

  public static bool IsPossibleSteamID(string steamID, out ulong id)
  {
    id = 0;
    return steamID.Length >= 17 && ulong.TryParse(steamID.Replace("@steam", ""), out id);
  }
}