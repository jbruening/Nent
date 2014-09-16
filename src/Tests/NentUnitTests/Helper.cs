using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NentUnitTests
{
    class Helper
    {
        public static void WaitUntil(Func<bool> func, double waitTime = 2000)
        {
            double counter = 0;
            while (!func())
            {
                if (counter >= waitTime)
                {
                    Assert.Fail("Timed out");
                }
                Thread.Sleep(10);
                counter += 10;
            }
        }
    }
}
