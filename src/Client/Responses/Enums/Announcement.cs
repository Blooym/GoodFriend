namespace GoodFriend.Client.Responses.Enums
{
    /// <summary>
    ///     The kind of announcement being made.
    /// </summary>
    public enum AnnouncementKind
    {
        Informational,
        Maintenance,
        Critical,
    }

    /// <summary>
    ///     The cause/reason for the announcement.
    /// </summary>
    public enum AnnouncementCause
    {
        Manual,
        Automatic,
        Scheduled,
    }
}