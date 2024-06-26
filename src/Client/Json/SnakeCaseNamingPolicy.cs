using System.Text.Json;

namespace GoodFriend.Client.Json;

/// <summary>
///     A naming policy that converts names to snake_case.
/// </summary>
internal sealed class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    /// <inheritdoc />
    public override string ConvertName(string name)
    {
        var result = string.Empty;
        var previous = char.MinValue;
        foreach (var c in name)
        {
            if (char.IsUpper(c) && previous != char.MinValue)
            {
                result += '_';
            }
            result += char.ToLower(c);
            previous = c;
        }
        return result;
    }
}
