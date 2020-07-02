using com.bitscopic.hilleman.core.domain.exception;
using System;

namespace com.bitscopic.hilleman.core.dao
{
    public class CrrudException : HillemanBaseException
    {
        public CrrudException() : base() { }

        public CrrudException(String message) : base(message) { }
    }
}