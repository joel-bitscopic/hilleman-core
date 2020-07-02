using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.bitscopic.hilleman.core.domain.exception.vista
{
    public class MErrorException : ApplicationException
    {
        public MErrorException() : base() { }

        public MErrorException(String message) : base(message) { }
    }
}
