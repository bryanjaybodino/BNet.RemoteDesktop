using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BNet.RemoteDesktop.Models;

namespace BNet.RemoteDesktop.Repositories
{
    /// <summary>
    /// Persists a list of SessionEntry objects to
    /// %AppData%\RemoteDesk\sessions.json.
    /// </summary>
    public static class SessionStore
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RemoteDesk",
            "sessions.json");

        private const int MaxSessions = 20;

        public static List<SessionEntry> Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return new List<SessionEntry>();
                string json = File.ReadAllText(FilePath);
                return Json.Deserialize<List<SessionEntry>>(json)
                    ?? new List<SessionEntry>();
            }
            catch { return new List<SessionEntry>(); }
        }

        /// <summary>
        /// Upsert by IP+Port: updates MachineName + LastConnected,
        /// moves entry to top, trims to MaxSessions, then saves.
        /// </summary>
        public static void SaveOrUpdate(string ip, int port, string machineName)
        {
            var list = Load();

            var existing = list.FirstOrDefault(
                s => s.HostIp == ip && s.Port == port);

            if (existing != null)
            {
                list.Remove(existing);
                existing.MachineName = machineName ?? existing.MachineName;
                existing.LastConnected = DateTime.Now;
                list.Insert(0, existing);
            }
            else
            {
                list.Insert(0, new SessionEntry
                {
                    HostIp = ip,
                    Port = port,
                    MachineName = machineName ?? "",
                    LastConnected = DateTime.Now
                });
            }

            if (list.Count > MaxSessions)
                list = list.Take(MaxSessions).ToList();

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                File.WriteAllText(FilePath, Json.Serialize(list, true));
            }
            catch { }
        }

        public static void Delete(string ip, int port)
        {
            var list = Load();
            list.RemoveAll(s => s.HostIp == ip && s.Port == port);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                File.WriteAllText(FilePath, Json.Serialize(list, true));
            }
            catch { }
        }
    }
}