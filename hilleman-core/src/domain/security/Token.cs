using System;
using System.Timers;

namespace com.bitscopic.hilleman.core.domain.security
{
    [Serializable]
    public class Token : IDisposable
    {
        public String tokenHash;
        public String value;
        public DateTime issued;
        public DateTime immutableExpiration;
        public TimeSpan timeout;
        public DateTime lastAccessed;
        public Int32 maxAccesses;
        public object state;

        Int32 _accesses = 0;

        [NonSerialized]
        Timer _timeoutTimer;
        [NonSerialized]
        ITokenStore _owner;
        [NonSerialized]
        internal ApiKey _appKey;
        //[NonSerialized]
        //internal string dbKey;

        public Token() 
        {
            this.issued = DateTime.Now;
            this.lastAccessed = DateTime.Now;
        }

        internal Token access()
        {
            _accesses++;
            return this;
        }

        /// <summary>
        /// Start a timeout timer on this token. If time elapses, a call to remove this token from the process' token store is made.
        /// Calling this function resets the timer if it has been set before.
        /// </summary>
        internal void startTokenTimer(ITokenStore owner)
        {
            _owner = owner;
            _timeoutTimer = new Timer(this.timeout.TotalMilliseconds);
            _timeoutTimer.Elapsed += new ElapsedEventHandler(timeoutTimer_Elapsed);

            _timeoutTimer.Start();
        }

        internal void resetTimer()
        {
            if (_timeoutTimer == null)
            {
                return;
            }
            _timeoutTimer.Stop();
            _timeoutTimer.Start();
        }

        internal void stopTimer()
        {
            if (_timeoutTimer != null)
            {
                _timeoutTimer.Stop();
            }
        }

        void timeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_timeoutTimer != null)
            {
                _timeoutTimer.Stop();
                _timeoutTimer.Dispose();
                _timeoutTimer = null;
            }
            _owner.revokeToken(this.value);
        }

        public void Dispose()
        {
            if (_timeoutTimer != null)
            {
                _timeoutTimer.Stop();
                _timeoutTimer = null;
            }
        }
    }
}