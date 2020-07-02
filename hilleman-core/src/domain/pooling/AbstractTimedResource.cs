using System;

namespace com.bitscopic.hilleman.core.domain.pooling
{
    public abstract class AbstractTimedResource : AbstractResource
    {
        public DateTime LastUsed;
        System.Timers.Timer _timer;
        bool _timedOut = false;

        public void setTimeout(TimeSpan timeout)
        {
            this._timer = new System.Timers.Timer(timeout.TotalMilliseconds);
            this._timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
            this._timer.Start();
        }

        void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timedOut = true;
            _timer.Stop();
            this.cleanUp();
            _timer.Dispose();
        }

        public override bool isAlive()
        {
            return !_timedOut; // return opposite of timed out
        }

        /// <summary>
        /// Stop the timer. Use only when disposing of the resource without timeouts
        /// </summary>
        internal void stopTimer()
        {
            if (_timer == null)
            {
                return;
            }
            _timer.Stop();
        }

        public void resetTimer()
        {
            if (_timer == null)
            {
                return;
            }
            _timer.Stop();
            _timer.Start();
        }

    }
}
