using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Orleans.Dashboard.Reports.Tracking
{
    internal class TrackingMessage
    {
        public string Id { get; set; }
        public string OperationName { get; set; }
        public string RootId { get; set; }
        public string ParentId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Tags { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Baggage { get; set; }

        public static TrackingMessage FromActivity(Activity activity)
        {
            return new TrackingMessage
            {
                Id = activity.Id,
                OperationName = activity.OperationName,
                Baggage = activity.Baggage,
                Duration = activity.Duration,
                EndTime = activity.StartTimeUtc + activity.Duration,
                ParentId = activity.ParentId,
                RootId = activity.RootId,
                StartTime = activity.StartTimeUtc,
                Tags = activity.Tags
            };
        }
    }
}