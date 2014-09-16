using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NentUnitTests
{
    class Helper
    {
        public static void WaitUntil(Func<bool> func, double waitTime = 2)
        {
            var timer = new Stopwatch();
            waitTime *= 1000;
            timer.Start();
            while (!func())
            {
                if (timer.ElapsedMilliseconds >= waitTime)
                {
                    Assert.Fail("Timed out");
                }
                Thread.Sleep(10);
            }
        }
    }
}
