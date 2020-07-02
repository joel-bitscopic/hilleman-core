using System;
using System.Collections.Generic;
using System.Text;
using com.bitscopic.hilleman.core.dao.vista.rpc;
using com.bitscopic.hilleman.core.domain;

namespace com.bitscopic.hilleman.core.dao
{
    public class ReadRangeRequest : BaseCrrudRequest
    {
        public String apiName;
        Dictionary<String, String> _requestDict = new Dictionary<string,string>();

        public ReadRangeRequest(SourceSystem target) : base(target) { }

        public ReadRangeRequest(SourceSystem target, com.bitscopic.hilleman.core.domain.to.ReadRange request) : base(target)
        {
            setFile(request.file);
            setFields(request.fields);
            setIens(request.iens);
            setFlags(request.flags);
            setCrossRef(request.xref);
            setMax(request.maxRex);
            setFrom(request.from);
            setScreenParam(request.screen);
            setIdentifierParam(request.identifier);
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

        public String getFile() { return _requestDict["file"]; }

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

            // massage IENS string so it contains leading and trailing commas. up to caller to ensure middle IENS string commas are present
            if (!_requestDict["iens"].StartsWith(","))
            {
                _requestDict["iens"] = String.Concat(",", _requestDict["iens"]);
            }
            if (!_requestDict["iens"].EndsWith(","))
            {
                _requestDict["iens"] = String.Concat(_requestDict["iens"], ",");
            }
        }

        public String getIens() { return (_requestDict.ContainsKey("iens") && !String.IsNullOrEmpty(_requestDict["iens"]) ? _requestDict["iens"] : String.Empty); }

        /// <summary>
        /// Set the VistA FileMan file cross reference to use for traversing the records in the file
        /// </summary>
        /// <param name="crossRef"></param>
        public void setCrossRef(String crossRef)
        {
            if (_requestDict.ContainsKey("xref"))
            {
                _requestDict["xref"] = crossRef;
            }
            else
            {
                _requestDict.Add("xref", crossRef);
            }
        }

        public String getCrossRef() { return (_requestDict.ContainsKey("xref") && !String.IsNullOrEmpty(_requestDict["xref"]) ? _requestDict["xref"] : String.Empty); }

        /// <summary>
        /// Set the VistA FileMan file identifier code
        /// </summary>
        /// <param name="identifier"></param>
        public void setIdentifierParam(String identifier)
        {
            if (_requestDict.ContainsKey("identifier"))
            {
                _requestDict["identifier"] = identifier;
            }
            else
            {
                _requestDict.Add("identifier", identifier);
            }
        }

        /// <summary>
        /// Set the VistA FileMan file screen code
        /// </summary>
        /// <param name="screen"></param>
        public void setScreenParam(String screen)
        {
            if (_requestDict.ContainsKey("screen"))
            {
                _requestDict["screen"] = screen;
            }
            else
            {
                _requestDict.Add("screen", screen);
            }
        }

        /// <summary>
        /// Set the flags to use (typically: I, P, B)
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

        public String getFlags()
        {
            if (_requestDict.ContainsKey("flags"))
            {
                return _requestDict["flags"];
            }
            return String.Empty;
        }

        /// <summary>
        /// Set the max number of records to return (as a string)
        /// </summary>
        /// <param name="maxRex"></param>
        public void setMax(String maxRex)
        {
            if (_requestDict.ContainsKey("maxRex"))
            {
                _requestDict["maxRex"] = maxRex;
            }
            else
            {
                _requestDict.Add("maxRex", maxRex);
            }
        }

        public String getMax() { return (_requestDict.ContainsKey("maxRex") && !String.IsNullOrEmpty(_requestDict["maxRex"]) ? _requestDict["maxRex"] : String.Empty); }

        /// <summary>
        /// Set the VistA FileMan file start parameter to use for traversing the records in the file
        /// </summary>
        /// <param name="from"></param>
        public void setFrom(String from)
        {
            if (_requestDict.ContainsKey("from"))
            {
                _requestDict["from"] = from;
            }
            else
            {
                _requestDict.Add("from", from);
            }
        }

        public String getFrom() { return (_requestDict.ContainsKey("from") && !String.IsNullOrEmpty(_requestDict["from"]) ? _requestDict["from"] : String.Empty); }

        /// <summary>
        /// Set the VistA FileMan file field numbers to return
        /// </summary>
        /// <param name="fieldNumbersString"></param>
        public void setFields(IList<String> fieldNumbers)
        {
            if (fieldNumbers == null || fieldNumbers.Count == 0)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach (String field in fieldNumbers)
            {
                sb.Append(field);
                sb.Append(";");
            }
            sb.Remove(sb.Length - 1, 1); // remove the last semicolon added

            setFields(sb.ToString());
        }

        /// <summary>
        /// Set the "part" value
        /// </summary>
        /// <param name="part"></param>
        public void setPart(String part)
        {
            if (_requestDict.ContainsKey("part"))
            {
                _requestDict["part"] = part;
            }
            else
            {
                _requestDict.Add("part", part);
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
        /// Build the request string based on the source type
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
                return buildDdrListerRpcString();
            }
            else
            {
                throw new NotImplementedException(
                    String.Format("The {0} source system type is not currently supported",
                    Enum.GetName(typeof(SourceSystemType), base.getSource().type)));
            }
        }

        public String buildDdrListerRpcString()
        { 
            VistaRpcQuery rpc = new VistaRpcQuery("DDR LISTER");
            Dictionary<String, String> ddrArgs = new Dictionary<string,string>();
                
            ddrArgs.Add("\"FILE\"", _requestDict["file"]);

            if (_requestDict.ContainsKey("iens") && !String.IsNullOrEmpty(_requestDict["iens"]))
            {
                ddrArgs.Add("\"IENS\"", _requestDict["iens"]);
            }

            // set a couple defaults
            if (!_requestDict.ContainsKey("flags"))
            {
                _requestDict.Add("flags", "IP");
            }
            if (!_requestDict.ContainsKey("xref"))
            {
                _requestDict.Add("xref", "#");
            }

            ddrArgs.Add("\"FIELDS\"", "@;" + _requestDict["fields"]);
            ddrArgs.Add("\"FLAGS\"", _requestDict["flags"]);

            if (_requestDict.ContainsKey("maxRex") && !String.IsNullOrEmpty(_requestDict["maxRex"]))
            {
                ddrArgs.Add("\"MAX\"", _requestDict["maxRex"]);
            }
            if (_requestDict.ContainsKey("from") && !String.IsNullOrEmpty(_requestDict["from"]))
            {
                ddrArgs.Add("\"FROM\"", _requestDict["from"]);
            }
            if (_requestDict.ContainsKey("part") && !String.IsNullOrEmpty(_requestDict["part"]))
            {
                ddrArgs.Add("\"PART\"", _requestDict["part"]);
            }
            if (_requestDict.ContainsKey("xref") && !String.IsNullOrEmpty(_requestDict["xref"]))
            {
                ddrArgs.Add("\"XREF\"", _requestDict["xref"]);
            }
            if (_requestDict.ContainsKey("screen") && !String.IsNullOrEmpty(_requestDict["screen"]))
            {
                ddrArgs.Add("\"SCREEN\"", _requestDict["screen"]);
            }
            if (_requestDict.ContainsKey("identifier") && !String.IsNullOrEmpty(_requestDict["identifier"]))
            {
                ddrArgs.Add("\"ID\"", _requestDict["identifier"]);
            }
            if (_requestDict.ContainsKey("options") && !String.IsNullOrEmpty(_requestDict["options"]))
            {
                ddrArgs.Add("\"OPTIONS\"", _requestDict["options"]);
            }
            rpc.addParameter(new VistaRpcParameter(VistaRpcParameterType.LIST, ddrArgs));

            return rpc.buildMessage();
        }
    }
}