using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Orleans.Runtime;

namespace Orleans.Dashboard.Reports.Tracking
{
    public interface IExternalDependencyTracker
    {
        Task<TResult> Track<TResult>(string name, Func<Activity, Task<TResult>> func);
        Task Track(string name, Func<Activity, Task> func);
    }

    internal class ExternalDependencyTracker : IExternalDependencyTracker
    {
        private readonly IAgentMessageWriter _messageWriter;
        private readonly AgentTrackingOptions _options;

        public ExternalDependencyTracker(IAgentMessageWriter writer, IOptions<AgentTrackingOptions> options)
        {
            this._messageWriter = writer;
            this._options = options.Value;
        }

        public async Task<TResult> Track<TResult>(string name, Func<Activity, Task<TResult>> func)
        {
            var activity = this.CreateActivity(name);
            TResult result = default;

            try
            {
                activity.Start();
                result = await func(activity);
            }
            catch (Exception exc)
            {
                activity.AddTag(Constants.ExceptionType, exc.GetType().FullName)
                    .AddTag(Constants.ExceptionMessage, exc.Message)
                    .AddTag(Constants.ExceptionSource, exc.Source)
                    .AddTag(Constants.StackTrace, this._options.IncludeStackTrace ? exc.StackTrace : string.Empty);
                throw exc;
            }
            finally
            {
                var completedActivity = this.FinishActivity(activity);
                this._messageWriter.Write(new ReportMessage
                {
                    Type = ReportMessageType.ExternalDependencyTrack,
                    Payload = completedActivity
                });
            }

            return result;
        }

        public async Task Track(string name, Func<Activity, Task> func)
        {
            var activity = this.CreateActivity(name);

            try
            {
                activity.Start();
                await func(activity);
            }
            catch (Exception exc)
            {
                activity.AddTag(Constants.ExceptionType, exc.GetType().FullName)
                    .AddTag(Constants.ExceptionMessage, exc.Message)
                    .AddTag(Constants.ExceptionSource, exc.Source)
                    .AddTag(Constants.StackTrace, this._options.IncludeStackTrace ? exc.StackTrace : string.Empty);
                throw;
            }
            finally
            {
                var completedActivity = this.FinishActivity(activity);
                this._messageWriter.Write(new ReportMessage
                {
                    Type = ReportMessageType.ExternalDependencyTrack,
                    Payload = completedActivity
                });
            }
        }

        private Activity FinishActivity(Activity activity)
        {
            var rootActivityId = RequestContext.Get(Constants.RootActivityId)?.ToString();
            if (string.IsNullOrWhiteSpace(rootActivityId))
            {
                rootActivityId = activity.Id;
                RequestContext.Set(Constants.RootActivityId, rootActivityId);
            }
            activity.AddTag(Constants.RootActivityId, rootActivityId);
            activity.AddTag(Constants.ParentActivityId, activity.ParentId);
            RequestContext.Set(Constants.ParentActivityId, activity.ParentId);
            activity.Stop();

            return activity;
        }

        private Activity CreateActivity(string name)
        {
            var newActivity = new Activity(name);
            newActivity.SetParentId(Activity.Current.Id);

            var correlationId = RequestContext.Get(Constants.CorrelationId)?.ToString();
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                newActivity.AddTag(Constants.CorrelationId, correlationId);
            }

            Activity.Current = newActivity;

            return newActivity;
        }
    }
}