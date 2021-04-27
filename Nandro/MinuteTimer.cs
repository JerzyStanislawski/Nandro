using System;
using System.Threading;

namespace Nandro
{
    class MinuteTimer
    {
        Timer _timer;

        public MinuteTimer(Action action, bool startAtOnce = true)
        {
            _timer = new Timer(state => action.Invoke(), null, startAtOnce ? 0 : 60 * 1000, 60 * 1000);
        }
    }
}
