using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace PhotoBooth.Client.Models
{
    public class CustomRetryPolicy : IRetryPolicy
    {
        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            return TimeSpan.FromMilliseconds(500);
        }
    }
}
