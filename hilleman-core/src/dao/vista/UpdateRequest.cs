using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.dao.vista.rpc;

namespace com.bitscopic.hilleman.core.dao
{
    public class UpdateRequest : BaseCrrudRequest
    {
        Dictionary<String, String> _requestDict = new Dictionary<string, string>();
        Dictionary<String, String> _fieldsAndValues = new Dictionary<string, string>();

        public UpdateRequest(SourceSystem target) : base(target) { }

        public void addFieldAndValue(String fieldNumber, String value)
        {
            if (_fieldsAndValues.ContainsKey(fieldNumber))
            {
                _fieldsAndValues[fieldNumber] = value;
            }
            else
            {
                _fieldsAndValues.Add(fieldNumber, value);
            }
        }

        /// <summary>
        /// Set the target VistA FileMan file for the request
        /// </summary>
        /// <param name="fileNumber"></param>
        public void setFile(String fileNumber)
        {
            if (_requestDict.ContainsKey("file"))
            {
                _requestDict["file"] = fileNumber;
            }
            else
            {
                _requestDict.Add("file", fileNumber);
            }
        }

        public String getFile()
        {
            if (_requestDict.ContainsKey("file"))
            {
                return _requestDict["file"];
            }
            return String.Empty;
        }

        /// <summary>
        /// Set the IENS string for the request
        /// </summary>
        /// <param name="iens"></param>
        public void setIens(String iens)
        {
            if (_requestDict.ContainsKey("iens"))
            {
                _requestDict["iens"] = iens;
            }
            else
            {
                _requestDict.Add("iens", iens);
            }

            // massage IENS string so it contains  trailing commas. up to caller to ensure middle IENS string commas are present
            if (!_requestDict["iens"].EndsWith(","))
            {
                _requestDict["iens"] = String.Concat(_requestDict["iens"], ",");
            }
        }

        public String getIens()
        {
            if (_requestDict.ContainsKey("iens"))
            {
                return _requestDict["iens"];
            }
            return String.Empty;
        }

        internal string buildRequest()
        {
            if (base.getSource().type == SourceSystemType.VISTA_CRUD_REST_SVC)
            {
                return buildRestHttpUpdateRequest();
            }
            else if (base.getSource().type == SourceSystemType.VISTA_RPC_BROKER)
            {
                return buildRpcUpdateRequest();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private string buildRpcUpdateRequest()
        {
            if (!_requestDict.ContainsKey("file") || String.IsNullOrEmpty(_requestDict["file"]) 
                ||!_requestDict.ContainsKey("iens") || String.IsNullOrEmpty(_requestDict["iens"]))
            {
                throw new ArgumentException("Must supply file and iens for update");
            }

            VistaRpcQuery rpc = new VistaRpcQuery("DDR FILER");
            rpc.addParameter(new VistaRpcParameter(VistaRpcParameterType.LITERAL, "UPDATE"));

            int index = 0;
            //ddr.Args = new String[fieldsAndValues.Count];
            Dictionary<String, String> dictForRpc = new Dictionary<string, string>();
            foreach (String key in _fieldsAndValues.Keys)
            {
                dictForRpc.Add((++index).ToString(), String.Format("{0}^{1}^{2}^{3}", _requestDict["file"], key, _requestDict["iens"], _fieldsAndValues[key]));
                //                vistaFile + "^" + key + "^" + ien + "^" + fieldsAndValues[key]; // e.g. [0]: 2^.01^5,^PATIENT,NEW  [1]: 2^.09^5,^222113333
            }
            rpc.addParameter(new VistaRpcParameter(VistaRpcParameterType.LIST, dictForRpc));

            return rpc.buildMessage();
        }

        private string buildRestHttpUpdateRequest()
        {
            String fieldsAndValsPiece = Newtonsoft.Json.JsonConvert.SerializeObject(_fieldsAndValues);
            return "{ \"File\":\"" + _requestDict["file"] + "\", \"Iens\":\"" + _requestDict["iens"] + "\",\"FieldsAndValues\":" + fieldsAndValsPiece + " }";
        }
    }
}