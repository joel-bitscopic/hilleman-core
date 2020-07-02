using System;

namespace com.bitscopic.hilleman.core.domain.resource
{
    public interface IConnection
    {
        void connect();

        void disconnect();
    }
}
