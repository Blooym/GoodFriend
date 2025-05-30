using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace GoodFriend.Networking.SignalR;

internal sealed class ForeverRetryPolicy : IRetryPolicy
{
    public TimeSpan? NextRetryDelay(RetryContext retryContext) => retryContext.PreviousRetryCount switch
    {
        0 => (TimeSpan?)TimeSpan.FromSeconds(10),
        1 => (TimeSpan?)TimeSpan.FromSeconds(30),
        2 => (TimeSpan?)TimeSpan.FromSeconds(60),
        _ => (TimeSpan?)TimeSpan.FromSeconds(120),
    };
}
