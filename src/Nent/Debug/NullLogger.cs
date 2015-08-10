using System;

namespace Nent
{
    /// <summary>
    /// Logger, but logs to nowhere
    /// </summary>
    public sealed class NullLogger : ILogger
    {
        /// <summary>
        /// informational message
        /// </summary>
        /// <param name="info"></param>
        /// <param name="args"></param>
        public void Info(string info, params object[] args)
        {
            
        }

        /// <summary>
        /// warning message
        /// </summary>
        /// <param name="info"></param>
        /// <param name="args"></param>
        public void Warning(string info, params object[] args)
        {
            
        }

        /// <summary>
        /// error message
        /// </summary>
        /// <param name="info"></param>
        /// <param name="args"></param>
        public void Error(string info, params object[] args)
        {
            
        }

        /// <summary>
        /// exception message
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="info"></param>
        /// <param name="args"></param>
        public void Exception(Exception exception, string info, params object[] args)
        {
            
        }
    }
}
