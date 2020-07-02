using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.utils;
using Newtonsoft.Json;
using com.bitscopic.hilleman.core.dao;
using com.bitscopic.hilleman.core.domain.session;
using com.bitscopic.hilleman.core.dao.vista;

namespace com.bitscopic.hilleman.core.refactoring
{
    public class PatientSearchDao : IRefactoringApi
    {
        IVistaConnection _cxn;

        public PatientSearchDao(IVistaConnection cxn)
        {
            _cxn = cxn;
        }

        public void setTarget(IVistaConnection target)
        {
            _cxn = target;
        }

        public String getPatientIDFromSSN(String ssn, bool detectDuplicates = true)
        {
            String dfn = new ToolsDaoFactory().getToolsDao(_cxn).gvv("$O(^DPT(\"SSN\",\"" + ssn + "\",\"\"))");
            if (String.IsNullOrEmpty(dfn))
            {
                throw new ArgumentException("No patient ID found for that SSN!");
            }
            // detect 
            String dupe = new ToolsDaoFactory().getToolsDao(_cxn).gvv("$O(^DPT(\"SSN\",\"" + ssn + "\",\"" + dfn + "\"))");
            if (!String.IsNullOrEmpty(dupe))
            {
                throw new ArgumentException("Detected multiple patient records with the same SSN!");
            }
            return dfn;
        }

        public Patient getPatientWithAppts(String patientId)
        {
            Patient result = getPatient(patientId);
            result.appointments = new SchedulingDao(_cxn).getPatientAppointments(patientId, DateTime.Now.Subtract(new TimeSpan(30, 0, 0, 0)), DateTime.Now.Add(new TimeSpan(365 * 2, 0, 0, 0)));
            return result;
        }


        public Patient getPatient(String patientId)
        {
            ReadRequest request = buildGetPatientRequest(patientId);
            ReadResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).read(request);
            return toPatient(response, patientId);
        }

        internal ReadRequest buildGetPatientRequest(String patientId)
        {
            ReadRequest request = new ReadRequest(_cxn.getSource());
            request.setFile("2");
            request.setIens(patientId);
            request.setFields(".01;.02;.03;.05;.06;.07;.08;.09;.1;.101;.111;.112;.113;.114;.115;.116;.131;.132;.133;.134;.351;991.01;991.02;991.03;63");

            return request;
        }

        internal Patient toPatient(ReadResponse response, String patientId)
        {
            Patient result = new Patient() { id = patientId };
            Dictionary<String, String> internalDict = response.convertResponseToInternalDict();
            result.nameString = DictionaryUtils.safeGet(internalDict, ".01");
            result.gender = DictionaryUtils.safeGet(internalDict, ".02");
            result.dateOfBirthVistA = DictionaryUtils.safeGet(internalDict, ".03");
            result.dateOfBirth = DateUtils.parseDateTime(result.dateOfBirthVistA, TimeZoneInfo.Utc);
            // .05 -> Marital Status (-> 11)
            // .06 -> Race (-> 10)
            // .07 -> Occupation
            // .08 -> Religious Pref (-> 13)
            result.idSet = new IdentifierSet();
            result.idSet.add(DictionaryUtils.safeGet(internalDict, ".09"), "SSN");
            result.idSet.add(DictionaryUtils.safeGet(internalDict, "63"), "LRDFN");

            String wardLoc = DictionaryUtils.safeGet(internalDict, ".1");
            if (!String.IsNullOrEmpty(wardLoc))
            {
                result.hospitalLocation = new HospitalLocation() 
                { 
                    name = wardLoc, 
                    roomBed = DictionaryUtils.safeGet(internalDict, ".101")
                };
            }

            result.address = new Address()
            {
                street1 = DictionaryUtils.safeGet(internalDict, ".111"),
                street2 = DictionaryUtils.safeGet(internalDict, ".112"),
                street3 = DictionaryUtils.safeGet(internalDict, ".113"),
                city = DictionaryUtils.safeGet(internalDict, ".114"),
                state = DictionaryUtils.safeGet(internalDict, ".115"),
                zipcode = DictionaryUtils.safeGet(internalDict, ".116")
            };

            String homePhone = DictionaryUtils.safeGet(internalDict, ".131");
            String workPhone = DictionaryUtils.safeGet(internalDict, ".132");
            String cellPhone = DictionaryUtils.safeGet(internalDict, ".132");
            if (!String.IsNullOrEmpty(homePhone) ||!String.IsNullOrEmpty(workPhone) ||!String.IsNullOrEmpty(cellPhone))
            {
                result.phones = new Dictionary<string, string>();
            }
            if (!String.IsNullOrEmpty(homePhone)) { result.phones.Add("home", homePhone); }
            if (!String.IsNullOrEmpty(workPhone)) { result.phones.Add("work", workPhone); }
            if (!String.IsNullOrEmpty(cellPhone)) { result.phones.Add("cell", cellPhone); }

            String email = DictionaryUtils.safeGet(internalDict, ".133");
            if (!String.IsNullOrEmpty(email)) 
            {
                result.emailAddresses = new Dictionary<string, string>();
                result.emailAddresses.Add("personal", email);
            }

            // do something with this!
            String dateOfDeath = DictionaryUtils.safeGet(internalDict, ".351");

            result.idSet.ids.Add(new Identifier() { id = DictionaryUtils.safeGet(internalDict, "991.01"), name = "ICN", checksum = DictionaryUtils.safeGet(internalDict, "991.02") });
            // 991.03 = CMOR site
            return result;
        }

        public Patient updatePatientContactInfo(Patient p)
        {
            // get current record to see what's being changed
            ReadRequest rr = new ReadRequest(_cxn.getSource());
            rr.setFile("2");
            rr.setIens(p.idSet.ids[0].id);
            ReadResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).read(rr);
            Dictionary<String, String> patientDict = response.convertResponseToExternalDict();

            UpdateRequest request = new UpdateRequest(_cxn.getSource());
            bool anythingToUpdate = false;
            if (p.emailAddresses != null && p.emailAddresses.Count > 0 && p.emailAddresses.ContainsKey("personal"))
            {
                if (!patientDict.ContainsKey(".133") || !String.Equals(p.emailAddresses["personal"], patientDict[".133"], StringComparison.CurrentCultureIgnoreCase))
                {
                    anythingToUpdate = true;
                    // UPDATE EMAIL!!!
                    request.addFieldAndValue(".133", p.emailAddresses["personal"]);
                }
            }
            if (p.phones != null && p.phones.Count > 0 && p.phones.ContainsKey("cell"))
            {
                // TODO - we're relying on EXACT phone format matching here... would probably be better to normalize and compare!
                if (!patientDict.ContainsKey(".134") || !String.Equals(p.phones["cell"], patientDict[".134"]))
                {
                    anythingToUpdate = true;
                    // UPDATE PHONE!!!
                    request.addFieldAndValue(".134", p.phones["cell"]);
                }
            }

            if (p.phones != null && p.phones.Count > 0 && p.phones.ContainsKey("home"))
            {
                if (!patientDict.ContainsKey(".131") || !String.Equals(p.phones["home"], patientDict[".131"]))
                {
                    anythingToUpdate = true;
                    request.addFieldAndValue(".131", p.phones["home"]);
                }
            }

            if (p.phones != null && p.phones.Count > 0 && p.phones.ContainsKey("work"))
            {
                if (!patientDict.ContainsKey(".132") || !String.Equals(p.phones["work"], patientDict[".132"]))
                {
                    anythingToUpdate = true;
                    request.addFieldAndValue(".132", p.phones["work"]);
                }
            }

            if (p.address != null)
            {
                anythingToUpdate = true;
                request.addFieldAndValue(".111", p.address.street1);
                request.addFieldAndValue(".112", p.address.street2);
                request.addFieldAndValue(".114", p.address.city);
                request.addFieldAndValue(".115", p.address.state);
                request.addFieldAndValue(".116", p.address.zipcode);
            }

            if (!anythingToUpdate)
            {
                return p;
            }

            request.setFile("2");
            request.setIens(p.idSet.ids[0].id);
            UpdateResponse updateResponse = new CrrudDaoFactory().getCrrudDao(_cxn).update(request);
            if (updateResponse.isSuccessfulCreateUpdateDeleteResponse())
            {
                return p;
            }
            else
            {
                throw new com.bitscopic.hilleman.core.domain.exception.HillemanBaseException("Unexpected error updating patient record: " + StringUtils.join(updateResponse.value));
            }
        }

        public IList<Patient> searchForPatient(String target)
        {
            ReadRangeRequest request = buildSearchForPatientRequest(target);
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toSearchForPatientResponse(response);
        }

        /// <summary>
        /// This function contains the business logic for the different types of searches typically supported by the PATIENT file (#2).
        /// 
        /// * Full SSN
        /// * Last 4 SSN
        /// * Last initial + last 4 SSN
        /// * Name
        /// 
        /// This function examines the format and determines the appropriate cross reference to use for traversing the PATIENT file.
        /// It also converts the alphabetic characters to upper case as that is the convention for storage. 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        internal ReadRangeRequest buildSearchForPatientRequest(String target)
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setFile("2");
            request.setFields(".01;.02;.03;.06;.09;.1;.101;.111;.112;.113;.114;.115;.116;991.01;991.03;.133;.134;.131;.132");
            request.setFlags("IP");
            request.setMax("44");

             // specify cross ref based on target format (SSN: "SSN", last 4: if length is 5 -> "BS5", else "BS", name: "B")
            Int64 trash = 0;
            if (target.Length >= 4 && target.Length <= 5) // if last 4 of SSN --or-- last initial + last 4
            {
                if (target.Length == 5 && Int64.TryParse(target.Substring(1, 4), out trash))
                {
                    request.setCrossRef("BS5");
                }
                else if (Int64.TryParse(target.Substring(0, 4), out trash))
                {
                    request.setCrossRef("BS");
                }
                else
                {
                    request.setCrossRef("B");
                }

                request.setFrom(target.ToUpper());
            }
            else if (StringUtils.isInteger(target) && target.Length == 9)
            {
                request.setCrossRef("SSN");
                request.setFrom(target.Substring(0, 8) + "+"); // 123456789 --> 12345678+
            }
            else if (target.EndsWith("P") && target.Length == 10 && StringUtils.isInteger(target.Replace("P", ""))) // e.g. 123456789P is sometimes how 'sensitive' SSNs are recorded in vista
            {
                request.setCrossRef("SSN");
                request.setFrom(target.Substring(0, 8) + "+"); // 123456789P --> 12345678+
            }
            else
            {
                request.setCrossRef("B");
                request.setFrom(target.ToUpper());
            }


            return request;
        }

        internal IList<Patient> toSearchForPatientResponse(ReadRangeResponse response)
        {
            IList<Patient> result = new List<Patient>();

            foreach (String line in response.value)
            {
                String[] pieces = line.Split(new char[] { '^' });
                if (pieces.Length < 6)
                {
                    continue;
                }
                Patient p = new Patient()
                {
                    lastName = pieces[1].Split(new char[] { ',' })[0],
                    firstName = (pieces[1].Split(new char[] { ',' })[1]).Split(new char[] { ' ' })[0],
                    gender = pieces[2],
                    dateOfBirthVistA = pieces[3],
                    address = new Address()
                    {
                        street1 = pieces[8],
                        street2 = pieces[9],
                        street3 = pieces[10],
                        city = pieces[11],
                        state = pieces[12],
                        zipcode = pieces[13]
                    }
                };

                if (!String.IsNullOrEmpty(pieces[16]))
                {
                    p.emailAddresses = new Dictionary<string, string>();
                    p.emailAddresses.Add("personal", pieces[16]);
                }
                if (!String.IsNullOrEmpty(pieces[17]))
                {
                    p.phones = new Dictionary<string, string>();
                    p.phones.Add("cell", pieces[17]);
                }
                if (!String.IsNullOrEmpty(pieces[18]))
                {
                    if (p.phones == null)
                    {
                        p.phones = new Dictionary<string, string>();
                    }
                    p.phones.Add("home", pieces[18]);
                }
                if (!String.IsNullOrEmpty(pieces[19]))
                {
                    if (p.phones == null)
                    {
                        p.phones = new Dictionary<string, string>();
                    }
                    p.phones.Add("work", pieces[19]);
                }
                if (!String.IsNullOrEmpty(pieces[6]))
                {
                    p.hospitalLocation = new HospitalLocation() { name = pieces[6] };
                }
                // add DFN (IEN) and SSN to ID set
                p.addId(new Identifier() { id = pieces[0], name = "IEN", sourceSystemId = _cxn.getSource().id });
                p.addId(new Identifier() { id = pieces[5], name = "SSN", sourceSystemId = _cxn.getSource().id });

                result.Add(p);
            }

            return result;
        }
    }
}