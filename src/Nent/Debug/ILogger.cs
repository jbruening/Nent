using System;

namespace Nent
{
    /// <summary>
    /// Interface for logging information
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// informational message
        /// </summary>
        /// <param name="info"></param>
        /// <param name="args"></param>
        void Info(string info, params object[] args);
        /// <summary>
        /// warning message
        /// </summary>
        /// <param name="info"></param>
        /// <param name="args"></param>
        void Warning(string info, params object[] args);
        /// <summary>
        /// error message
        /// </summary>
        /// <param name="info"></param>
        /// <param name="args"></param>
        void Error(string info, params object[] args);

        /// <summary>
        /// exception
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="info"></param>
        /// <param name="args"></param>
        void Exception(Exception exception, string info, params object[] args);
    }
}
