using System;
using com.bitscopic.hilleman.core.dao;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.dao.vista;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.utils;

namespace com.bitscopic.hilleman.core.refactoring
{
    public class HospitalLocationDao : IRefactoringApi
    {
        IVistaConnection _cxn;

        public HospitalLocationDao(IVistaConnection cxn)
        {
            _cxn = cxn;
        }

        public void setTarget(dao.vista.IVistaConnection target)
        {
            throw new NotImplementedException();
        }

        public Dictionary<String, String> getHospitalLocationTypes()
        {
            Dictionary<String, String> result = new Dictionary<string,string>();
            result.Add("C", "CLINIC");
            result.Add("M", "MODULE");
            result.Add("W", "WARD");
            result.Add("Z", "OTHER LOCATION");
            result.Add("N", "NON-CLINIC STOP");
            result.Add("F", "FILE AREA");
            result.Add("I", "IMAGING");
            result.Add("OR", "OPERATING ROOM");
            return result;
        }

        public Institution getInstitution(String ien)
        {
            ReadRequest request = buildGetInstitutionRequest(ien);
            ReadResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).read(request);
            return toInstitution(response);
        }

        internal ReadRequest buildGetInstitutionRequest(String ien)
        {
            ReadRequest request = new ReadRequest(_cxn.getSource());
            request.setIens(ien);
            request.setFile("4");
            request.setFields("*");
            return request;
        }

        internal Institution toInstitution(ReadResponse response)
        {
            Dictionary<String, String> responseDict = response.convertResponseToInternalDict();

            return new Institution()
            {
                name = DictionaryUtils.safeGet(responseDict, ".01"),
                stationNumber = DictionaryUtils.safeGet(responseDict, "99")
            };
        }


    }
}