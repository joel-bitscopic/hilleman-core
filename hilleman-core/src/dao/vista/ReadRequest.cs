using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.domain;
using System.Text;
using com.bitscopic.hilleman.core.dao.vista.rpc;

namespace com.bitscopic.hilleman.core.dao
{
    public class ReadRequest : BaseCrrudRequest
    {
        Dictionary<String, String> _requestDict = new Dictionary<string,string>();

        public ReadRequest(SourceSystem target) : base(target) { }

        public ReadRequest(SourceSystem target, String vistaFile, String iens, String fields, String flags)
            : base(target)
        {
            setFile(vistaFile);
            setIens(iens);
            setFields(fields);
            setFlags(flags);
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

        /// <summary>
        /// Set the flags to use (typically: I, E, N)
        /// </summary>
        /// <param name="flags"></param>
        public void setFlags(String flags)
        {
            if (_requestDict.ContainsKey("flags"))
            {
                _requestDict["flags"] = flags;
            }
            else
            {
                _requestDict.Add("flags", flags);
            }
        }

        /// <summary>
        /// Set the VistA FileMan file field numbers to return (semicolon delimited)
        /// </summary>
        /// <param name="fieldNumbersString"></param>
        public void setFields(String fieldNumbersString)
        {
            if (_requestDict.ContainsKey("fields"))
            {
                _requestDict["fields"] = fieldNumbersString;
            }
            else
            {
                _requestDict.Add("fields", fieldNumbersString);
            }
        }

        public String getFields() { return _requestDict["fields"]; }

        /// <summary>
        /// Serialize the request in to a JSON string
        /// </summary>
        /// <returns></returns>
        public object buildRequest()
        {
            if (base.getSource().type == SourceSystemType.VISTA_CRUD_REST_SVC)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(_requestDict);
            }
            else if (base.getSource().type == SourceSystemType.VISTA_RPC_BROKER)
            {
                return buildDdrGetsEntryDataRequest();
            }
            else
            {
                throw new NotImplementedException(
                    String.Format("The {0} source system type is not currently supported",
                    Enum.GetName(typeof(SourceSystemType), base.getSource().type)));
            }
        }

        public String buildDdrGetsEntryDataRequest()
        {
            if (!_requestDict.ContainsKey("file") || String.IsNullOrEmpty(_requestDict["file"]) ||
                !_requestDict.ContainsKey("iens") || String.IsNullOrEmpty(_requestDict["iens"]))
            {
                throw new ArgumentException("Must supply all arguments for read");
            }

            // set defaults
            if (!_requestDict.ContainsKey("flags"))
            {
                _requestDict.Add("flags", "IEN");
            }
            if (!_requestDict.ContainsKey("fields"))
            {
                _requestDict.Add("fields", "*");
            }


            VistaRpcQuery rpc = new VistaRpcQuery("DDR GETS ENTRY DATA");
            Dictionary<String, String> ddrArgs = new Dictionary<string, string>();

            ddrArgs.Add("\"FILE\"", _requestDict["file"]);
            ddrArgs.Add("\"IENS\"", _requestDict["iens"]);
            ddrArgs.Add("\"FIELDS\"", _requestDict["fields"]);
            ddrArgs.Add("\"FLAGS\"", _requestDict["flags"]);

            rpc.addParameter(new VistaRpcParameter(VistaRpcParameterType.LIST, ddrArgs));

            return rpc.buildMessage();
        }

    }
}