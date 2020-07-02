using System;
using com.bitscopic.hilleman.core.domain;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.dao.vista.rpc;
using System.Text;
using com.bitscopic.hilleman.core.utils;
using com.bitscopic.hilleman.core.domain.to;

namespace com.bitscopic.hilleman.core.dao
{
    public class CreateRequest : BaseCrrudRequest
    {
        public const String FILER_FIND_OR_CREATE = "?+";
        public const String FILER_CREATE = "+";
        public const String FILER_FIND = "?";

        bool _useExactIens = false;
        Dictionary<String, String> _requestDict = new Dictionary<string,string>();
        Dictionary<String, String> _fieldsAndValues = new Dictionary<string,string>();

        public CreateRequest(SourceSystem target) : base(target) { }

        public CreateRequest(SourceSystem target, VistaRecord record) : base(target) 
        {
            setFile(record.file.number);
            if (record.exactIens)
            {
                setExactIens(true);
            }
            setIens(record.iens);

            foreach (VistaField field in record.fields)
            {
                addFieldAndValue(field.number, field.value);
            }
        }

        public void setExactIens(bool tf)
        {
            _useExactIens = tf;
        }

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

        /// <summary>
        /// Serialize the request in to a JSON string
        /// </summary>
        /// <returns></returns>
        public object buildRequest()
        {
            if (base.getSource().type == SourceSystemType.VISTA_CRUD_REST_SVC)
            {
                return buildRestHttpPutRequestBody();
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

        public String buildRestHttpPutRequestBody()
        {
            String fieldsAndValsPiece = Newtonsoft.Json.JsonConvert.SerializeObject(_fieldsAndValues);
            return "{ \"File\":\"" + _requestDict["file"] + "\", \"FieldsAndValues\":" + fieldsAndValsPiece + " }";
        }

        public String buildDdrFilerRequest()
        {
            if (!_requestDict.ContainsKey("file") || String.IsNullOrEmpty(_requestDict["file"]))
            {
                throw new ArgumentException("Must supply file for create");
            }

            VistaRpcQuery rpc = new VistaRpcQuery("DDR FILER");
            rpc.addParameter(new VistaRpcParameter(VistaRpcParameterType.LITERAL, "ADD"));

            bool hasIens = (_requestDict.ContainsKey("iens") && !String.IsNullOrEmpty(_requestDict["iens"]));
            int index = 0;
            String levelStr = "+1";
            Dictionary<String, String> dictForRpc = new Dictionary<string, string>();

            // BEGIN SPECIFYING SUBFILE IEN LOGIC (mostly)
            // NOTE: this requires mod to DDR FILER (FILEC^DDR3) routine => I $D(DDRDATA("IENs")) M DDRIENS=DDRDATA("IENs") ; JAM 1/13/15 - added to support passing IENS
            if (_useExactIens) // e.g. ?+3151231.1030,92,
            {
                String exactIensStr = _requestDict["iens"]; // e.g. ?+3151231.1030,92,
                String firstPiece = StringUtils.piece(exactIensStr, ",", 1); // e.g. ?+3151231.1030
                String ienModifiers = StringUtils.extractNonNumeric(exactIensStr); // e.g. ?+
                Int32 level = StringUtils.count(exactIensStr, ','); // e.g. 2 (two commas but nothing after last)
                levelStr = ienModifiers + level.ToString(); // e.g. ?+2 
                dictForRpc.Add("\"IENs\"," + level.ToString(), StringUtils.extractNumeric(firstPiece)); // e.g. "IENS",2=3151231.1030
                
                if (level == 1)
                {
                    levelStr = String.Concat("+", firstPiece);
                }

                // finally need to adjust IENS so that formatters below will build correctly
                _requestDict["iens"] = exactIensStr.Replace(firstPiece, ""); // just take away the first piece with a simple replace!
                if (_requestDict["iens"].StartsWith(","))
                {
                    _requestDict["iens"] = _requestDict["iens"].Substring(1);
                }
            } // continue on after adding this parameter for specific IENS
            // END LOGIC (mostly)

            foreach (String key in _fieldsAndValues.Keys)
            {
                if (!hasIens)
                {
                    dictForRpc.Add((++index).ToString(), String.Format("{0}^{1}^{2},^{3}", _requestDict["file"], key, levelStr, _fieldsAndValues[key]));
                    //ddr.Args[index++] = vistaFile + "^" + key + "^+1,^" + fieldsAndValues[key]; // e.g. [0]: 2^.01^+1,PATIENT,NEW^DDROOT(1)  [1]: 2^.09^+1,^222113333
                }
                else
                {
                    //dictForRpc.Add("1", "44.001^.01^?+2,8,^3150113.1115");
                    dictForRpc.Add((++index).ToString(), String.Format("{0}^{1}^{2},{3}^{4}", _requestDict["file"], key, levelStr, _requestDict["iens"], _fieldsAndValues[key]));
                    //ddr.Args[index++] = vistaFile + "^" + key + "^+1," + iens + "^" + fieldsAndValues[key]; // e.g. [0]: 2^.01^+1,PATIENT,NEW  [1]: 2^.09^+1,^222113333
                }
            }
            rpc.addParameter(new VistaRpcParameter(VistaRpcParameterType.LIST, dictForRpc));

            return rpc.buildMessage();
        }
    }
}