using System.Diagnostics;
using System.Threading.Tasks;
using Orleans.Runtime;
using System;
using Microsoft.Extensions.Options;

namespace Orleans.Dashboard.Reports.Tracking
{
    internal sealed class GrainMethodExecutionFilter : IIncomingGrainCallFilter
    {
        private readonly IAgentMessageWriter _messageWriter;
        private readonly AgentTrackingOptions _options;

        public GrainMethodExecutionFilter(IOptions<AgentTrackingOptions> options, IAgentMessageWriter writer)
        {
            this._messageWriter = writer;
            this._options = options.Value;
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            var grainInterface = context.InterfaceMethod.DeclaringType.FullName;
            var grainImplementation = context.ImplementationMethod.DeclaringType.FullName;
            var methodName = context.InterfaceMethod.Name;
            var grainKey = this.GetGrainPrimaryKey(context);
            var grainId = context.Grain.ToString();

            var activity = CreateActivity($"{grainImplementation}.{methodName}")
                .AddTag(Constants.GrainInterface, grainInterface)
                .AddTag(Constants.GrainImplementation, grainImplementation)
                .AddTag(Constants.IsSystemTarget, string.IsNullOrWhiteSpace(grainKey).ToString())
                .AddTag(Constants.MethodName, methodName)
                .AddTag(Constants.GrainPrimaryKey, grainKey)
                .AddTag(Constants.GrainIdentity, grainId);

            try
            {
                activity.Start();
                await context.Invoke();
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
                    Type = ReportMessageType.GrainMethodExecution,
                    Payload = TrackingMessage.FromActivity(completedActivity)
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

            if (Activity.Current != null)
            {
                newActivity.SetParentId(Activity.Current.Id);
            }
            else
            {
                var parentId = RequestContext.Get(Constants.ParentActivityId)?.ToString();
                if (!string.IsNullOrWhiteSpace(parentId))
                {
                    newActivity.SetParentId(parentId);
                }
            }

            var correlationId = RequestContext.Get(Constants.CorrelationId)?.ToString();
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                newActivity.AddTag(Constants.CorrelationId, correlationId);
            }

            Activity.Current = newActivity;

            return newActivity;
        }

        private string GetGrainPrimaryKey(IIncomingGrainCallContext context)
        {
            //TODO: Find a better way to figure out the key type
            if (context.Grain is ISystemTarget) return string.Empty;
            if (context.Grain is IGrainWithIntegerKey) return context.Grain.GetPrimaryKeyLong().ToString();
            if (context.Grain is IGrainWithGuidKey) return context.Grain.GetPrimaryKey().ToString();
            if (context.Grain is IGrainWithStringKey) return context.Grain.GetPrimaryKeyString();

            //TODO: Deal with compount keys

            return string.Empty;
        }
    }
}