using com.bitscopic.hilleman.core.domain;
using System;

namespace com.bitscopic.hilleman.core.dao.vista.rpc
{
    public class VistaUserRpcConnection : VistaRpcConnection, IVistaConnection
    {
        public User user;
        public bool isAvailable;

        public VistaUserRpcConnection(SourceSystem source) : base(source) { }

        public override void cleanUp()
        {
            this.isAvailable = false;
            base.cleanUp();
        }

        public bool safeCallXWBImHere()
        {
            try
            {
                VistaRpcQuery request = new VistaRpcQuery("XWB IM HERE");
                if ("1" != (String)base.query(request))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}