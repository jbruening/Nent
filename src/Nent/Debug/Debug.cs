using System;

namespace Nent
{
    /// <summary>
    /// Debug
    /// </summary>
    public static class Debug
    {
        /// <summary>
        /// Reference to the actual log receiver
        /// </summary>
        public static ILogger logger = new NullLogger();

        /// <summary>
        /// Info message
        /// </summary>
        /// <param name="value"></param>
        /// <param name="args"></param>
        [JetBrains.Annotations.StringFormatMethod("value")]
        public static void Log(string value, params object[] args)
        {
            logger.Info(value, args);
        }
        /// <summary>
        /// Error message
        /// </summary>
        /// <param name="value"></param>
        /// <param name="args"></param>
        [JetBrains.Annotations.StringFormatMethod("value")]
        public static void LogError(string value, params object[] args)
        {
            logger.Error(value, args);
        }
        /// <summary>
        /// Warning message
        /// </summary>
        /// <param name="value"></param>
        /// <param name="args"></param>
        [JetBrains.Annotations.StringFormatMethod("value")]
        public static void LogWarning(string value, params object[] args)
        {
            logger.Warning(value, args);
        }

        /// <summary>
        /// Exception message
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="value"></param>
        /// <param name="args"></param>
        [JetBrains.Annotations.StringFormatMethod("value")]
        public static void LogException(Exception exception, string value, params object[] args)
        {
            logger.Exception(exception, value, args);
        }

        public static void LogException(Exception exception)
        {
            LogException(exception, "");
        }
    }
}
