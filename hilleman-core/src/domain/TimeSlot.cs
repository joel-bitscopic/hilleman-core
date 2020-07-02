using System;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class TimeSlot
    {
        public DateTime start;
        public DateTime end;
        public String text;
        public bool available;

        public TimeSlot() { }
    }
}