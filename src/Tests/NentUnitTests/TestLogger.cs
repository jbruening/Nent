using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nent;

namespace NentUnitTests
{
    /// <summary>
    /// Console recipient for the log
    /// </summary>
    public sealed class TestLogger : ILogger
    {
        /// <summary>
        /// Info
        /// </summary>
        /// <param name="info"></param>
        /// <param name="args"></param>
        public void Info(string info, params object[] args)
        {
            Console.WriteLine(info, args);
        }

        /// <summary>
        /// Warning
        /// </summary>
        /// <param name="info"></param>
        /// <param name="args"></param>
        public void Warning(string info, params object[] args)
        {
            Console.WriteLine(info, args);
        }

        /// <summary>
        /// error
        /// </summary>
        /// <param name="info"></param>
        /// <param name="args"></param>
        public void Error(string info, params object[] args)
        {
            Assert.Fail(info, args);
        }
    }
}
