using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nent
{
    /// <summary>
    /// Used for yielding yields via StartCoroutine. should support infinite depth.
    /// </summary>
    public sealed class Coroutine : YieldInstruction
    {
        internal IEnumerator Routine;
        private bool _innerFinished = false;

        internal Coroutine(IEnumerator routine)
        {
            Routine = routine;
        }

        /// <summary>
        /// method that says if the YieldInstruction is done
        /// </summary>
        public override bool IsDone()
        {
            if (_innerFinished) return true;

            if (Routine.Current != null)
            {
                if (Routine.Current is YieldInstruction)
                {
                    if ((Routine.Current as YieldInstruction).IsDone())
                    {
                        _innerFinished = true;
                        return false;
                    }
                }
            }

            //if the routine finishes, then we need to say we're finished
            _innerFinished = !Routine.MoveNext();
            return false;
        }

        /// <summary>
        /// standardized behaviour to run through coroutines
        /// </summary>
        /// <param name="unblockedCoroutines"></param>
        /// <param name="shouldRunNextFrame"></param>
        internal static void Run(ref List<IEnumerator> unblockedCoroutines, ref List<IEnumerator> shouldRunNextFrame)
        {
            for (int i = 0; i < unblockedCoroutines.Count; i++)
            {
                var coroutine = unblockedCoroutines[i];

                var yRoute = coroutine.Current as YieldInstruction;
                if (yRoute != null)
                {
                    //yielding on a yieldinstruction
                    //running IsDone is equivilent to MoveNext for yieldinstructions
                    if (!yRoute.IsDone())
                    {
                        shouldRunNextFrame.Add(coroutine);
                        continue;
                    }
                }

                //everything else...
                if (!coroutine.MoveNext())
                    // This coroutine has finished
                    continue;

                yRoute = coroutine.Current as YieldInstruction;
                if (yRoute is Coroutine)
                {
                    //remove the routine we just made from the top of the stack...
                    var croute = yRoute as Coroutine;
                    lock (shouldRunNextFrame)
                    {
                        var last = shouldRunNextFrame[shouldRunNextFrame.Count - 1];
                        if (!ReferenceEquals(croute.Routine, last))
                        {
                            Debug.LogError(
                                "Something went wrong when yielding on a coroutine. Are you using coroutines in other threads?");
                            continue;
                        }
                        shouldRunNextFrame.RemoveAt(shouldRunNextFrame.Count - 1);

                        //add the outer so that it can call the inner.
                        shouldRunNextFrame.Add(coroutine);
                    }
                    continue;
                }

                if (yRoute == null)
                {
                    // This coroutine yielded null, or some other value we don't understand; run it next frame.
                    shouldRunNextFrame.Add(coroutine);
                    continue;
                }

                if (!yRoute.IsDone())
                {
                    shouldRunNextFrame.Add(coroutine);
                }
            }

            unblockedCoroutines.Clear();
            unblockedCoroutines.AddRange(shouldRunNextFrame);
            shouldRunNextFrame.Clear();
        }
    }
}
