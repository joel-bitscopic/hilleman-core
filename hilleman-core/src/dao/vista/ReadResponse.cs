using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.utils;
using System.Text;
using com.bitscopic.hilleman.core.domain.to;
using System.Linq;

namespace com.bitscopic.hilleman.core.dao
{
    public class ReadResponse : BaseCrrudResponse
    {
        public ReadResponse() { }

        public static ReadResponse parseReadResponse(ReadRequest request, String response)
        {
            if (request.getSource().type == domain.SourceSystemType.VISTA_CRUD_REST_SVC)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ReadResponse>(response);
            }
            else if (request.getSource().type == domain.SourceSystemType.VISTA_RPC_BROKER)
            {
                return new ReadResponse() { value = parseDdrGetsEntryDataResponse(response) };
            }
            else
            {
                throw new NotImplementedException("Unable to parse that read response type");
            }
        }

        public static IList<String> parseDdrGetsEntryDataResponse(String response)
        {
            return StringUtils.splitToList(response, StringUtils.CRLF_ARY, StringSplitOptions.RemoveEmptyEntries);
        }

        public Dictionary<String, String> convertedResponseInternal = null;
        public Dictionary<String, String> convertResponseToInternalDict()
        {
            if (this.convertedResponseInternal != null)
            {
                return this.convertedResponseInternal;
            }

            this.convertedResponseInternal = new Dictionary<string, string>();

            if (this.value == null || this.value.Count == 0)
            {
                return this.convertedResponseInternal;
            }

            // get IEN and add to result
            foreach (String line in this.value)
            {
                String[] pieces = StringUtils.split(line, StringUtils.CARAT_ARY, StringSplitOptions.RemoveEmptyEntries);
                if (pieces.Length < 5)
                {
                    continue;
                }
                this.convertedResponseInternal.Add("IEN", pieces[1]);
                break;
            }

            StringBuilder wpValue = new StringBuilder();
            String currentField = "";
            bool inWP = false;
            foreach (String line in this.value)
            {
                if (inWP)
                {
                    if (String.Equals(line, "$$END$$"))
                    {
                        inWP = false;
                        if (!this.convertedResponseInternal.ContainsKey(currentField))
                        {
                            this.convertedResponseInternal.Add(currentField, wpValue.ToString());
                        }
                        currentField = "";
                        continue;
                    }
                    else
                    {
                        wpValue.AppendLine(line);
                        continue;
                    }
                }
                String[] pieces = StringUtils.split(line, StringUtils.CARAT_ARY, StringSplitOptions.RemoveEmptyEntries);
                if (pieces.Length > 2 && String.Equals(pieces[3], "[WORD PROCESSING]"))
                {
                    inWP = true;
                    currentField = pieces[2];
                    wpValue = new StringBuilder();
                    continue;
                }

                if (pieces.Length < 4)
                {
                    continue;
                }

                if (!this.convertedResponseInternal.ContainsKey(pieces[2]))
                {
                    this.convertedResponseInternal.Add(pieces[2], pieces[3]);
                }
            }

            return this.convertedResponseInternal;
        }

        public Dictionary<String, String> convertedResponseExternal = null;
        public Dictionary<String, String> convertResponseToExternalDict()
        {
            if (this.convertedResponseExternal != null)
            {
                return this.convertedResponseExternal;
            }

            this.convertedResponseExternal = new Dictionary<string, string>();

            if (this.value == null || this.value.Count == 0)
            {
                return this.convertedResponseExternal;
            }

            // get IEN and add to result
            foreach (String line in this.value)
            {
                String[] pieces = StringUtils.split(line, StringUtils.CARAT_ARY, StringSplitOptions.RemoveEmptyEntries);
                if (pieces.Length < 5)
                {
                    continue;
                }
                this.convertedResponseExternal.Add("IEN", pieces[1]);
                break;
            }

            foreach (String line in this.value)
            {
                String[] pieces = StringUtils.split(line, StringUtils.CARAT_ARY, StringSplitOptions.RemoveEmptyEntries);
                if (pieces.Length < 5 || this.convertedResponseExternal.ContainsKey(pieces[2]))
                {
                    continue;
                }
                if (String.IsNullOrEmpty(pieces[4])) // if external value was empty, fall back to internal
                {
                    this.convertedResponseExternal.Add(pieces[2], pieces[3]);
                }
                else
                {
                    this.convertedResponseExternal.Add(pieces[2], pieces[4]);
                }
            }

            return this.convertedResponseExternal;
        }

        public List<VistaRecord> getRecordsForFile(String fileNumber)
        {
            if (this.value == null || this.value.Count == 0)
            {
                return null;
            }

            // first build a dictionary w/ ONLY the records for the given file number (organized by field number)
            // NOTE: VERY important - there may be > 1 subfile record in results so we must organize by IENS
            Dictionary<String, Dictionary<String, String[]>> piecesByIensThenFieldForFile = new Dictionary<string, Dictionary<string, string[]>>();
            bool inWPField = false;
            String currentField = null;
            String currentIensForWP = null;
            StringBuilder wpValue = new StringBuilder();
            foreach (String line in this.value)
            {
                if (inWPField)
                {
                    if (String.Equals(line, "$$END$$"))
                    {
                        inWPField = false;
                        // reconstruct line so it appears the whole WP field was on that single line - do this just to simplify code below!
                        piecesByIensThenFieldForFile[currentIensForWP].Add(currentField, new String[] { fileNumber, currentIensForWP, currentField, wpValue.ToString() });
                        currentField = "";
                        continue;
                    }
                    else
                    {
                        wpValue.AppendLine(line);
                        continue;
                    }
                }

                String[] pieces = StringUtils.split(line, StringUtils.CARAT);
                if (pieces.Length < 4 || !String.Equals(pieces[0], fileNumber))
                {
                    continue;
                }

                String iens = pieces[1];
                currentIensForWP = iens;
                if (!piecesByIensThenFieldForFile.ContainsKey(iens))
                {
                    piecesByIensThenFieldForFile.Add(iens, new Dictionary<string, string[]>());
                }


                if (String.Equals(pieces[3], "[WORD PROCESSING]"))
                {
                    inWPField = true;
                    currentField = pieces[2];
                    wpValue = new StringBuilder();
                    continue;
                }


                if (!piecesByIensThenFieldForFile[iens].ContainsKey(pieces[2]))
                {
                    piecesByIensThenFieldForFile[iens].Add(pieces[2], pieces);
                }
            }

            Dictionary<String, VistaRecord> recordsByIens = new Dictionary<string, VistaRecord>();
            foreach (String iens in piecesByIensThenFieldForFile.Keys)
            {
                if (!recordsByIens.ContainsKey(iens))
                {
                    recordsByIens.Add(iens, new VistaRecord() { fields = new List<VistaField>(), file = new VistaFile() { number = fileNumber }, iens = iens });
                }

                Dictionary<String, String[]> piecesByFieldNo = piecesByIensThenFieldForFile[iens];
                foreach (String fieldNo in piecesByFieldNo.Keys)
                {
                    String[] piecesFromDict = piecesByFieldNo[fieldNo];
                    if (piecesFromDict.Length < 4)
                    {
                        continue;
                    }

                    if (piecesFromDict.Length > 4)
                    {
                        if (piecesFromDict.Length > 5) // for LEDI in particular, found some fields that contained '^' - join all the split pieces back together!
                        {
                            recordsByIens[iens].fields.Add(new VistaField() { number = piecesFromDict[2], value = StringUtils.join(piecesFromDict, "^", 3, piecesFromDict.Length) });
                        }
                        else
                        {
                            recordsByIens[iens].fields.Add(new VistaField() { number = piecesFromDict[2], value = piecesFromDict[3], externalValue = piecesFromDict[4] });
                        }
                    }
                    else
                    {
                        recordsByIens[iens].fields.Add(new VistaField() { number = piecesFromDict[2], value = piecesFromDict[3] });
                    }
                }
            }


            return recordsByIens.Values.ToList();
        }


    }

    public class ReadResponseInternalExternalDict
    {
        Dictionary<String, String> internalDict;
        Dictionary<String, String> externalDict;

        public ReadResponseInternalExternalDict() { }

        public ReadResponseInternalExternalDict(Dictionary<String, String> internalDict, Dictionary<String, String> externalDict)
        {
            this.internalDict = internalDict;
            this.externalDict = externalDict;
        }

        String safeGetInternalValue(String key)
        {
            if (internalDict.ContainsKey(key))
            {
                return internalDict[key];
            }
            else
            {
                return null;
            }
        }

        String safeGetExternalValue(String key)
        {
            if (externalDict.ContainsKey(key))
            {
                return externalDict[key];
            }
            else
            {
                return null;
            }
        }
    }
}