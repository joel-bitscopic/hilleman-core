using com.bitscopic.hilleman.core.domain.exception;
using System;

namespace com.bitscopic.hilleman.core.dao.vista.rpc
{
    public class VistaRpcConnectionException : HillemanBaseException
    {
        public VistaRpcConnectionException() : base() { }
        public VistaRpcConnectionException(String message) : base(message) { }
    }
}