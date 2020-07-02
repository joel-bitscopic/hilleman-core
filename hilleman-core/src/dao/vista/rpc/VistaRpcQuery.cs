using System;
using System.Collections.Generic;
using System.Text;

namespace com.bitscopic.hilleman.core.dao.vista.rpc
{
    public class VistaRpcQuery
    {
        internal String _rpcName;
        IList<VistaRpcParameter> _params = new List<VistaRpcParameter>();

        public VistaRpcQuery(String rpcName)
        {
            _rpcName = rpcName;
        }

        public VistaRpcQuery addParameter(VistaRpcParameter newParam)
        {
            _params.Add(newParam);
            return this;
        }

        internal string buildMessage()
        {
            const string PREFIX = "[XWB]";
            const int COUNT_WIDTH = 3;
            const string RPC_VERSION = "1.108";

            StringBuilder sParams = new StringBuilder();
            sParams.Append("5");

            for (int i = 0; i < _params.Count; i++)
            {
                VistaRpcParameter vp = _params[i];
                //int pType = vp.Type;
                if (vp.getType() == VistaRpcParameterType.LITERAL)
                {
                    sParams.Append('0');
                    sParams.Append(VistaRpcStringUtils.LPack((String)vp.getValue(), COUNT_WIDTH));
                    sParams.Append('f');
                }
                else if (vp.getType() == VistaRpcParameterType.REFERENCE)
                {
                    sParams.Append('1');
                    sParams.Append(VistaRpcStringUtils.LPack((String)vp.getValue(), COUNT_WIDTH));
                    sParams.Append('f');
                }
                else if (vp.getType() == VistaRpcParameterType.LIST)
                {
                    sParams.Append('2');
                    sParams.Append(VistaRpcStringUtils.convertListToString((Dictionary<String, String>)vp.getValue()));
                }
            }
            string msg = "";


            // translated/copied from the Delphi broker code
            if (sParams.ToString() == "5")
            {
                sParams.Append("4f");
            }

            msg = PREFIX + "11" + Convert.ToString(COUNT_WIDTH) + "02" + VistaRpcStringUtils.SPack(RPC_VERSION) +
                VistaRpcStringUtils.SPack(_rpcName) + sParams + '\x0004';
            return msg;
        }
    }
}