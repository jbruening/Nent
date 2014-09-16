using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nent
{
    /// <summary>
    /// Yield for the specified time
    /// </summary>
    public class WaitForSeconds : YieldInstruction
    {
        private readonly GameState _state;
        private readonly double _finishTime;

        /// <summary>
        /// wait for the specified number of seconds
        /// </summary>
        /// <param name="state"></param>
        /// <param name="seconds"></param>
        public WaitForSeconds(GameState state, float seconds)
        {
            _state = state;
            _finishTime = _state.Time + seconds;
        }

        /// <summary>
        /// when the yieldinstruction finishes
        /// </summary>
        public override bool IsDone()
        {
            if (_state.Time >= _finishTime)
                return true;
            return false;
        }
    }
}
