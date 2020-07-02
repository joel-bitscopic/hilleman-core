using System;

namespace com.bitscopic.hilleman.core.dao.vista
{
    public class ToolsDaoFactory
    {
        public ToolsDaoFactory() { }

        public IToolsDao getToolsDao(IVistaConnection cxn)
        {
            if (cxn.getSource().type == domain.SourceSystemType.VISTA_RPC_BROKER)
            {
                return new com.bitscopic.hilleman.core.dao.vista.rpc.VistaRpcToolsDao(cxn);
            }
            else
            {
                throw new NotImplementedException("Currently only broker connections supported");
            }
        }
    }
}