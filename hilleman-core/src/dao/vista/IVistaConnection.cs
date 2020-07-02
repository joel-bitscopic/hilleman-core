using System;

namespace com.bitscopic.hilleman.core.dao.vista
{
    public interface IVistaConnection
    {
        void connect();
        void disconnect();
        com.bitscopic.hilleman.core.domain.SourceSystem getSource();
        VistaConnectionStateInfo getStateInfo();
    }
}