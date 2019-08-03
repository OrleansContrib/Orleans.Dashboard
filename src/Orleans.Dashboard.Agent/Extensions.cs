using System.Threading;
using System.Threading.Tasks;

namespace Orleans.Dashboard
{
    internal static class Extensions
    {
        internal static Task WhenCancelled(this CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            var waitForCancellation = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            token.Register(obj =>
            {
                var tcs = (TaskCompletionSource<object>)obj;
                tcs.TrySetResult(null);
            }, waitForCancellation);

            return waitForCancellation.Task;
        }
    }
}