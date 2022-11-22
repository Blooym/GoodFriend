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
        private readonly List<EventLogEntry> eventLog = new();

        /// <summary>
        ///    The maximum number of entries to keep in the log.
        /// </summary>
        private const int MaxEntries = 150;

        /// <summary>
        ///    Gets a specific entry from the <see cref="EventLogManager"/> event log by ID.
        /// </summary>
        /// <param name="id">The id of the entry to get.</param>
        internal EventLogEntry? GetEntry(Guid id) => this.eventLog.Find(x => x.ID == id);

        /// <summary>
        ///     Gets all entries from the <see cref="EventLogManager"/> event log.
        /// </summary>
        internal List<EventLogEntry> GetEntries() => this.eventLog;

        /// <summary>
        ///     Gets all entries from the <see cref="EventLogManager"/> event log of a specific type.
        /// </summary>
        /// <param name="type">The type of entries to get.</param>
        /// <param name="showHigher">Whether to show entries of a higher type.</param>
        internal List<EventLogEntry> GetEntries(EventLogType type, bool showHigher = true) => this.eventLog.FindAll(x => x.Type == type || (showHigher && x.Type > type));

        /// <summary>
        ///     Adds a new entry to the <see cref="eventLog"/>.
        /// </summary>
        /// <param name="message">The message of the entry.</param>
        /// <param name="type">The type of the entry.</param>
        internal void AddEntry(string message, EventLogType type = EventLogType.Info)
        {
            this.eventLog.Add(new EventLogEntry
            {
                ID = Guid.NewGuid(),
                Type = type,
                Message = message,
                Timestamp = DateTime.Now,
            });
            PluginLog.Debug($"EventLogManager(AddEntry): Added entry to log: [{type}] \"{message}\"");
            if (this.eventLog.Count > MaxEntries)
            {
                this.eventLog.RemoveRange(0, MaxEntries / 2);
                PluginLog.Verbose($"EventLogManager(AddEntry): Log is at max size ({MaxEntries}), removing half of the oldest entries.");
            }
        }

        /// <summary>
        ///     Removes an entry from the <see cref="EventLogManager"/>'s log.
        /// </summary>
        /// <param name="id">The id of the entry to remove.</param>
        internal void RemoveEntry(Guid id)
        {
            PluginLog.Debug($"EventLogManager(RemoveEntry): Removing entry: {id}");
            this.eventLog.RemoveAll(x => x.ID == id);
        }

        /// <summary>
        ///     Removes all entries from the <see cref="EventLogManager"/>'s log.
        /// </summary>
        internal void ClearAll()
        {
            PluginLog.Debug("EventLogManager(ClearLog): Clearing log.");
            this.eventLog.Clear();
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
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3,
        }
    }
}
