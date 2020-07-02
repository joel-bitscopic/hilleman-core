using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.domain.exception;
using com.bitscopic.hilleman.core.utils;
using com.bitscopic.hilleman.core.domain.vista;
using System.Text;
using System.Linq;

namespace com.bitscopic.hilleman.core.dao.vista.rpc
{
    public class VistaRpcToolsDao : IToolsDao
    {
        VistaRpcConnection _cxn;

        public VistaRpcToolsDao(IVistaConnection connection)
        {
            _cxn = (VistaRpcConnection)connection;
        }

        internal String serializeSymbolTable()
        {
            VistaRpcQuery qry = new VistaRpcQuery("BTP SST");
            return (String)_cxn.query(qry);
        }

        internal String deserializeSymbolTable(string sst)
        {
            VistaRpcQuery qry = new VistaRpcQuery("BTP DST");
            qry.addParameter(new VistaRpcParameter(VistaRpcParameterType.LITERAL, sst));
            return (String)_cxn.query(qry);
        }

        internal String deserializeSymbolTable(Dictionary<String, String> symbolTable)
        {
            StringBuilder sb = new StringBuilder();
            foreach (String key in symbolTable.Keys)
            {
                sb.Append(key);
                sb.Append("\x1f");
                sb.Append(symbolTable[key]);
                sb.Append("\x1e");
            }
            sb.Remove(sb.Length - ("\x1e").Length, 1); // remove last RS character

            return this.deserializeSymbolTable(sb.ToString());
        }

        public String gvv(String arg)
        {
            VistaRpcQuery qry = new VistaRpcQuery("XWB GET VARIABLE VALUE");
            qry.addParameter(new VistaRpcParameter(VistaRpcParameterType.REFERENCE, arg));
            return (String)_cxn.query(qry);
        }

        /// <summary>
        /// WARNING: DON'T USE UNLESS YOU KNOW WHAT YOU'RE DOING!!! CAN IRREPARABLY SCREW UP YOUR VISTA!!
        /// </summary>
        /// <param name="global">The global to set</param>
        /// <param name="globalValue">The value for the global</param>
        public void setGlobal(String globalKey, String globalValue)
        {
            ReadRangeRequest injector = new ReadRangeRequest(_cxn.getSource());
            injector.setFile("200");
            injector.setFlags("IP");
            injector.setMax("1");
            injector.setFields(".01");
            injector.setIdentifierParam(String.Format("SET {0}=\"{1}\"", globalKey, globalValue));
            new CrrudDaoFactory().getCrrudDao(_cxn).readRange(injector);
        }

        /// <summary>
        /// WARNING: DON'T USE UNLESS YOU KNOW WHAT YOU'RE DOING!!! CAN IRREPARABLY SCREW UP YOUR VISTA!!
        /// </summary>
        /// <param name="global">The global to kill</param>
        public void killGlobal(String globalKey)
        {
            ReadRangeRequest injector = new ReadRangeRequest(_cxn.getSource());
            injector.setFile("200");
            injector.setFlags("IP");
            injector.setMax("1");
            injector.setFields(".01");
            injector.setIdentifierParam(String.Format("KILL {0}", globalKey));
            new CrrudDaoFactory().getCrrudDao(_cxn).readRange(injector);
        }

        /// <summary>
        /// WARNING: DON'T USE UNLESS YOU KNOW WHAT YOU'RE DOING!!! CAN IRREPARABLY SCREW UP YOUR VISTA!!
        /// </summary>
        /// <param name="global">The global to kill</param>
        public void run(String m)
        {
            ReadRangeRequest injector = new ReadRangeRequest(_cxn.getSource());
            injector.setFile("200");
            injector.setFlags("IP");
            injector.setMax("1");
            injector.setFields(".01");
            injector.setIdentifierParam(m);
            new CrrudDaoFactory().getCrrudDao(_cxn).readRange(injector);
        }

        public void heartbeat()
        {
            _cxn.query(new VistaRpcQuery("XWB IM HERE"));
        }

        public String getWelcomeMessage()
        {
            return (String)_cxn.query(new VistaRpcQuery("XUS INTRO MSG"));
        }

        public String getVistaSystemTime()
        {
            VistaRpcQuery qry = new VistaRpcQuery("ORWU DT");
            qry.addParameter(new VistaRpcParameter(VistaRpcParameterType.LITERAL, "NOW"));
            return (String)_cxn.query(qry);
        }

        public FmFile getFileDetails(String vistaFileNumber, bool fetchFields = true)
        {
            FmFile file = new FmFile();
            
            Decimal trash = 0;
            if (!Decimal.TryParse(vistaFileNumber, out trash))
            {
                throw new ArgumentException("Not a valid VistA file number");
            }

            string fileNameQuery = String.Format("$P($G(^DIC({0},0)),U,1)", vistaFileNumber);
            string fileGlobalQuery = String.Format("$G(^DIC({0},0,\"GL\"))", vistaFileNumber);
            string combinedQueries = String.Concat(fileNameQuery, "_\"~|~\"_", fileGlobalQuery);
            string combinedResult = this.gvv(combinedQueries);
            string[] pieces = StringUtils.split(combinedResult, "~|~");
            file.name = pieces[0];
            file.global = pieces[1];
            file.number = vistaFileNumber;

            if (fetchFields)
            {
                file.fields = this.getFields(vistaFileNumber).Values.ToList();
            }

            return file;
        }

        public Dictionary<String, String> getVistaFieldSetValues(String vistaFileNumber, String vistaFieldNumber)
        {
            FmField field = this.getField(vistaFileNumber, vistaFieldNumber);
            if (field.dataType != FmFieldType.SET_OF_CODES)
            {
                throw new ArgumentException(String.Format("Field {0} from file {1} is not a 'set of codes' data type", vistaFieldNumber, vistaFileNumber));
            }
            return field.setValues;
        }

        public Dictionary<string, FmField> getFields(String vistaFileNumber)
        {
            Dictionary<string, FmField> result = new Dictionary<string, FmField>();

            string currentFieldNo = this.gvv(String.Format("$O(^DD({0},0))", vistaFileNumber)); // get first field #
            while (!String.IsNullOrEmpty(currentFieldNo))
            {
                FmField currentField = this.getField(vistaFileNumber, currentFieldNo, ref currentFieldNo); // passed by reference because RPC below fetches next field name for loop
                if (!result.ContainsKey(currentField.number))
                {
                    result.Add(currentField.number, currentField);
                }
            }

            return result;
        }

        public FmField getField(String vistaFileNumber, String vistaFieldNumber)
        {
            String trash = "";
            return this.getField(vistaFileNumber, vistaFieldNumber, ref trash);
        }

        public FmField getField(String vistaFileNumber, String vistaFieldNumber, ref String nextField)
        {
            FmField result = new FmField() { number = vistaFieldNumber };

            string combinedQuery = String.Format("$O(^DD({0},{1}))_\"~~^~~\"_$G(^DD({0},{1},0))", vistaFileNumber, vistaFieldNumber);

            //string combinedQuery = String.Format("$G(^DD({0},{1},0)),0))", vistaFileNumber, vistaFieldNumber);
            string combinedResults = this.gvv(combinedQuery);
            String[] combinedPieces = StringUtils.split(combinedResults, "~~^~~");
            nextField = StringUtils.isNumeric(combinedPieces[0]) ? combinedPieces[0] : "";
            string[] pieces = StringUtils.split(combinedPieces[1], "^");

        //    if (vistaFieldNumber == ".033")
        //    {
        //        System.Console.WriteLine(combinedResults);
        //    }

            result.name = pieces[0];
            result.dataTypeCode = pieces[1];
            result = setPropsForCodes(result, pieces[1]);
            result.nodePiece = pieces[3];

          /*  if (result.dataType == FmFieldType.POINTER && String.IsNullOrEmpty(pieces[2]))
            {
                System.Console.WriteLine("Field type is pointer but no pointer to def: " + combinedResults);
            }
           */

            if (result.dataType == FmFieldType.POINTER && !String.IsNullOrEmpty(pieces[2]))
            {
                string pointedToFileHeader = this.gvv(String.Format("$G(^{0}0))", pieces[2])); // need to put carat back in because string split removes it
                if (!String.IsNullOrEmpty(pointedToFileHeader))
                {
                    result.pointsTo = new List<FmFile>();
                    string[] pointedToFileHeaderPieces = StringUtils.split(pointedToFileHeader, StringUtils.CARAT);
                    result.pointsTo.Add(new FmFile()
                    {
                        global = "^" + pieces[2], // need to put carat back in because string split removes it
                        name = pointedToFileHeaderPieces[0],
                        number = StringUtils.extractNumeric(pointedToFileHeaderPieces[1])
                    });
                }
            }
            else if (result.dataType == FmFieldType.SET_OF_CODES)
            {
                if (!String.IsNullOrEmpty(pieces[2]))
                {
                    result.setValues = new Dictionary<string, string>();
                    String[] externalsKeysAndVals = pieces[2].Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (String individual in externalsKeysAndVals)
                    {
                        String[] keyAndVal = individual.Split(new String[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                        if (!result.setValues.ContainsKey(keyAndVal[0]))
                        {
                            result.setValues.Add(keyAndVal[0], keyAndVal[1]);
                        }
                    }
                }
            }

            // re-assemble transform
            if (pieces.Length > 4)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 4; i < pieces.Length; i++)
                {
                    sb.Append(pieces[i]);
                    sb.Append("^");
                }
                sb = sb.Remove(sb.Length - 1, 1);
                if (result.dataType != FmFieldType.COMPUTED) // computed fields don't have input transform!
                {
                    result.inputTransform = sb.ToString();
                }
            }

            return result;
        }

        // see: http://www.hardhats.org/fileman/u2/fa_cond.htm
        internal FmField setPropsForCodes(FmField result, string props)
        {
            //System.Console.WriteLine(result.number + ": " + props);
            if (props.Contains("R"))
            {
                result.required = true;
            }
            if (props.Contains("a"))
            {
                result.alwaysAudited = true;
            }
            if (props.Contains("e"))
            {
                result.editDeleteAudited = true;
            }

            // if the props start with a number then some type of multiple
            if (StringUtils.startsWithNumeric(props))
            {
                result.pointsTo = new List<FmFile>() { new FmFile() { number = StringUtils.extractNumeric(props).ToString() } };
                if (props.Contains("P"))
                {
                    result.dataType = FmFieldType.POINTER_MULTIPLE;
                }
                else if (props.Contains("D"))
                {
                    result.dataType = FmFieldType.DATE_MULTIPLE;
                }
                else if (props.Contains("S"))
                {
                    result.dataType = FmFieldType.SET_MULTIPLE;
                }
                else
                {
                    result.dataType = FmFieldType.MULTIPLE;
                }
                return result;
            }


            if (props.Contains("W"))
            {
                result.dataType = FmFieldType.WORD_PROCESSING;
            }
            if (props.Contains("V"))
            {
                result.dataType = FmFieldType.VARIABLE_POINTER;
            }
            if (props.Contains("S"))
            {
                result.dataType = FmFieldType.SET_OF_CODES;
            }
            if (props.Contains("P"))
            {
                String numeric = StringUtils.extractNumeric(props);
                result.dataType = FmFieldType.POINTER;
                result.pointsTo = new List<FmFile>() { new FmFile() { number = numeric.ToString() } };
            }
            if (props.Contains("N"))
            {
                result.dataType = FmFieldType.NUMERIC;
            }
            if (props.Contains("K"))
            {
                result.dataType = FmFieldType.MUMPS;
            }
            if (props.Contains("F"))
            {
                result.dataType = FmFieldType.FREE_TEXT;
            }
            if (props.Contains("D"))
            {
                result.dataType = FmFieldType.DATE_TIME;
            }
            if (props.Contains("C"))
            {
                result.dataType = FmFieldType.COMPUTED;
            }


            return result;
        }
    }
}