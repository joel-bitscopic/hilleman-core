using System;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.dao.vista.rpc;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.dao
{
    public class DeleteRequest : BaseCrrudRequest
    {
        String _file;
        String _iens;

        public DeleteRequest(SourceSystem target) : base(target) { }

        public void setFile(String file)
        {
            _file = file;
        }
        public String getFile()
        {
            return _file;
        }

        public void setIens(String iens)
        {
            _iens = iens;
        }
        public String getIens()
        {
            return _iens;
        }

        public String buildRequest()
        {
            if (base.getSource().type == SourceSystemType.VISTA_CRUD_REST_SVC)
            {
                return String.Format("{0}/{1}", _file, _iens);
            }
            else if (base.getSource().type == SourceSystemType.VISTA_RPC_BROKER)
            {
                return buildDdrFilerRequest();
            }
            else
            {
                throw new NotImplementedException(
                    String.Format("The {0} source system type is not currently supported",
                    Enum.GetName(typeof(SourceSystemType), base.getSource().type)));
            }
        }

        private string buildDdrFilerRequest()
        {
            if (String.IsNullOrEmpty(_file) || String.IsNullOrEmpty(_iens))
            {
                throw new ArgumentException("Must supply file for create");
            }

            VistaRpcQuery rpc = new VistaRpcQuery("DDR FILER");
            rpc.addParameter(new VistaRpcParameter(VistaRpcParameterType.LITERAL, "EDIT"));

            Dictionary<String, String> dictForRpc = new Dictionary<string, string>();
            // vistaFile + "^.01^" + recordIen + "^@" // per API docs, setting .01 field to "@" deletes record
            dictForRpc.Add("1", String.Format("{0}^.01^{1}^@", _file, _iens));
            rpc.addParameter(new VistaRpcParameter(VistaRpcParameterType.LIST, dictForRpc));

            return rpc.buildMessage();
        }
    }
}