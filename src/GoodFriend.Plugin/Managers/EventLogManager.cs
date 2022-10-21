namespace GoodFriend.Managers
{
    using System;
    using System.Collections.Generic;
    using Dalamud.Logging;

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
            this.EventLog.Add(new EventLogEntry
            {
                id = Guid.NewGuid(),
                type = type,
                message = message,
                timestamp = DateTime.Now,
            });
            PluginLog.Debug($"EventLogManager(AddEntry): Added entry to log: [{type}] \"{message}\"");
            if (this.EventLog.Count > this.MaxEntries)
            {
                this.EventLog.RemoveAt(0);
                PluginLog.Debug($"EventLogManager(AddEntry): Log is at max size ({this.MaxEntries}), removing oldest entry (ID: {this.EventLog[0].id})");
            }
        }

        /// <summary>
        ///     Removes an entry from the <see cref="EventLogManager"/>'s log.
        /// </summary>
        /// <param name="id">The id of the entry to remove.</param>
        internal void RemoveEntry(Guid id)
        {
            PluginLog.Debug($"EventLogManager(RemoveEntry): Removing entry: {id}");
            this.EventLog.RemoveAll(x => x.id == id);
        }

        /// <summary>
        ///     Removes all entries from the <see cref="EventLogManager"/>'s log.
        /// </summary>
        internal void ClearLog()
        {
            PluginLog.Debug("EventLogManager(ClearLog): Clearing log.");
            this.EventLog.Clear();
        }

        /// <summary>
        ///    Gets a specific entry from the <see cref="EventLogManager"/>'s log.
        /// </summary>
        /// <param name="id">The id of the entry to get.</param>
        internal EventLogEntry? GetEntry(Guid id)
        {
            return this.EventLog.Find(x => x.id == id);
        }

        /// <summary>
        ///     EventLogEntry is a single entry in the <see cref="EventLogManager"/> log.
        /// </summary>
        [Serializable]
        internal class EventLogEntry
        {
            public Guid id { get; set; }
            public EventLogType type { get; set; }
            public string? message { get; set; }
            public DateTime timestamp { get; set; }
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