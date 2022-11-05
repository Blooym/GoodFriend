using System;
using System.Collections.Generic;
using Dalamud.Logging;

namespace GoodFriend.Managers
{
    /// <summary>
    ///     Initializes and manages a record of events.
    /// </summary>
    internal sealed class EventLogManager
    {
        /// <summary>
        ///     The <see cref="EventLogEntry"/>s that have been logged.
        /// </summary>
        internal List<EventLogEntry> EventLog { get; private set; } = new();

        /// <summary>
        ///    The maximum number of entries to keep in the log.
        /// </summary>
        private readonly int MaxEntries = 100;

        /// <summary>
        ///     Adds a new entry to the <see cref="EventLog"/>.
        /// </summary>
        /// <param name="message">The message of the entry.</param>
        /// <param name="type">The type of the entry.</param>
        internal void AddEntry(string message, EventLogType type = EventLogType.Info)
        {
            EventLog.Add(new EventLogEntry
            {
                ID = Guid.NewGuid(),
                Type = type,
                Message = message,
                Timestamp = DateTime.Now,
            });
            PluginLog.Debug($"EventLogManager(AddEntry): Added entry to log: [{type}] \"{message}\"");
            if (EventLog.Count > MaxEntries)
            {
                EventLog.RemoveAt(0);
                PluginLog.Debug($"EventLogManager(AddEntry): Log is at max size ({MaxEntries}), removing oldest entry (ID: {EventLog[0].ID})");
            }
        }

        /// <summary>
        ///     Removes an entry from the <see cref="EventLogManager"/>'s log.
        /// </summary>
        /// <param name="id">The id of the entry to remove.</param>
        internal void RemoveEntry(Guid id)
        {
            PluginLog.Debug($"EventLogManager(RemoveEntry): Removing entry: {id}");
            _ = EventLog.RemoveAll(x => x.ID == id);
        }

        /// <summary>
        ///     Removes all entries from the <see cref="EventLogManager"/>'s log.
        /// </summary>
        internal void ClearLog()
        {
            PluginLog.Debug("EventLogManager(ClearLog): Clearing log.");
            EventLog.Clear();
        }

        /// <summary>
        ///    Gets a specific entry from the <see cref="EventLogManager"/>'s log.
        /// </summary>
        /// <param name="id">The id of the entry to get.</param>
        internal EventLogEntry? GetEntry(Guid id)
        {
            return EventLog.Find(x => x.ID == id);
        }

        /// <summary>
        ///     EventLogEntry is a single entry in the <see cref="EventLogManager"/> log.
        /// </summary>
        [Serializable]
        internal class EventLogEntry
        {
            public Guid ID { get; set; }
            public EventLogType Type { get; set; }
            public string? Message { get; set; }
            public DateTime Timestamp { get; set; }
        }

        /// <summary>
        ///     The type of the <see cref="EventLogEntry"/> .
        /// </summary>
        internal enum EventLogType
        {
            Debug,
            Info,
            Warning,
            Error,
        }
    }
}