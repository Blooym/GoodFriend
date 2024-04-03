namespace GoodFriend.Client.Http.Enums;

/// <summary>
///     The kind of announcement being made.
/// </summary>
public enum AnnouncementKind
{
    Informational,
    Maintenance,
    Critical,
    Miscellaneous,
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
