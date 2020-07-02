using com.bitscopic.hilleman.core.domain.pooling;
using com.bitscopic.hilleman.core.domain.pooling.connection.vista;
using System;

namespace com.bitscopic.hilleman.core.dao.vista.rpc
{
    /// <summary>
    /// Helper class for commonly used VistA Tools DAO functions. Note: VistaRpcConnectionPools MUST be correctly configured before invoking these APIs
    /// </summary>
    public static class VistaRpcToolsDaoHelper
    {
        public static String gvv(String sitecode, String arg)
        {
            VistaRpcConnection cxn = (VistaRpcConnection)VistaRpcConnectionPools.getInstance().checkOutAlive(sitecode);
            try
            {
                return new VistaRpcCrrudDao(cxn).gvv(arg);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                VistaRpcConnectionPools.getInstance().checkIn((AbstractResource)cxn);
            }
        }

        public static ReadRangeResponse readRange(String sitecode, ReadRangeRequest request)
        {
            VistaRpcConnection cxn = (VistaRpcConnection)VistaRpcConnectionPools.getInstance().checkOutAlive(sitecode);
            try
            {
                return new VistaRpcCrrudDao(cxn).readRange(request);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                VistaRpcConnectionPools.getInstance().checkIn((AbstractResource)cxn);
            }
        }

        public static ReadResponse read(String sitecode, ReadRequest request)
        {
            VistaRpcConnection cxn = (VistaRpcConnection)VistaRpcConnectionPools.getInstance().checkOutAlive(sitecode);
            try
            {
                return new VistaRpcCrrudDao(cxn).read(request);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                VistaRpcConnectionPools.getInstance().checkIn((AbstractResource)cxn);
            }
        }
    }
}
