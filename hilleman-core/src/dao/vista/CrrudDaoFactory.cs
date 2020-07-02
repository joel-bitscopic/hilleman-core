using System;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.dao.vista.rpc;
using com.bitscopic.hilleman.core.dao.vista.http;
using System.Configuration;

namespace com.bitscopic.hilleman.core.dao.vista
{
    public class CrrudDaoFactory
    {
        public ICrrudDao getCrrudDao(IVistaConnection cxn)
        {
            if (cxn.getSource().type == domain.SourceSystemType.VISTA_CRUD_REST_SVC)
            {
                return new http.VistaHttpRestDao(cxn);
            }
            else if (cxn.getSource().type == domain.SourceSystemType.VISTA_RPC_BROKER)
            {
                return new vista.rpc.VistaRpcCrrudDao(cxn);
            }
            else if (cxn.getSource().type == SourceSystemType.SQLITE_CACHE)
            {
                return new dao.sql.sqlite.VistaSqliteCacheDao(cxn.getSource().connectionString);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Instantiate a new ICrrudDao based off the type of SourceSystem. Instantiates a new connection for the ICrrudDao implementation
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public ICrrudDao getCrrudDao(SourceSystem source)
        {
            if (source.type == domain.SourceSystemType.VISTA_CRUD_REST_SVC)
            {
                return new http.VistaHttpRestDao(new VistaHttpRestConnection(source));
            }
            else if (source.type == domain.SourceSystemType.VISTA_RPC_BROKER)
            {
                return new vista.rpc.VistaRpcCrrudDao(new VistaRpcConnection(source));
            }
            else if (source.type == SourceSystemType.SQLITE_CACHE)
            {
                return new dao.sql.sqlite.VistaSqliteCacheDao(source.connectionString);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}