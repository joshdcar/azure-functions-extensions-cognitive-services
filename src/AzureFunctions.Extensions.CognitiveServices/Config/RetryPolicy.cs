using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Config
{
    public class RetryPolicy
    {
        public int MaxRetryAttemptsAfterThrottle { get; set; } = 3;

        public int MaxRetryWaitTimeInSeconds { get; set; } = 90;
    }

    public class PollingPolicy
    {
        public int MaxRetryAttempts { get; set; } = 10;

        public int WaitBetweenRetry { get; set; } = 1;

        public int MaxRetryWaitTimeInSeconds { get; set; } = 120;
    }
}
