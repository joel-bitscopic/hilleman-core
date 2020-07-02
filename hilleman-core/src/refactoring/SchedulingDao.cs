using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.utils;
using Newtonsoft.Json;
using com.bitscopic.hilleman.core.dao;
using System.Text;
using com.bitscopic.hilleman.core.domain.session;
using com.bitscopic.hilleman.core.dao.vista;

namespace com.bitscopic.hilleman.core.refactoring
{
    public class SchedulingDao : IRefactoringApi
    {
        IVistaConnection _cxn;

        public SchedulingDao(IVistaConnection cxn)
        {
            _cxn = cxn;
        }

        public void setTarget(IVistaConnection target)
        {
            _cxn = target;
        }

        #region Get Appointments

        #region Get Patient's Appointments

        public Appointment getPatientAppointment(String patientId, DateTime apptDateTime)
        {
            ReadRequest request = buildGetPatientApptRequest(patientId, apptDateTime);
            ReadResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).read(request);
            return toPatientAppointment(response);
        }

        internal ReadRequest buildGetPatientApptRequest(String patientId, DateTime appDateTime)
        {
            ReadRequest request = new ReadRequest(_cxn.getSource());
            request.setFile("2.98");
            request.setFlags("IEN");
            request.setFields("*");
            request.setIens(DateUtils.toVistaDateShortTime(appDateTime, _cxn.getSource().timeZoneParsed) + "," + patientId);
            return request;
        }

        internal Appointment toPatientAppointment(ReadResponse response)
        {
            Dictionary<String, String> apptDict = response.convertResponseToInternalDict();
            Appointment result = new Appointment()
            {
                start = DateUtils.toDateTime(apptDict["IEN"], _cxn.getSource().timeZoneParsed),
                location = new Location() { id = DictionaryUtils.safeGet(apptDict, ".01") },
                status = DictionaryUtils.safeGet(apptDict, "3"),
                purpose = DictionaryUtils.safeGet(apptDict, "9"),
                type = DictionaryUtils.safeGet(apptDict, "9.5"),
                created = DateUtils.toDateTime(DictionaryUtils.safeGet(apptDict, "20"), _cxn.getSource().timeZoneParsed),
                encounterLink = DictionaryUtils.safeGet(apptDict, "21")
            };

            return result;
        }

        /// <summary>
        /// The most sensible RPC entry point (ORWCV VST) located to retrieve a patient's appointments contains a
        /// huge amount of indirection. The amount of code that is executed before
        /// the routine PAT^SDAMA303 is significant. Finally, once reaching this routine, the code traverses
        /// the APPOINTMENT multiple of the PATIENT file forwards or backwards (depending on the function arguments
        /// specified) and simply does a $O/$G to retrieve data until there are no more appointments or the max #
        /// is reached. Using the appointment date/dtime and clinic, the code then attempts to match up the appointment
        /// in the HOSPITAL LOCATION/APPOINTMENT file for all the non-cancelled appointments. Data elements such as:
        /// appointment length, comments, overbook, check in time, check out user, etc. are all supplemented by this
        /// match operation
        /// </summary>
        /// <param name="patientId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IList<Appointment> getPatientAppointments(String patientId, String startDateString, String endDateString)
        {
            return getPatientAppointments(patientId, 
                DateUtils.parseDateTime(startDateString, _cxn.getSource().timeZoneParsed), 
                DateUtils.parseDateTime(endDateString, _cxn.getSource().timeZoneParsed));
        }

        public IList<Appointment> getPatientAppointments(String patientId, DateTime start, DateTime end)
        {
            ReadRangeRequest request = buildGetPatientAppointmentsRequest(patientId, start, end);
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toAppointments(response, patientId);
        }

        internal ReadRangeRequest buildGetPatientAppointmentsRequest(String patientId, DateTime start, DateTime end)
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            
            request.setFile("2.98");
            request.setFields(".01;.01E;3;9;9.5;19;20");
            request.setIens(patientId);
            request.setFlags("IP");
            request.setCrossRef("#");
            request.setMax("44");
            request.setFrom(DateUtils.toVistaDateShortTime(start.Subtract(new TimeSpan(0, 0, 1)), _cxn.getSource().timeZoneParsed));

            return request;
        }

        internal IList<Appointment> toAppointments(ReadRangeResponse response, String patientId)
        {
            IList<Appointment> result = new List<Appointment>();

            foreach (String apptStr in response.value)
            {
                if (String.IsNullOrEmpty(apptStr) || !apptStr.Contains("^"))
                {
                    continue;
                }

                String[] pieces = apptStr.Split(new char[] { '^' });

                if (pieces.Length < 7)
                {
                    continue;
                }

                Appointment newAppt = new Appointment()
                {
                    start = DateUtils.toDateTime(pieces[0], _cxn.getSource().timeZoneParsed),
                    location = new Location() { id = pieces[1], name = pieces[2] },
                    status = pieces[3],
                    purpose = pieces[4],
                    type = pieces[5],
                    created = DateUtils.toDateTime(pieces[7], _cxn.getSource().timeZoneParsed)
                };

                // business rule/data supplement - if appointment is not cancelled (status != C, CA, PC, PCA) then fetch corresponding
                // appointment on the HOSPITAL LOCATION (#44) file
                if (!(new List<String>() { "C", "CA", "PC", "PCA" }.Contains(newAppt.status))) // if the status isn't in this list
                {
                    Appointment apptFromSc = matchAppointment(newAppt.location.id, patientId, pieces[0]);
                    if (apptFromSc != null)
                    {
                        newAppt.length = apptFromSc.length;
                        // TODO - copy over other supplemented values
                    }
                }
                // end supplement

                result.Add(newAppt);
            }

            return result;
        }

        #endregion

        #region Get Clinic Appointments

        #region Match Single Patient's Appointment
        public Appointment matchAppointment(IList<Appointment> apptsForSlot, String patientId)
        {
            return toMatchingAppointment(apptsForSlot, patientId);
        }

        public Appointment matchAppointment(String clinicIen, String patientId, String apptDateTime)
        {
            ReadRangeRequest request = buildMatchAppointmentRequest(clinicIen, patientId, apptDateTime);
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toMatchingAppointment(response, patientId);
        }

        internal ReadRangeRequest buildMatchAppointmentRequest(String clinicIen, String patientId, String apptDateTime)
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());

            request.setFile("44.003");
            request.setFields(".01;1;9;302;303;304;305;306;309;310;688");
            request.setIens(String.Concat(",", apptDateTime,  ",", clinicIen, ","));
            request.setFlags("IP");
            request.setCrossRef("#");
            // note: removing screen param so we can see all appointments in slot and set areOtherApptsInSlot property
            //request.setScreenParam("I $P(^(0),U)=" + patientId);

            return request;
        }

        internal Appointment toMatchingAppointment(IList<Appointment> appts, String patientId)
        {
            Appointment match = null;
            if (appts != null && appts.Count > 0)
            {
                foreach (Appointment appt in appts)
                {
                    if (appt.patient != null && String.Equals(appt.patient.id, patientId))
                    {
                        match = appt;
                        break;
                    }
                }
            }
            return match;
        }

        internal Appointment toMatchingAppointment(ReadRangeResponse response, String patientId)
        {
            if (response.value == null || response.value.Count == 0)
            {
                return null;
            }


            //.01;1;9;302;303;304;305;306;309;310;688
            foreach (String appt in response.value)
            {
                String[] pieces = appt.Split(new char[] { '^' });
                if (pieces == null || pieces.Length < 2 || !String.Equals(patientId, pieces[1]))
                {
                    continue;
                }
                Appointment result = new Appointment();
                result.id = pieces[0];
                result.patient = new Patient() { id = pieces[1] };
                result.length = pieces[2];
                result.consultLink = pieces[11];
                result.isOverbook = !String.IsNullOrEmpty(pieces[3]);
                return result;
                // 9 = OVERBOOK
                // 302-309 are check in/out fields
                // 310 - APPOINTMENT CANCELLED?
                // 688 - CONSULT LINK -> #123
            }
            
            return null;
        }
        #endregion

        #region Fetch Clinic's Appointments
        public Int32 getOverbooksForDay(String clinicIen, String day)
        {
            return getOverbooksForDay(clinicIen, DateUtils.parseDateTime(day, _cxn.getSource().timeZoneParsed));
        }

        public Int32 getOverbooksForDay(String clinicIen, DateTime day)
        {
            IList<Appointment> apptsForDay = getClinicAppointments(clinicIen, 
                DateUtils.toVistaDate(day.Subtract(new TimeSpan(1, 0, 0, 0))) + ".235959",
                DateUtils.toVistaDate(day.Add(new TimeSpan(1, 0, 0, 0))) + ".000001");

            Int32 runningCount = 0;

            foreach (Appointment appt in apptsForDay)
            {
                if (appt.isOverbook)
                {
                    runningCount++;
                }
            }

            return runningCount;
        }

        public IList<Appointment> getClinicAppointments(String clinicIen, String startDateTime, String stopDateTime)
        {
            IList<String> apptTimes = getClinicAppointmentTimes(clinicIen, startDateTime, stopDateTime);
            List<Appointment> allAppts = new List<Appointment>();
            foreach (String apptTime in apptTimes)
            {
                allAppts.AddRange(getAppointmentsForClinicAndTime(clinicIen, apptTime));
            }
            return allAppts;
        }

        internal IList<Appointment> getAppointmentsForClinicAndTime(String clinicIen, String timeSlot)
        {
            ReadRangeRequest request = buildGetAppointmentsForClinicAndTimeRequest(clinicIen, timeSlot);
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toAppointmentsForClinicAndTime(response, timeSlot);
        }

        internal ReadRangeRequest buildGetAppointmentsForClinicAndTimeRequest(String clinicIen, String timeSlot)
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());

            DateTime normalizedSlot = DateUtils.parseDateTime(timeSlot, _cxn.getSource().timeZoneParsed);

            request.setFile("44.003");
            request.setFields(".01;1;8;9;302;304;305;306;309;688");
            request.setIens(String.Concat(",", DateUtils.toVistaDateShortTime(normalizedSlot, _cxn.getSource().timeZoneParsed), ",", clinicIen, ","));
            request.setFlags("IP");
            request.setCrossRef("#");

            return request;
        }

        internal IList<Appointment> toAppointmentsForClinicAndTime(ReadRangeResponse response, String timeSlot)
        {
            IList<Appointment> result = new List<Appointment>();

            foreach (String apptTimeRec in response.value)
            {
                String[] pieces = apptTimeRec.Split(new char[] { '^' });
                if (pieces == null || pieces.Length < 2)
                {
                    continue;
                }
                Appointment current = new Appointment();
                current.id = pieces[0];
                current.patient = new Patient() { id = pieces[1] };
                current.length = pieces[2];

                current.start = DateUtils.parseDateTime(timeSlot, _cxn.getSource().timeZoneParsed);
                current.isOverbook = !String.IsNullOrEmpty(pieces[4]);
                // TODO - check in/out fields??

                current.created = DateUtils.toDateTime(pieces[3], _cxn.getSource().timeZoneParsed);
                current.consultLink = pieces[10];

                result.Add(current);
            }

            return result;

        }

        internal IList<String> getClinicAppointmentTimes(String clinicIen, String startDateTime, String stopDateTime)
        {
            ReadRangeRequest request = buildGetClinicAppointmentTimesRequest(clinicIen, startDateTime, stopDateTime);
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toClinicAppointmentTimes(response);
        }

        internal ReadRangeRequest buildGetClinicAppointmentTimesRequest(String clinicIen, String startDateTime, String stopDateTime)
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());

            DateTime normalizedStart = DateUtils.parseDateTime(startDateTime, _cxn.getSource().timeZoneParsed);
            DateTime normalizedEnd = DateUtils.parseDateTime(stopDateTime, _cxn.getSource().timeZoneParsed);

            request.setFile("44.001");
            request.setFields(".01");
            request.setIens(String.Concat(",", clinicIen, ","));
            request.setFlags("IP");
            request.setCrossRef("B");
            request.setFrom(DateUtils.toVistaDate(normalizedStart)); // startDateTime.Split(new char[] { '.' })[0]);
            request.setScreenParam(
                String.Format("I $P(^(0),U)>={0},$P(^(0),U)<={1}", 
                DateUtils.toVistaDateShortTime(normalizedStart, _cxn.getSource().timeZoneParsed), 
                DateUtils.toVistaDateShortTime(normalizedEnd, _cxn.getSource().timeZoneParsed)));

            return request;
        }

        private IList<String> toClinicAppointmentTimes(ReadRangeResponse response)
        {
            IList<String> result = new List<String>();

            foreach (String apptTimeRec in response.value)
            {
                String[] pieces = apptTimeRec.Split(new char[] { '^' });
                if (pieces == null || pieces.Length < 2)
                {
                    continue;
                }
                result.Add(pieces[0]);
            }

            return result;
        }

        #endregion

        #endregion

        #endregion

        #region Helpers

        #region Get Clinic Details

        public IList<Clinic> getClinics(String target)
        {
            return getClinics(target, 44);
        }

        public IList<Clinic> getClinics(String target, Int32 maxRex)
        {
            ReadRangeRequest request = buildGetClinicsRequest(target, maxRex);
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toClinics(response);
        }

        internal ReadRangeRequest buildGetClinicsRequest(String target, Int32 maxRex = 44)
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());

            request.setFile("44");
            request.setFields(".01;1;1912;1913;1914;1917;1918");
            request.setFlags("IP");
            request.setFrom(target);
            request.setMax(maxRex.ToString());
            request.setScreenParam("I $P(^(0),U,3)=\"C\",'$G(^(\"OOS\"))");
            request.setCrossRef("B");

            return request;
        }

        internal IList<Clinic> toClinics(ReadRangeResponse response)
        {
            IList<Clinic> result = new List<Clinic>();

            foreach (String line in response.value)
            {
                String[] pieces = StringUtils.split(line, StringUtils.CARAT);
                if (pieces.Length < 3)
                {
                    continue;
                }
                result.Add(new Clinic()
                {
                    id = pieces[0],
                    name = pieces[1],
                    appointmentLength = pieces[3],
                    startTime = pieces[5],
                    displayIncrementsPerHour = pieces[6]
                });
            }

            return result;
        }

        public Clinic getClinic(String clinicId)
        {
            ReadRequest request = buildGetClinicDetailsRequest(clinicId);
            ReadResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).read(request);
            return toClinic(response, clinicId);
        }

        public Clinic getClinicDetails(String clinicId)
        {
            Clinic result = getClinic(clinicId);
            IList<String> specialInstructions = getClinicSpecialInstructions(clinicId);
            IList<Person> privilegedUsers = getPrivilegedUsers(clinicId);
            result.privilegedUsers = privilegedUsers;
            result.specialInstructions = specialInstructions;
            return result;
        }

        internal ReadRequest buildGetClinicDetailsRequest(String clinicId)
        {
            ReadRequest request = new ReadRequest(_cxn.getSource());
            request.setFile("44");
            request.setIens(clinicId);
            return request;
        }

        public IList<String> getClinicSpecialInstructions(String clinicId)
        {
            ReadRangeRequest request = buildGetClinicSpecialInstructions(clinicId);
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toClinicSpecialInstructions(response);
        }

        internal ReadRangeRequest buildGetClinicSpecialInstructions(String clinicId)
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setFields(".01");
            request.setFile("44.03");
            request.setIens(clinicId);
            return request;
        }

        internal IList<String> toClinicSpecialInstructions(ReadRangeResponse response)
        {
            IList<String> result = new List<String>();
            foreach (String line in response.value)
            {
                String[] pieces = StringUtils.split(line, StringUtils.CARAT_ARY, StringSplitOptions.RemoveEmptyEntries);
                if (pieces.Length < 2)
                {
                    continue;
                }
                result.Add(pieces[1]);
            }
            return result;
        }

        public bool isPrivilegedUser(String clinicId, String userId)
        {
            ReadRequest request = buildIsPrivilegedUserRequest(clinicId, userId);
            ReadResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).read(request);
            return toIsPrivilegedUser(response);
        }

        internal ReadRequest buildIsPrivilegedUserRequest(String clinicId, String userId)
        {
            ReadRequest request = new ReadRequest(_cxn.getSource());
            request.setFile("44.04");
            request.setIens(userId + "," + clinicId);
            return request;
        }

        internal bool toIsPrivilegedUser(ReadResponse response)
        {
            if (response == null || response.value.Count == 0 ||
                (response.value.Count > 0 && String.Equals(response.value[0], "[ERROR]", StringComparison.CurrentCultureIgnoreCase)))
            {
                return false;
            }
            
            return true;
        }

        public IList<Person> getPrivilegedUsers(String clinicId)
        {
            ReadRangeRequest request = buildGetPrivilegedUsersRequest(clinicId);
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toPrivilegedUsers(response);
        }

        internal ReadRangeRequest buildGetPrivilegedUsersRequest(String clinicId)
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setFields(".01");
            request.setFile("44.04");
            request.setFlags("IP");
            request.setIens(clinicId);
            request.setIdentifierParam("S A=$P($NA(^(0)),\",\",3) D EN^DDIOL($P($G(^VA(200,A,0)),U))"); // fetch the privileged user's name
            return request;
        }

        internal IList<Person> toPrivilegedUsers(ReadRangeResponse response)
        {
            if (response == null || response.value.Count == 0 ||
                (response.value.Count == 1 && String.Equals(response.value[0], "[DATA]", StringComparison.CurrentCultureIgnoreCase)))
            {
                return null;
            }

            IList<Person> result = new List<Person>();

            foreach (String line in response.value)
            {
                String[] pieces = StringUtils.split(line, StringUtils.CARAT_ARY, StringSplitOptions.RemoveEmptyEntries);
                if (pieces.Length < 3)
                {
                    continue;
                }
                Person current = new Person()
                {
                    nameString = pieces[2]
                };
                current.addId(new Identifier() { id = pieces[1], name = "IEN", sourceSystemId = _cxn.getSource().id });

                result.Add(current);
            }

            return result;
        }

        internal Clinic toClinic(ReadResponse response, String id)
        {
            Dictionary<String, String> clinicDict = response.convertResponseToInternalDict();
            if (clinicDict.ContainsKey("2") && !String.Equals("C", clinicDict["2"]))
            {
                throw new ArgumentException(String.Format("Record {0} is not a clinic - it is type: {1}", id, clinicDict["2"])); 
            }

            Clinic result = new Clinic();
            result.id = id;
            if (clinicDict.ContainsKey(".01"))
            {
                result.name = clinicDict[".01"];
            }
            if (clinicDict.ContainsKey("8"))
            {
                result.stopCode = clinicDict["8"];
            }
            if (clinicDict.ContainsKey("1914"))
            {
                result.startTime = clinicDict["1914"];
            }
            if (clinicDict.ContainsKey("1912"))
            {
                result.appointmentLength = clinicDict["1912"];
            }
            if (clinicDict.ContainsKey("1917"))
            {
                result.displayIncrementsPerHour = clinicDict["1917"];
            }

            if (String.IsNullOrEmpty(result.startTime))
            {
                result.startTime = "8"; // default to 8 AM start time if not specified
            }
            return result;
        }

        #endregion

        #region Clinic Availability

        public String getClinicAvailabilityString(String clinicId, DateTime day)
        {
            ReadRequest request = buildGetClinicAvailabilityRequest(clinicId, day);
            ReadResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).read(request);
            return response.convertResponseToInternalDict()["1"];
        }

        public IList<TimeSlot> getClinicAvailability(String clinicId, String dateString)
        {
            return getClinicAvailability(clinicId, DateUtils.parseDateTime(dateString, _cxn.getSource().timeZoneParsed));
        }

        public IList<TimeSlot> getClinicAvailability(String clinicId, DateTime day)
        {
            Clinic c = getClinic(clinicId);
            ReadRequest request = buildGetClinicAvailabilityRequest(clinicId, day);
            ReadResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).read(request);
            return toClinicAvailabilityForDay(response, day, c);
        }

        internal ReadRequest buildGetClinicAvailabilityRequest(String clinicId, DateTime day)
        {
            ReadRequest request = new ReadRequest(_cxn.getSource());
            request.setFile("44.005");
            request.setIens(DateUtils.toVistaDate(day) + "," + clinicId);
            return request;
        }

        internal IList<TimeSlot> toClinicAvailabilityForDay(ReadResponse response, DateTime day, Clinic clinic)
        {
            Dictionary<String, String> clinDict = response.convertResponseToInternalDict();
            if (clinDict != null && clinDict.Count > 0)
            {
                return parseSingleDayAvailability(clinDict["1"], day, Convert.ToInt32(clinic.startTime), Convert.ToInt32(clinic.appointmentLength), Convert.ToInt32(clinic.displayIncrementsPerHour));
            }
            return new List<TimeSlot>();
        }

        public IList<TimeSlot> getClinicAvailability(String clinicId, String startDateString, String endDateString)
        {
            return getClinicAvailability(clinicId, DateUtils.parseDateTime(startDateString, _cxn.getSource().timeZoneParsed), DateUtils.parseDateTime(endDateString, _cxn.getSource().timeZoneParsed));
        }

        public IList<TimeSlot> getClinicAvailability(String clinicId, DateTime startDate, DateTime endDate)
        {
            ReadRangeRequest request = buildGetClinicAvailabilityRequest(clinicId, startDate, endDate);
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);

            Clinic c = getClinic(clinicId);
            return toAvailabilityForDateRange(response, c);
        }

        internal IList<TimeSlot> toAvailabilityForDateRange(ReadRangeResponse response, Clinic clinic)
        {
            List<TimeSlot> result = new List<TimeSlot>();
            
            foreach (String line in response.value)
            {
                String[] pieces = StringUtils.split(line, StringUtils.CARAT_ARY, StringSplitOptions.None);
                if (pieces == null || pieces.Length < 3)
                {
                    continue;
                }

                DateTime currentDay = DateUtils.toDateTime(pieces[1], _cxn.getSource().timeZoneParsed);
                IList<TimeSlot> daysAvailability = parseSingleDayAvailability(pieces[2], currentDay, clinic.startTime, clinic.appointmentLength, clinic.displayIncrementsPerHour);
                result.AddRange(daysAvailability);
            }

            return result;
        }

        internal ReadRangeRequest buildGetClinicAvailabilityRequest(String clinicId, DateTime startDate, DateTime endDate)
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setCrossRef("B");
            request.setFields(".01;1");
            request.setFile("44.005");
            request.setFlags("IP");
            request.setIens(clinicId);
            request.setFrom(DateUtils.toVistaDate(startDate));

            Double maxDays = endDate.Subtract(startDate).TotalDays;
            request.setMax((Convert.ToInt32(maxDays + 1)).ToString());

            request.setScreenParam(String.Format("I $P($G(^(0)),U)<{0}", DateUtils.toVistaDate(endDate.AddDays(1))));

            return request;
        }

        internal IList<TimeSlot> parseSingleDayAvailability(String availabilityString, DateTime day, String clinicStartTime, String apptLengthMins, String displayIncrementsPerHour)
        {
            return parseSingleDayAvailability(availabilityString, day, Convert.ToInt32(clinicStartTime), Convert.ToInt32(apptLengthMins), Convert.ToInt32(displayIncrementsPerHour));
        }

        // e.g. "MO 12  |       [1] [1] [1] [1] [1] [1] "
        // e.g. "TU 13  |       [1] [1] [1] [1] "
        // e.g. "WE 14  [1] [1] [1] [1] [1] [1] [1] [1] "
        internal IList<TimeSlot> parseSingleDayAvailability(String availabilityString, DateTime day, int clinicStartTime, int apptLengthMins, Int32 displayIncrementsPerHour)
        {
            IList<TimeSlot> result = new List<TimeSlot>();
            if (String.IsNullOrEmpty(availabilityString) || (!availabilityString.Contains("[") && !availabilityString.Contains("|")))
            {
                return result; // not a valid time slot line
            }

            int startIdx = 8;
            int iFlag = 0;
            int slotCount = 0;
            DateTime clinicStartDateTime = new DateTime(day.Year, day.Month, day.Day, clinicStartTime, 0, 0);
            while (startIdx < availabilityString.Length)
            {
                DateTime currentSlotStart = clinicStartDateTime.AddMinutes(slotCount * (60 / displayIncrementsPerHour));
                DateTime currentSlotEnd = currentSlotStart.AddMinutes(apptLengthMins);
                TimeSlot current = new TimeSlot()
                {
                    start = currentSlotStart, // clinicStartDateTime.AddMinutes(slotCount * (60/displayIncrementsPerHour)),
                    end = currentSlotEnd // clinicStartDateTime.AddMinutes((slotCount + 1) * apptLengthMins),
                };

                char flag = availabilityString[startIdx];
                if ((65 <= flag && flag <= 90) || ((97 <= flag && flag <= 122))) // overbooked if character letter
                {
                    current.available = false;
                    current.text = flag.ToString();
                }
                else if (Int32.TryParse(flag.ToString(), out iFlag))
                {
                    if (iFlag > 0)
                    {
                        current.available = true;
                        current.text = iFlag.ToString();
                    }
                    else
                    {
                        current.available = false;
                        current.text = "0";
                    }
                }
                else
                {
                    current.available = false;
                    current.text = "No availability";
                }
                // don't forget to increment slotCount
                slotCount++;
                startIdx += getIndexShiftForDisplayIncrements(displayIncrementsPerHour); // getIndexShiftForApptLength(apptLengthMins);
                result.Add(current);
            }
            return result;
        }

        internal int getIndexShiftForDisplayIncrements(int displayIncrementsPerHour)
        {
            switch (displayIncrementsPerHour)
            {
                case 0: // use 0 for variable length appts - seem to use 2/hour slotting
                    return 4;
                case 1:
                    return 8;
                case 2:
                    return 4;
                case 3:
                    return 2;
                case 4:
                    return 2;
                case 6:
                    return 2;
                default:
                    throw new ArgumentException("Unable to parse availability for those display increments: " + displayIncrementsPerHour.ToString());
            }
        }

        //internal int getIndexShiftForApptLength(int apptLength)
        //{
        //    switch (apptLength)
        //    {
        //        case 120:
        //            return 4;
        //        case 60:
        //            return 8;
        //        case 30:
        //            return 4;
        //        case 20:
        //            return 2; 
        //        case 15:
        //            return 2;
        //        case 10:
        //            return 2;
        //        default:
        //            throw new ArgumentException(apptLength.ToString() + " is not a standard appointment length");
        //    }
        //}

        public void saveAvailabilityString(String clinicId, DateTime appointmentDateTime, String availabilityString)
        {
            UpdateRequest request = new UpdateRequest(_cxn.getSource());
            request.setFile("44.005");
            request.setIens(DateUtils.toVistaDate(appointmentDateTime) + "," + clinicId);
            request.addFieldAndValue("1", availabilityString);
            UpdateResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).update(request);
        }

        public void decreaseClinicAvailability(String clinicId, DateTime appointmentDateTime)
        {
            UpdateRequest request = buildDecreaseClinicAvailabilityRequest
                (getClinic(clinicId), appointmentDateTime, getClinicAvailabilityString(clinicId, appointmentDateTime));
            UpdateResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).update(request);
        }

        internal UpdateRequest buildDecreaseClinicAvailabilityRequest(Clinic clinic, DateTime appointmentDateTime, String currentAvailabilityString)
        {
            UpdateRequest request = new UpdateRequest(_cxn.getSource());
            request.setFile("44.005");
            request.setIens(DateUtils.toVistaDate(appointmentDateTime) + "," + clinic.id);
            request.addFieldAndValue("1", changeAvailabilityString(-1, clinic, appointmentDateTime, currentAvailabilityString));
            return request;
        }

        internal UpdateRequest buildIncreaseClinicAvailabilityRequest(Clinic clinic, DateTime appointmentDateTime, String currentAvailabilityString)
        {
            UpdateRequest request = new UpdateRequest(_cxn.getSource());
            request.setFile("44.005");
            request.setIens(DateUtils.toVistaDate(appointmentDateTime) + "," + clinic.id);
            request.addFieldAndValue("1", changeAvailabilityString(1, clinic, appointmentDateTime, currentAvailabilityString));
            return request;
        }

        internal string changeAvailabilityString(Int32 directionAndAmount, Clinic clinic, DateTime appointmentDateTime, string availabilityString)
        {
            Int32 apptLengthMins = Convert.ToInt32(clinic.appointmentLength);
            Int32 displayIncrementsPerHour = String.IsNullOrEmpty(clinic.displayIncrementsPerHour) ? 0 : Convert.ToInt32(clinic.displayIncrementsPerHour);
            Int32 clinicStartTime = Convert.ToInt32(clinic.startTime);

            int currentIdx = 8;

            DateTime currentClinicApptTime = new DateTime(appointmentDateTime.Year, appointmentDateTime.Month, appointmentDateTime.Day, clinicStartTime, 0, 0);

            if (currentClinicApptTime > appointmentDateTime)
            {
                throw new ArgumentException("Appointment is earlier than clinic start");
            }

            // walk through appointment slots
            while (currentClinicApptTime < appointmentDateTime)
            {
                currentClinicApptTime = currentClinicApptTime.AddMinutes(apptLengthMins);
                currentIdx += getIndexShiftForDisplayIncrements(displayIncrementsPerHour); //getIndexShiftForApptLength(apptLengthMins);
            }

            if (currentIdx > availabilityString.Length || currentClinicApptTime.CompareTo(appointmentDateTime) != 0)
            {
                throw new com.bitscopic.hilleman.core.domain.exception.HillemanBaseException("There was a problem locating the correct appointment slot in the availability string!");
            }

            // found right spot! now change availability string based on current character/flag
            // character progression: XWVUTSRQPONMLKJIHGFEDCBA0123456789jklmnopqrstuvwxyz
            String flagsString = "XWVUTSRQPONMLKJIHGFEDCBA0123456789jklmnopqrstuvwxyz";
            char flag = availabilityString[currentIdx];
            Int32 indexInFlagsStr = flagsString.IndexOf(flag);

            if (indexInFlagsStr < 0)
            {
                throw new ArgumentException("The availability string appears to contain invalid characters...");
            }

            Int32 newIndex = indexInFlagsStr + directionAndAmount;
            if (newIndex < 0 || flagsString.Length <= newIndex)
            {
                throw new ArgumentException("Unable to adjust the availability string by that amount/direction");
            }

            flag = flagsString[indexInFlagsStr + directionAndAmount];

            // replace the flag in the availability string
            StringBuilder sb = new StringBuilder(availabilityString);
            sb[currentIdx] = flag;
            return sb.ToString();
        }

        #endregion

        #region Check-In/Check-Out

        // TODO!!

        #endregion

        #region EWL

        // TODO!

        #endregion

        #region Mark No-Show
        // TODO!!
        #endregion

        #region Get Cancellation Reasons

        public IList<CancellationReason> getCancellationReasons()
        {
            ReadRangeRequest request = buildGetCancellationReasonsRequest();
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toCancellationReasons(response);
        }

        internal ReadRangeRequest buildGetCancellationReasonsRequest()
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setFile("409.2");
            request.setFields(".01;2;3");
            request.setScreenParam("I '$P(^(0),U,4)"); // screen inactive

            return request;
        }

        internal IList<CancellationReason> toCancellationReasons(ReadRangeResponse response)
        {
            IList<CancellationReason> result = new List<CancellationReason>();
            
            if (response.value != null && response.value.Count > 0)
            {
                foreach (String line in response.value)
                {
                    String[] pieces = StringUtils.split(line, StringUtils.CARAT);
                    result.Add(new CancellationReason()
                    {
                        id = pieces[0],
                        name = pieces[1],
                        type = pieces[2],
                        synonym = pieces[3]
                    });
                }
            }

            return result;
        }

        #endregion

        #region Get Appointment Statuses

        public IList<AppointmentStatus> getAppointmentStatuses()
        {
            ReadRangeRequest request = buildGetAppointmentStatusesRequest();
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toAppointmentStatuses(response);
        }

        internal ReadRangeRequest buildGetAppointmentStatusesRequest()
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setFile("409.63");
            request.setFields(".01;.02;.03;.04;.05;.06");

            return request;
        }

        internal IList<AppointmentStatus> toAppointmentStatuses(ReadRangeResponse response)
        {
            IList<AppointmentStatus> result = new List<AppointmentStatus>();

            if (response.value != null && response.value.Count > 0)
            {
                foreach (String line in response.value)
                {
                    String[] pieces = StringUtils.split(line, StringUtils.CARAT);
                    result.Add(new AppointmentStatus()
                    {
                        id = pieces[0],
                        name = pieces[1],
                        abbreviation = pieces[2],
                        checkInAllowed = StringUtils.parseBool(pieces[3]),
                        cancelAllowed = StringUtils.parseBool(pieces[4]),
                        noShowAllowed = StringUtils.parseBool(pieces[5]),
                        checkOutAllowed = StringUtils.parseBool(pieces[6])
                    });
                }
            }

            return result;
        }
        #endregion

        #region Get Scheduling Request Types

        /// <summary>
        /// Comes from LSTSRT^SDMAPI1 - appears to be the defined SET values
        /// of patient appointment subfile (2.98) field #25 - SCHEDULING REQUEST TYPE.
        /// 
        /// As these are most likely pretty static, it's not clear whether a dynamic call
        /// to VistA is *really* needed. For now, ust returning a static list from the current 
        /// FileMan definition for this field/file according to OSEHRA.org
        /// </summary>
        /// <returns></returns>
        public Dictionary<String, String> getSchedulingRequestTypes()
        {
            Dictionary<String, String> result = new Dictionary<string, string>();
            result.Add("N", "'NEXT AVAILABLE' APPT.");
            result.Add("C", "OTHER THAN 'NEXT AVA.' (CLINICIAN REQ.)");
            result.Add("P", "OTHER THAN 'NEXT AVA.' (PATIENT REQ.)");
            result.Add("W", "WALKIN APPT.");
            result.Add("M", "MULTIPLE APPT. BOOKING");
            result.Add("A", "AUTO REBOOK");
            result.Add("O", "OTHER THAN 'NEXT AVA.' APPT.");
            return result;
        }

        #endregion

        #endregion

        #region Create Appointments

        #region Create New Clinic Appointment

        public String createClinicAppointment(Appointment appointment)
        {
            CreateRequest request = buildCreateClinicAppointmentRequest(appointment);
            CreateResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).create(request);
            return CreateResponse.getCreatedIEN(response);
        }

        internal CreateRequest buildCreateClinicAppointmentRequest(Appointment appointment)
        {
            CreateRequest request = new CreateRequest(_cxn.getSource());
            request.setFile("44.003");
            request.setIens(DateUtils.toVistaDateShortTime(appointment.start, _cxn.getSource().timeZoneParsed) + "," + appointment.location.id);
            request.addFieldAndValue(".01", appointment.patient.id);
            request.addFieldAndValue("1", appointment.length);
            request.addFieldAndValue("2", ""); // x-ray text
            request.addFieldAndValue("3", ""); // other tests text
            request.addFieldAndValue("4", ""); // ward location text
            request.addFieldAndValue("7", appointment.createdBy.id); // data entry clerk -> NEW PERSON
            request.addFieldAndValue("8", DateUtils.toVistaDateTime(appointment.created, _cxn.getSource().timeZoneParsed)); // date appointment made (make caller specify to respect timezones)
            request.addFieldAndValue("9", ""); // overbook - 'O'
            request.addFieldAndValue("10", ""); // prior xray results to clinic - 'Y'
            return request;
        }

        internal bool apptSlotExists(Appointment appointment)
        {
            return ("0" == new ToolsDaoFactory().getToolsDao(_cxn).gvv(
                String.Format("$D(^SC({0},\"S\",{1}))", appointment.location.id, DateUtils.toVistaDateShortTime(appointment.start, _cxn.getSource().timeZoneParsed))));

        }

        public void createNewClinicAppointmentSlotIfNeeded(Appointment appointment)
        {
            if (apptSlotExists(appointment))
            {
                createNewClinicAppointmentSlot(appointment);
            }
        }

        internal ReadRequest buildDoesClinicAppointmentSlotExistRequest(Appointment appointment)
        {
            ReadRequest request = new ReadRequest(_cxn.getSource());
            request.setIens(DateUtils.toVistaDateShortTime(appointment.start, _cxn.getSource().timeZoneParsed) + "," + appointment.location.id);
            request.setFields(".01");
            request.setFile("44.003");
            return request;
        }

        internal String createNewClinicAppointmentSlot(Appointment appointment)
        {
            CreateRequest request = buildCreateNewAppointmentSlotRequest(appointment);
            CreateResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).create(request);
            return CreateResponse.getCreatedIEN(response);
        }

        internal CreateRequest buildCreateNewAppointmentSlotRequest(Appointment appointment)
        {
            CreateRequest request = new CreateRequest(_cxn.getSource());
            request.setFile("44.001");
            request.setExactIens(true);
            request.setIens(CreateRequest.FILER_FIND_OR_CREATE + DateUtils.toVistaDateTime(appointment.start, _cxn.getSource().timeZoneParsed) + "," + appointment.location.id);
            request.addFieldAndValue(".01", DateUtils.toVistaDateShortTime(appointment.start, _cxn.getSource().timeZoneParsed));
            return request;
        }

        #endregion

        #region Create New Patient Appointment
        public String createPatientAppointment(Appointment appointment)
        {
            CreateRequest request = buildCreatePatientAppointmentRequest(appointment);
            CreateResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).create(request);
            return CreateResponse.getCreatedIEN(response);
        }

        internal CreateRequest buildCreatePatientAppointmentRequest(Appointment appointment)
        {
            CreateRequest request = new CreateRequest(_cxn.getSource());
            request.setFile("2.98");
            request.setExactIens(true);
            request.setIens(CreateRequest.FILER_FIND_OR_CREATE + DateUtils.toVistaDateShortTime(appointment.start, _cxn.getSource().timeZoneParsed) + "," + appointment.patient.id);
            request.addFieldAndValue(".01", appointment.location.id);
            request.addFieldAndValue("3", appointment.status); // status (SET of codes)
            request.addFieldAndValue("5", ""); // lab date
            request.addFieldAndValue("6", ""); // xray date
            request.addFieldAndValue("7", ""); // ekg date
            request.addFieldAndValue("9", appointment.purpose); // purpose of visit (1: C&P, 2: 10-10, 3: SCHEDULED VISIT, 4: UNSCHEDULED)
            request.addFieldAndValue("9.5", appointment.type); // appt type (-> 409.1)
            request.addFieldAndValue("19", appointment.createdBy.id); // entered by (-> 200)
            request.addFieldAndValue("20", DateUtils.toVistaDateTime(appointment.created, _cxn.getSource().timeZoneParsed)); // date appt made
            request.addFieldAndValue("24", ""); // appt type subcategory (-> 35.2)
            request.addFieldAndValue("25", ""); // scheduling request type (SET of codes)
            request.addFieldAndValue("26", ""); // next available appt indicator (SET of codes)
            return request;
        }

        #endregion

        #region Create Appointment Wrapper

        public Appointment createAppointment(String userId, String patientId, String clinicId, String startDateTime, String apptLength)
        {
            Appointment arg = new Appointment()
            {
                created = DateTime.Now,
                createdBy = new Person() { id = userId }, // fetching from logged in user's session. TBD - provide another API where user ID is passed in??
                location = new Location() { id = clinicId },
                patient = new Patient() { id = patientId },
                start = DateUtils.parseDateTime(startDateTime, _cxn.getSource().timeZoneParsed),
                length = apptLength,
                status = "NT", // hardcoded ok for now but should be dynamic
                type = "9", // hardcoded ok for now but should be dynamic
                purpose = "3" // hardcoded ok for now but should be dynamic
            };
            return this.createAppointment(patientId, arg);
        } 

        public Appointment createAppointment(String patientId, Appointment appt)
        {
            if ("0" != new ToolsDaoFactory().getToolsDao(_cxn).gvv(
                String.Format("$D(^DPT({0},\"S\",{1}))", patientId, DateUtils.toVistaDateShortTime(appt.start, _cxn.getSource().timeZoneParsed))))
            {
                throw new ArgumentException("The patient already has an appointment at that time!");
            }
            if (matchAppointment(appt.location.id, patientId, DateUtils.toVistaDateShortTime(appt.start, _cxn.getSource().timeZoneParsed)) != null)
            {
                throw new ArgumentException("The clinic already has an appointment for the patient at that time!");
            }
            createNewClinicAppointmentSlotIfNeeded(appt);
            String patientApptId = createPatientAppointment(appt);
            String clinicApptId = createClinicAppointment(appt);
            decreaseClinicAvailability(appt.location.id, appt.start);
            return appt;
        }

        #endregion

        #endregion

        #region Appointment Cancellation

        #region Check Appointment State
        /// <summary>
        /// Check if appointment is in a state that permits cancellation. Returns NULL if cancellation is not permitted. 
        /// Returns KeyValuePair of Appointment objects if ok to cancel. Key is the patient appointment, Value is the clinic appointment
        /// </summary>
        /// <param name="patientId"></param>
        /// <param name="clinicId"></param>
        /// <param name="apptDateTime"></param>
        /// <returns></returns>
        public KeyValuePair<Appointment, Appointment> okToCancel(String patientId, String clinicId, DateTime apptDateTime)
        {
            Appointment patientAppt = this.getPatientAppointment(patientId, apptDateTime);
            Appointment clinicAppt = this.matchAppointment(clinicId, patientId, DateUtils.toVistaDateShortTime(apptDateTime, _cxn.getSource().timeZoneParsed));
            // TODO - checks!!!
            return new KeyValuePair<Appointment, Appointment>(patientAppt, clinicAppt);
        }

        #endregion

        #region Cancel Clinic Appointment

        internal void deleteAppointmentSlot(String clinicId, DateTime apptTime)
        {
            // changed from a delete request to run to eliminate another call to Vista to fetch the appointments before executing the kill
            new ToolsDaoFactory().getToolsDao(_cxn).run(String.Format("K:$O(^SC({0},\"S\",{1},0))'>0 ^SC({0},\"S\",{1},0)", clinicId, DateUtils.toVistaDateShortTime(apptTime, _cxn.getSource().timeZoneParsed)));
            // TODO - can probably delete build* and to* functions associated with this call...
            //DeleteRequest request = buildDeleteAppointmentSlotRequest(clinicId, apptTime);
            //DeleteResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).delete(request);
        }

        internal DeleteRequest buildDeleteAppointmentSlotRequest(String clinicId, DateTime apptTime)
        {
            DeleteRequest request = new DeleteRequest(_cxn.getSource());
            request.setFile("44.001");
            request.setIens(clinicId + "," + DateUtils.toVistaDateShortTime(apptTime, _cxn.getSource().timeZoneParsed)); // e.g. 12,3150506.1215
            return request;
        }

        public void cancelClinicAppointment(DateTime apptTime, String patientId, String clinicId, String apptIen)
        {
            DeleteRequest request = buildCancelClinicAppointmentRequest(apptTime, patientId, clinicId, apptIen);
            DeleteResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).delete(request);
        }

        internal DeleteRequest buildCancelClinicAppointmentRequest(DateTime apptTime, String patientId, String clinicId, String apptIen)
        {
            DeleteRequest request = new DeleteRequest(_cxn.getSource());
            request.setFile("44.003");
            request.setIens(clinicId + "," + DateUtils.toVistaDateShortTime(apptTime, _cxn.getSource().timeZoneParsed) + "," + apptIen); // e.g. 12,3150506.1215,2
            return request;
        }

        // per CANCEL^SDMDAL1 - the "ARAD" cross ref (which i *think* is for sending prior xray results to the clinic) gets set to N when killing the appt 
        internal void setAradXref(String clinicId, DateTime apptDateTime, String patientId, String setTo = "N")
        {
            new ToolsDaoFactory().getToolsDao(_cxn).setGlobal(
                String.Format("^SC(\"ARAD\",{0},{1},{2})", clinicId, DateUtils.toVistaDateShortTime(apptDateTime, _cxn.getSource().timeZoneParsed), patientId), "N");
        }

        /// <summary>
        /// Per CANCEL^SDMDAL1 - kill consult link xref. These typically look something like this in prod:
        /// ^SC("AWAS1",972255,2210,3050912.143,1)
        /// ^SC("AWAS1",3726503,13389,3150626.11,1)
        /// ^SC("AWAS1",3726503,13389,3150626.11,1,0)
        /// </summary>
        /// <param name="consultLink"></param>
        internal void killAwasXref(String consultLink, String clinicId, DateTime apptDateTime, String clinicApptId)
        {
            // TBD - it's probably sufficient to kill ^SC(AWAS1,consultLink) - which is what the M code does
          //  new ToolsDaoFactory().getToolsDao(_cxn).killGlobal(
          //      String.Format("^SC(\"AWAS1\",{0},{1},{2},{3})", consultLink, clinicId, DateUtils.toVistaDateShortTime(apptDateTime), clinicApptId));
            new ToolsDaoFactory().getToolsDao(_cxn).killGlobal(
                String.Format("^SC(\"AWAS1\",{0})", consultLink));
        }

        public void killFirstOverbook(IList<Appointment> apptsInSlot, String clinicId, DateTime slot)
        {
            if (apptsInSlot != null && apptsInSlot.Count > 0)
            {
                foreach (Appointment appt in apptsInSlot)
                {
                    if (appt.isOverbook)
                    {
                        removeOverbook(clinicId, slot, appt.id);
                        break;
                    }
                }
            }
        }

        public void removeOverbook(String clinicId, DateTime slot, String apptIen)
        {
            UpdateRequest request = buildRemoveOverbookRequest(clinicId, slot, apptIen);
            UpdateResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).update(request);
        }

        internal UpdateRequest buildRemoveOverbookRequest(String clinicId, DateTime slot, String apptIen)
        {
            UpdateRequest request = new UpdateRequest(_cxn.getSource());
            request.setFile("44.003");
            request.setIens(String.Concat(apptIen, ",", DateUtils.toVistaDateShortTime(slot, _cxn.getSource().timeZoneParsed), ",", clinicId));
            request.addFieldAndValue("9", "");
            return request;
        }

        #endregion

        #region Cancel Patient Appointment

        public void cancelPatientAppointment(
            DateTime apptTime, 
            String patientId, 
            String clinicId, 
            String cancelTypeCode, 
            String cancelledBy, 
            String cancelDateTime,
            String cancelReasonCode,
            String endUserId,
            String dateApptMade,
            String cancellationRemarks)
        {
            UpdateRequest request = buildCancelPatientAppointmentRequest(DateUtils.toVistaDateShortTime(apptTime, _cxn.getSource().timeZoneParsed), patientId, cancelTypeCode, cancelledBy, cancelDateTime, cancelReasonCode, endUserId, dateApptMade, cancellationRemarks);
            UpdateResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).update(request);
        }

        /// <summary>
        /// SDMDAL.int >>
        /// CANCEL(RETURN,DFN,SD,TYP,RSN,RMK,CDT,USR,OUSR,ODT) ; Cancel appointment.
        /// N IENS,FDA
        /// S IENS=SD_","_DFN_","
        /// S FDA(2.98,IENS,3)=TYP
        /// S FDA(2.98,IENS,14)=USR
        /// S FDA(2.98,IENS,15)=CDT
        /// S FDA(2.98,IENS,16)=RSN
        /// S FDA(2.98,IENS,19)=OUSR
        /// S FDA(2.98,IENS,20)=ODT
        /// S:$G(RMK)]"" FDA(2.98,IENS,17)=$E(RMK,1,160)
        /// D FILE^DIE("","FDA","RETURN")
        /// Q

        /// </summary>
        /// <param name="apptTime"></param>
        /// <param name="patientId"></param>
        /// <param name="clinicId"></param>
        internal UpdateRequest buildCancelPatientAppointmentRequest(
            String apptTime, 
            String patientId, 
            String cancelTypeCode, 
            String cancelledBy, 
            String cancelDateTime, 
            String cancelReasonCode, 
            String userId, 
            String dateApptMade, 
            String cancellationRemarks)
        {
            UpdateRequest request = new UpdateRequest(_cxn.getSource());
            request.setFile("2.98");
            request.setIens(patientId + "," + apptTime);
            request.addFieldAndValue("3", cancelTypeCode);
            request.addFieldAndValue("14", cancelledBy);
            request.addFieldAndValue("15", cancelDateTime);
            request.addFieldAndValue("16", cancelReasonCode);
            request.addFieldAndValue("19", userId);
            request.addFieldAndValue("20", dateApptMade); // TBD - do we really need to update this to the exact same value we just read?? 
            request.addFieldAndValue("17", cancellationRemarks);
            return request;
        }

        #endregion

        #region Cancel Appointment Wrapper

        public void cancelAppointmentWithRules(
            String patientId, 
            String clinicId, 
            DateTime apptDateTime,
            String cancelTypeCode,
            String cancelReasonCode,
            String cancelUserId, 
            String endUserId, 
            String remarks)
        {
            if (!isPrivilegedUser(clinicId, endUserId))
            {
                throw new UnauthorizedAccessException("User does not have permission for clinic");
            }

            String beforeAvailabilityString = getClinicAvailabilityString(clinicId, apptDateTime);
            Clinic theClinic = getClinic(clinicId);

            Appointment patientAppt = getPatientAppointment(patientId, apptDateTime);

            OutpatientEncounter encounter = null;
            if (!String.IsNullOrEmpty(patientAppt.encounterLink))
            {
                encounter = new EncounterDao(_cxn).getEncounter(patientAppt.encounterLink);
            }

            IList<Appointment> apptsInSlot = getAppointmentsForClinicAndTime(clinicId, DateUtils.toVistaDateShortTime(apptDateTime, _cxn.getSource().timeZoneParsed));
            Appointment clinicAppt = matchAppointment(apptsInSlot, patientId);

            Consult apptConsult = null;
            if (!String.IsNullOrEmpty(clinicAppt.consultLink))
            {
                apptConsult = new ConsultDao(_cxn).getConsult(clinicAppt.consultLink);
            }

            // if not already cancelled
            if (!String.IsNullOrEmpty(patientAppt.status) && !patientAppt.status.Contains("C"))
            {
                cancelPatientAppointment(
                    apptDateTime, 
                    patientId, 
                    clinicId, 
                    cancelTypeCode, 
                    cancelUserId, 
                    DateUtils.toVistaDateTime(DateTime.Now, _cxn.getSource().timeZoneParsed), 
                    cancelReasonCode, 
                    endUserId, 
                    DateUtils.toVistaDateTime(patientAppt.created, _cxn.getSource().timeZoneParsed), 
                    remarks); 
            }

            cancelClinicAppointment(apptDateTime, patientId, clinicId, clinicAppt.id);

            if (apptsInSlot.Count == 1) // note this was the count during setup - should be 0 now since we already cancelled clinic appt
            {
                deleteAppointmentSlot(clinicId, apptDateTime);
            }

            if (apptsInSlot.Count > 1) // if more than one appt, remove any potential overbooks
            {
                killFirstOverbook(apptsInSlot, clinicId, apptDateTime);
            }

            setAradXref(clinicId, apptDateTime, patientId);

            if (!String.IsNullOrEmpty(clinicAppt.consultLink))
            {
                killAwasXref(clinicAppt.consultLink, clinicId, apptDateTime, clinicAppt.id);
            }

            String afterAvailabilityString = changeAvailabilityString(+1, theClinic, apptDateTime, beforeAvailabilityString);
            saveAvailabilityString(clinicId, apptDateTime, afterAvailabilityString);

            // setup:
            // 1: get patient appt
            // 1a: get outpatient encounter (use field 21 from 2.98)
            // 2: get clinic appt
            // 2a: get consult (use field 688 from 44.003)
            // TODO: figure out EXACTLY what $$STATUS^SDAM1 is doing to determine correct status
            // end setup
            // per CHKCAN^SDMAPI3
            // is appointment already cancelled? field 3 (STATUS) in 2.98 contains 'C' in code
            // if outpatient encounter checked out? field 303 (CHECKED OUT) in 44.003 > 0
            // does user have access to clinic?
            // does status permit cancel?
            // cancel patient appointment
            // kill first overbook appointment
            // TODO BEFORE PROD: handle consult, if present --- field 688 (CONSULT LINK) in 44.003
                // consult comes from ^GMR(123
                // per CANCEL^SDCAPI1
                // CPRS status = SCHEDULED? Quit consult handling if status != "" and != SCHEDULED --- field 8 in 123 -> 100.01 ORDER STATUS FILE
                //(CONSULT,6,3,SNDPRV,"","",.COMMENT)
                // set 44.003 field 688 (CONSULT LINK) to '@'
            // kill clinic appointment
            // if no more appts in clinic slot, kill slot


        }

        #endregion

        #endregion

        #region Wait List

        public IList<WaitListEntry> getWaitList(String patientId)
        {
            ReadRangeRequest request = buildGetWaitListRequest(patientId);
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toWaitList(response);
        }

        internal ReadRangeRequest buildGetWaitListRequest(String patientId)
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setFile("409.3");
            request.setFrom((Convert.ToInt32(patientId) - 1).ToString());
            request.setPart(patientId);
            request.setCrossRef("B");
            request.setMax("1000");
            request.setFields(".01;1;2;3;4;5;6;7;8;9;10;11;12;13;13.1;13.2;13.3;13.4;13.5;13.6;13.7;13.8;14;15;21;22;23;36;39");

            return request;
        }

        internal IList<WaitListEntry> toWaitList(ReadRangeResponse response)
        {
            IList<WaitListEntry> result = new List<WaitListEntry>();

            foreach (String s in response.value)
            {
                String[] pieces = StringUtils.split(s, StringUtils.CARAT);

                WaitListEntry newEntry = new WaitListEntry()
                {
                    id = pieces[0],
                    patient = new Patient() { id = pieces[1] },
                    originatingDate = DateUtils.parseDateTime(pieces[2], _cxn.getSource().timeZoneParsed),
                    institution = new Institution() { id = pieces[3] },
                    transmissionStatus = pieces[4],
                    type = pieces[5],
                    teamLink = pieces[6],
                    position = pieces[7],
                    serviceSpecialty = pieces[8],
                    clinicLocation = pieces[9],
                    originatingUser = pieces[10],
                    priority = pieces[11],
                    requestedBy = pieces[12]
                };

                result.Add(newEntry);
            }

            return result;
        }

        #endregion

        #region Appointment Check-In

        public DateTime checkInAppointmentByPatientId(String clinicId, DateTime apptDateTime, String patientId, String checkInUser)
        {
            Appointment appt = matchAppointment(clinicId, patientId, DateUtils.toVistaDateShortTime(apptDateTime, _cxn.getSource().timeZoneParsed));
            DateTime checkInTime = DateUtils.getVistaSystemTime(_cxn); // time should be in UTC!
            UpdateRequest request = buildCheckinAppointmentRequest(clinicId, apptDateTime, appt.id, checkInUser, checkInTime);
            UpdateResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).update(request);

            // TODO - call event drivers from file 101 for "SDAM APPOINTMENT EVENTS"!!
            return checkInTime;
        }

        internal UpdateRequest buildCheckinAppointmentRequest(String clinicId, DateTime apptDateTime, String apptIen, String checkInUser, DateTime checkInTime)
        {
            UpdateRequest request = new UpdateRequest(_cxn.getSource());
            
            request.setFile("44.003");
            request.setIens(String.Format("{0},{1},{2}", clinicId, DateUtils.toVistaDateShortTime(apptDateTime, _cxn.getSource().timeZoneParsed), apptIen));
            request.addFieldAndValue("302", checkInUser);
            request.addFieldAndValue("309", DateUtils.toVistaDateTime(checkInTime, _cxn.getSource().timeZoneParsed));

            return request;
        }

        #endregion

        #region No-Show

        public void markAppointmentNoShow(String clinicId, DateTime apptDateTime, String patientId, String userId, DateTime noShowDateTime)
        {
            Appointment patientAppt = getPatientAppointment(patientId, apptDateTime);
            IList<AppointmentStatus> statuses = getAppointmentStatuses();
            if (String.IsNullOrEmpty(patientAppt.status) || String.Equals(patientAppt.status, "NT"))
            {

            }
        }

        public bool okToMarkNoShow(Appointment appt, IList<AppointmentStatus> vistaApptStatuses)
        {
            // TODO - finish!! Seems to be a combination of calls to various subroutines to determine if marking appt as no-show is ok
            foreach (AppointmentStatus status in vistaApptStatuses)
            {
                if (String.Equals(appt.status, status.id))
                {
                    if (!status.noShowAllowed)
                    {
                        return true;
                    }
                    break;
                }
            }
            return false;
        }

        #endregion

    }
}