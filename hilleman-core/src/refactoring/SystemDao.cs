using System;
using System.Collections.Generic;
using System.Linq;
using com.bitscopic.hilleman.core.dao.vista;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.dao;
using com.bitscopic.hilleman.core.utils;
using com.bitscopic.hilleman.core.domain.vista;
using com.bitscopic.hilleman.core.dao.vista.rpc;

namespace com.bitscopic.hilleman.core.refactoring
{
    public class SystemDao : IRefactoringApi
    {
        IVistaConnection _cxn;

        public SystemDao(IVistaConnection cxn)
        {
            _cxn = cxn;
        }

        public void setTarget(com.bitscopic.hilleman.core.dao.vista.IVistaConnection target)
        {
            _cxn = target;
        }

        #region Options

        /// <summary>
        /// Fetches Options that have type of 'BROKER'
        /// </summary>
        /// <returns></returns>
        public Dictionary<String, MenuOption> getRpcOptions()
        {
            ReadRangeRequest request = buildGetRpcOptionsRequest();
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toVistaMenuOptions(response);
        }

        internal ReadRangeRequest buildGetRpcOptionsRequest()
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setFile("19");
            request.setFields(".01;3;3.6;4");
            request.setScreenParam("I $P($G(^(0)),U,4)=\"B\"");
            return request;
        }

        /// <summary>
        /// Fetch all records from the OPTION (#19) file
        /// </summary>
        /// <returns></returns>
        public Dictionary<String, MenuOption> getVistaMenuOptions()
        {
            ReadRangeRequest request = buildGetVistaMenuOptionsRequest();
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toVistaMenuOptions(response);
        }

        internal ReadRangeRequest buildGetVistaMenuOptionsRequest()
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setFile("19");
            request.setFields(".01;3;3.6;4");
            return request;
        }

        internal Dictionary<String, MenuOption> toVistaMenuOptions(ReadRangeResponse response)
        {
            Dictionary<String, MenuOption> result = new Dictionary<string, MenuOption>();
            if (response == null || response.value == null || response.value.Count == 0)
            {
                return result;
            }

            foreach (String line in response.value)
            {
                if (String.IsNullOrEmpty(line))
                {
                    continue;
                }

                String[] pieces = StringUtils.split(line, StringUtils.CARAT);
                if (pieces == null || pieces.Length < 2)
                {
                    continue;
                }

                MenuOption current = new MenuOption() { id = pieces[0], name = pieces[1] };
                if (pieces.Length > 2)
                {
                    if (!String.IsNullOrEmpty(pieces[2]))
                    {
                        current.locked = true;
                        current.lockKey = new SecurityKey() { name = pieces[2] };
                    }
                }
                if (pieces.Length > 3)
                {
                    current.createdBy = new User() { id = pieces[3] };
                }
                if (pieces.Length > 4)
                {
                    current.type = pieces[4];
                }

                result.Add(current.id, current);
            }

            return result;
        }

        #endregion

        #region RPCs

        public IList<RemoteProcedure> searchRpc(String target, Int32 maxRecords = 44)
        {
            IList<KeyValuePair<String, String>> sorted = LookupTableUtils.getNEntriesFromLookupTable(_cxn, "8994", target, maxRecords);
            IList<RemoteProcedure> result = new List<RemoteProcedure>();
            foreach (KeyValuePair<String, String> kvp in sorted)
            {
                result.Add(new RemoteProcedure() { id = kvp.Key, name = kvp.Value });
            }
            return result;
        }

        public RemoteProcedure getRpcDetail(String id)
        {
            RemoteProcedure rpc = getRpcRecord(id);
            rpc.inputParameters = getRpcParameters(id).ToList();
            return rpc;
        }

        internal RemoteProcedure getRpcRecord(String id)
        {
            ReadRequest request = buildGetRpcRecordRequest(id);
            ReadResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).read(request);
            return toRpcRecord(response);
        }

        internal ReadRequest buildGetRpcRecordRequest(String id)
        {
            ReadRequest request = new ReadRequest(_cxn.getSource());
            request.setFile("8994");
            request.setIens(id);
            return request;
        }

        internal RemoteProcedure toRpcRecord(ReadResponse response)
        {
            Dictionary<String, String> dict = response.convertResponseToInternalDict();
            RemoteProcedure result = new RemoteProcedure();
            result.name = dict[".01"];
            result.tag = DictionaryUtils.safeGet(dict, ".02");
            result.routine = DictionaryUtils.safeGet(dict, ".03");
            result.returnValueType = DictionaryUtils.safeGet(dict, ".04");
            result.version = DictionaryUtils.safeGet(dict, ".09");
            result.description = DictionaryUtils.safeGet(dict, "1");
            result.id = DictionaryUtils.safeGet(dict, "IEN");
            result.responseDescription = DictionaryUtils.safeGet(dict, "3");
            return result;
        }

        internal IList<RemoteProcedureParameter> getRpcParameters(string rpcIen)
        {
            ReadRangeRequest request = buildGetRpcParametersRequest(rpcIen);
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toRpcParameters(response);
        }

        internal ReadRangeRequest buildGetRpcParametersRequest(String rpcIen)
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setFields(".01;.02;.03;.04;.05");
            request.setFile("8994.02");
            request.setIens(rpcIen);
            request.setIdentifierParam("IP");
            return request;
        }

        internal IList<RemoteProcedureParameter> toRpcParameters(ReadRangeResponse response)
        {
            IList<RemoteProcedureParameter> result = new List<RemoteProcedureParameter>();

            if (response != null && response.value != null && response.value.Count > 0)
            {
                for (int i = 0; i < response.value.Count; i++)
                {
                    if (String.IsNullOrEmpty(response.value[i]))
                    {
                        continue;
                    }
                    String[] pieces = StringUtils.split(response.value[i], StringUtils.CARAT);

                    RemoteProcedureParameter current = new RemoteProcedureParameter();
                    current.id = pieces[0];
                    current.name = pieces[1];
                    current.type = pieces[2];
                    
                    String maximumDataLength = pieces[3];
                    Int32.TryParse(maximumDataLength, out current.maximumDataLength);

                    String required = pieces[4];
                    current.required = StringUtils.parseBool(required);

                    result.Add(current);
                }
            }
            return result;
        }

        public IList<RemoteProcedure> getRpcsForOption(String optionId)
        {
            ReadRangeRequest request = buildGetRpcsForOptionRequest(optionId);
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toRpcsForOption(response);
        }

        internal ReadRangeRequest buildGetRpcsForOptionRequest(String optionId)
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setFile("19.05");
            request.setFields(".01");
            request.setFlags("IP"); // grab external value (RPC name)
            request.setIdentifierParam("S RPCIEN=$P(^(0),U,1) S RPCNAME=$P($G(^XWB(8994,RPCIEN,0)),U,1) D EN^DDIOL(RPCNAME)");
            request.setIens(optionId);
            return request;
        }

        internal IList<RemoteProcedure> toRpcsForOption(ReadRangeResponse response)
        {
            IList<RemoteProcedure> rpcs = new List<RemoteProcedure>();

            if (response != null && response.value != null && response.value.Count > 0)
            {
                foreach (String line in response.value)
                {
                    if (String.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    String[] pieces = StringUtils.split(line, StringUtils.CARAT);
                    if (pieces.Length < 3 || String.IsNullOrEmpty(pieces[2]))
                    {
                        continue; // don't add RPCs with blank name
                    }
                    RemoteProcedure current = new RemoteProcedure();
                    current.id = pieces[1];
                    if (pieces.Length > 2)
                    {
                        current.name = pieces[2];
                    }
                    rpcs.Add(current);
                }
            }

            return rpcs;
        }

        #endregion

        public FmFile getVistaFileDetails(String fileNumber)
        {
            return new VistaRpcToolsDao(_cxn).getFileDetails(fileNumber);
        }
    }
}