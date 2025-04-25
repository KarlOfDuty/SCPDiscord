using System;
using System.Collections.Generic;
using System.IO;
using PluginAPI.Core;

namespace SCPDiscord
{
  internal static class Logger
  {
    private static List<string> startupCache = new List<string>();
    private static TextWriter logFileWriter = null;
    private static object fileLock = new object();

    internal static void Info(string message)
    {
      Log.Info(message.Replace("<", "<\0"));
      LogToFile("INFO", message);
    }

    internal static void Warn(string message)
    {
      Log.Warning(message.Replace("<", "<\0"));
      LogToFile("WARNING", message);
    }

    internal static void Error(string message)
    {
      Log.Error(message.Replace("<", "<\0"));
      LogToFile("ERROR", message);
    }

    internal static void Debug(string message)
    {
      if (Config.GetBool("settings.debug"))
      {
        Log.Debug(message.Replace("<", "<\0"));
      }

      LogToFile("DEBUG", message);
    }

    private static void LogToFile(string loglevel, string message)
    {
      // Add prefix
      string logMessage = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": [" + loglevel + "] ";

      // Add message with indentation
      logMessage += message.Replace("\n", "\n" + new string(' ', logMessage.Length));

      lock (fileLock)
      {
        if (!Config.ready)
        {
          startupCache.Add(logMessage);
          return;
        }

        if (logFileWriter == null)
        {
          return;
        }

        try
        {
          logFileWriter.WriteLine(logMessage);
          logFileWriter.Flush();
        }
        catch (Exception e)
        {
          Log.Error("Error writing to log file:\n" + e);
        }
      }
    }

    internal static void SetupLogfile(string path)
    {
      lock (fileLock)
      {
        if (string.IsNullOrWhiteSpace(path))
        {
          startupCache.Clear();
          logFileWriter?.Close();
          logFileWriter = null;
          return;
        }

        if (File.Exists(path))
        {
          try
          {
            logFileWriter = File.AppendText(Path.GetFullPath(path));
            Log.Info("Opened log file \"" + Path.GetFullPath(path) + "\".");
          }
          catch (Exception e)
          {
            Log.Error("Error opening log file \"" + Path.GetFullPath(path) + "\":\n" + e);
          }
        }
        else
        {
          try
          {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
          }
          catch (Exception e)
          {
            Log.Error("Error creating log file directory \"" + Path.GetDirectoryName(Path.GetFullPath(path)) + "\":\n" + e);
          }

          try
          {
            logFileWriter = File.CreateText(Path.GetFullPath(path));
            Log.Info("Created log file \"" + Path.GetFullPath(path) + "\".");
          }
          catch (Exception e)
          {
            Log.Error("Error creating log file \"" + Path.GetFullPath(path) + "\":\n" + e);
          }
        }

        if (!startupCache.IsEmpty())
        {
          try
          {
            foreach (string line in startupCache)
            {
              logFileWriter.WriteLine(line);
            }

            logFileWriter.Flush();
          }
          catch (Exception e)
          {
            Log.Error("Error writing cache to log file:\n" + e);
          }

          startupCache.Clear();
        }
      }
    }
  }
}