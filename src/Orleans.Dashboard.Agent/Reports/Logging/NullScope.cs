using System;

namespace Orleans.Dashboard.Reports.Logging
{
    /// <summary>
    /// An empty scope without any logic.
    /// </summary>
    internal class NullScope : IDisposable
    {
        private NullScope()
        {
        }

        public static NullScope Instance { get; } = new NullScope();

#pragma warning disable CA1063 // Implement IDisposable Correctly - Nothing at all to dispose.
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
#pragma warning restore CA1063 // Implement IDisposable Correctly - Nothing at all to dispose.
        {
        }
    }
}