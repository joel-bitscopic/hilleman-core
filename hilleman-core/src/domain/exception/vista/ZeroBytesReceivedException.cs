using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.bitscopic.hilleman.core.domain.exception.vista
{
    public class ZeroBytesReceivedException : ApplicationException
    {

        public ZeroBytesReceivedException() : base() { }

        public ZeroBytesReceivedException(String message) : base(message) { }
    }
}
