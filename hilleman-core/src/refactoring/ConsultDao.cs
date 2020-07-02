using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.dao;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.utils;
using com.bitscopic.hilleman.core.dao.vista;

namespace com.bitscopic.hilleman.core.refactoring
{
    public class ConsultDao : IRefactoringApi
    {
        IVistaConnection _cxn;

        public ConsultDao(IVistaConnection cxn)
        {
            _cxn = cxn;
        }

        public void setTarget(IVistaConnection target)
        {
            throw new NotImplementedException();
        }

        public Consult getConsult(String consultIen)
        {
            ReadRequest request = buildGetConsultRequest(consultIen);
            ReadResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).read(request);
            return toConsult(response);
        }

        internal ReadRequest buildGetConsultRequest(String consultIen)
        {
            ReadRequest request = new ReadRequest(_cxn.getSource());
            request.setFile("123");
            request.setIens(consultIen);
            return request;
        }

        internal Consult toConsult(ReadResponse response)
        {
            if (response.value == null || response.value.Count == 0)
            {
                return null;
            }

            Consult result = new Consult();
            Dictionary<String, String> responseDict = response.convertResponseToInternalDict();
            Dictionary<String, String> responseDictExternal = response.convertResponseToExternalDict();
            result.id = responseDict["IEN"];
            result.entryDate = DateUtils.parseDateTime(responseDict[".01"], _cxn.getSource().timeZoneParsed);
            if (responseDict.ContainsKey("3")) { result.requestDate = DateUtils.parseDateTime(responseDict["3"], _cxn.getSource().timeZoneParsed); }
            if (responseDict.ContainsKey(".02")) { result.patient = new Patient() { id = responseDict[".02"] }; }
            if (responseDict.ContainsKey(".03")) { result.order = new Order() { id = responseDict[".03"] }; }
            if (responseDict.ContainsKey(".1")) { result.displayText = responseDict[".1"]; }
            if (responseDict.ContainsKey("8"))  
            { 
                if (result.order == null) 
                {
                    result.order = new Order(); 
                } 
                result.order.status = new IdAndValue() { id = responseDict["8"], value = responseDictExternal["8"] };
            }

            return result;
        }

        public void cancelAppointmentConsult(Consult consult, Appointment appt, String cancelCode)
        {
            //TODO - need to finish this call!!
            throw new NotImplementedException();
        }

        internal UpdateRequest buildCancelAppointmentConsultRequest(Consult consult, Appointment appt, String cancelCode, String cancelRemarks)
        {
            UpdateRequest request = new UpdateRequest(_cxn.getSource());

            request.setFile("123");

            String cancelledBy = "";
            if (cancelCode.Contains("P"))
            {
                cancelledBy = "by the Patient.";
            }
            else if (cancelCode.Contains("C"))
            {
                cancelledBy = "by the Clinic.";
            }
            else
            {
                cancelledBy = ", whole clinic.";
            }

            String mRoutineFormatDt = appt.start.ToString("MM/dd/yyyy @ HH:mm");
            String comment = String.Format("{0} Appt. on {1} was cancelled {2}", appt.location.name, mRoutineFormatDt);
            request.addFieldAndValue("8", "6"); // CPRS STATUS - these appear to be hardcodes in CANCEL^SDCAPI1 call to $$STATUS^GMRCGUIS
            request.addFieldAndValue("9", "3"); // LAST ACTION TAKEN - these appear to be hardcodes in CANCEL^SDCAPI1 call to $$STATUS^GMRCGUIS

            return request;
        }

        public CreateRequest addConsultActivity(String consultIen, DateTime timestamp, String duz)
        {
            throw new NotImplementedException();
        }

        internal CreateRequest buildAddConsultActivityRequest(String consultIen, DateTime timestamp, String duz)
        {
            // TODO - FINISH!! Check out SETCOM^GMRCGUIB - seems to be a lot going on... tabling for now...
            CreateRequest request = new CreateRequest(_cxn.getSource());

            request.setFile("123.SOMETHING");
            request.setIens(consultIen);
            request.addFieldAndValue(".01", DateUtils.toVistaDateTime(timestamp, _cxn.getSource().timeZoneParsed));
            request.addFieldAndValue("1", "");
            request.addFieldAndValue("2", "");
            request.addFieldAndValue("3", "");
            request.addFieldAndValue("4", duz);
            request.addFieldAndValue("6", "");
            request.addFieldAndValue("8", "");

            return request;
        }

    }
}