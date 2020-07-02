using System;

namespace com.bitscopic.hilleman.core.dao.vista
{
    public class VistaConnectionFactory
    {

        public IVistaConnection getVistaConnection(com.bitscopic.hilleman.core.domain.SourceSystem ss)
        {
            if (ss.type == domain.SourceSystemType.VISTA_RPC_BROKER)
            {
                return new vista.rpc.VistaRpcConnection(ss);
            }
            else if (ss.type == domain.SourceSystemType.VISTA_CRUD_REST_SVC)
            {
                return new http.VistaHttpRestConnection(ss);
            }
            else if (ss.type == domain.SourceSystemType.SQLITE_CACHE)
            {
                return new sql.sqlite.VistaSqliteCacheConnection(ss);
            }
            else
            {
                throw new NotImplementedException("Unable to create a new IVistaConnection from that SourceSystem!");
            }
        }
    }
}