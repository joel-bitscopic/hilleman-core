using com.bitscopic.hilleman.core.dao.vista;
using com.bitscopic.hilleman.core.dao.vista.rpc;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.bitscopic.hilleman.core.lib
{
    public class RPALib
    {
        public RPALib() { }

        public String invokeRPA(String serializedArgs)
        {
            IVistaConnection cxn = null;
            try
            {
                // args can be any structure - dictionary of strings to keep this simple to start with
                // e.g.: 
                Dictionary<String, String> deserializedArgs = SerializerUtils.deserialize<Dictionary<String, String>>(serializedArgs);
                validateArgsForRPA(deserializedArgs);

                // get args for calls
                SourceSystem ss = getSourceSystemFromConfig(deserializedArgs["site"]); // todo - take site ID from args
                String accessCode = deserializedArgs["accessCode"];
                String verifyCode = deserializedArgs["verifyCode"];
                String brokerContext = DictionaryUtils.safeGet(deserializedArgs, "brokerContext");

                // check args

                // login to VistA
                cxn = new VistaRpcConnection(ss);
                cxn.connect();
                User loggedInUser = new VistaRpcCrrudDao(cxn).login(new VistaRpcLoginCredentials()
                {
                    provider = ss,
                    username = accessCode,
                    password = verifyCode
                }, new VistaRpcConnectionBrokerContext("", String.IsNullOrEmpty(brokerContext) ? "OR CPRS GUI CHART" : brokerContext)); // default to OR CPRS GUI CHART

                // find patient by SSN
                List<Patient> patientMatchResult = new VistaRpcPatientDao(cxn).matchFullSSN(deserializedArgs["patientSSN"]);

                return SerializerUtils.serialize(patientMatchResult);

                // todo - other stuff...
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cxn.disconnect(); // always disconnecting at end for "stateless" 
            }
        }

        private void validateArgsForRPA(Dictionary<string, string> deserializedArgs)
        {
            if (deserializedArgs == null || deserializedArgs.Count == 0
                || String.IsNullOrEmpty(DictionaryUtils.safeGet(deserializedArgs, "site"))
                || String.IsNullOrEmpty(DictionaryUtils.safeGet(deserializedArgs, "accessCode"))
                || String.IsNullOrEmpty(DictionaryUtils.safeGet(deserializedArgs, "verifyCode"))
                || String.IsNullOrEmpty(DictionaryUtils.safeGet(deserializedArgs, "patientSSN")))
            {
                throw new hilleman.core.domain.exception.HillemanBaseException(message: "Missing required args!");
            }
        }

        public SourceSystem getSourceSystemFromConfig(String id)
        {
            SourceSystemTable sstFromConfig = new SourceSystemTable(MyConfigurationManager.getValue("SourceSystemTable"));
            return sstFromConfig.getSourceSystem(id);
        }
    }


}
