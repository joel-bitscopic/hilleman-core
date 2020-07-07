using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.utils;

namespace com.bitscopic.hilleman.core.dao.vista.rpc
{
    public class VistaRpcPatientDao
    {
        VistaRpcConnection _cxn;

        public VistaRpcPatientDao(IVistaConnection connection)
        {
            _cxn = (VistaRpcConnection)connection;
        }

        internal List<Patient> matchFullSSN(String ssn)
        {
            VistaRpcQuery qry = new VistaRpcQuery("ORWPT FULLSSN");
            qry.addParameter(new VistaRpcParameter(VistaRpcParameterType.LITERAL, ssn));
            return toPatients((String)_cxn.query(qry));
        }

        internal List<Patient> toPatients(String rpcResponse)
        {
            List<Patient> result = new List<Patient>();

            String[] lines = StringUtils.split(rpcResponse, StringUtils.CRLF);
            for (int i = 0; i < lines.Length; i++)
            {
                if (String.IsNullOrEmpty(lines[i]))
                {
                    continue;
                }

                Patient current = new Patient();
                String[] fields = StringUtils.split(lines[i], StringUtils.CARAT);
                current.id = fields[0];
                current.nameString = fields[1];
                if (fields.Length <= 2)
                {
                    result.Add(current);
                    continue;
                }
                if (!String.IsNullOrEmpty(fields[2]))
                {
                    current.dateOfBirthVistA = fields[2];
                }
                if (!String.IsNullOrEmpty(fields[3]))
                {
                    current.idSet = new IdentifierSet();
                    current.idSet.add(new Identifier() { id = fields[3], name = "SSN" });
                }

                result.Add(current);
            }

            return result;
        }

    }
}