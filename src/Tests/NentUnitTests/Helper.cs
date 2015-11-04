using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nent;

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

        /// <summary>
        /// similar to waituntil, but throws if func returns false
        /// </summary>
        /// <param name="func"></param>
        /// <param name="waitTime"></param>
        public static void Ensure(Func<bool> func, double waitTime = 2)
        {
            var watch = new Stopwatch();
            waitTime *= 1000;
            watch.Start();
            while (func())
            {
                if (watch.ElapsedMilliseconds >= waitTime)
                    return;
                Thread.Sleep(10);
            }
            Assert.Fail("Returned false at some point when it shouldn't have");
        }

        public static GameObject CreateInvoke(GameState state)
        {
            GameObject gobj = null;
            state.InvokeIfRequired(() => gobj = state.CreateNewGameObject());
            WaitUntil(() => gobj != null);
            return gobj;
        }

        public static T AddInvoke<T>(GameObject gobj) where T : Component
        {
            T ret = null;
            gobj.GameState.InvokeIfRequired(() => { ret = gobj.AddComponent<T>(); });
            WaitUntil(() => ret != null);
            return ret;
        }
    }
}
