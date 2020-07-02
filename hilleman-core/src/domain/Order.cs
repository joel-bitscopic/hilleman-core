using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class Order : BaseClass
    {
        public Order() { }

        public String text;
        public IdAndValue status;

        public DateTime entered;
        public DateTime start;
        public DateTime stop;
        public DateTime completed;

        public object objectOfOrder;
        public User orderedBy;
        public User enteredBy;
        public User signedBy;
        public DateTime signed;

        public Patient patient;
    }
}