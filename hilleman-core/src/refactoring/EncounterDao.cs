using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.dao;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.utils;
using com.bitscopic.hilleman.core.dao.vista;

namespace com.bitscopic.hilleman.core.refactoring
{
    public class EncounterDao : IRefactoringApi
    {
        IVistaConnection _cxn;

        public EncounterDao(IVistaConnection cxn)
        {
            _cxn = cxn;
        }

        public void setTarget(IVistaConnection target)
        {
            throw new NotImplementedException();
        }

        #region Helpers

        #region Outpatient Classification Types
        
        public IList<OutpatientClassificationType> getOutpatientClassificationTypes()
        {
            ReadRangeRequest request = buildGetOutpatientClassificationTypesRequest();
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toOutpatientClassificationTypes(response);
        }

        internal ReadRangeRequest buildGetOutpatientClassificationTypesRequest()
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setFile("409.41");
            request.setFields(".01;.04;.06;.07");
            return request;
        }

        internal IList<OutpatientClassificationType> toOutpatientClassificationTypes(ReadRangeResponse response)
        {
            IList<OutpatientClassificationType> result = new List<OutpatientClassificationType>();

            if (response.value == null || response.value.Count == 0)
            {
                return result;
            }

            foreach (String s in response.value)
            {
                String[] pieces = StringUtils.split(s, StringUtils.CARAT);
                OutpatientClassificationType current = new OutpatientClassificationType();
                current.id = pieces[0];
                current.name = pieces[1];
                current.displayName = pieces[3];
                current.abbreviation = pieces[4];
                result.Add(current);
            }

            return result;
        }

        #endregion

        #endregion

        #region Get Outpatient Encounter

        /// <summary>
        /// Fetch an outpatient encounter by ID. Returns parent encounter, child encounters, classisfications
        /// </summary>
        /// <param name="encounterId"></param>
        /// <returns></returns>
        public OutpatientEncounter getEncounter(String encounterId)
        {
            ReadRequest request = buildGetOutpatientEncounterRequest(encounterId);
            ReadResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).read(request);
            return toEncounter(response);
        }

        internal ReadRequest buildGetOutpatientEncounterRequest(String encounterId)
        {
            ReadRequest request = new ReadRequest(_cxn.getSource());
            request.setFile("409.68");
            request.setIens(encounterId);
            return request;
        }

        internal OutpatientEncounter toEncounter(ReadResponse rr)
        {
            OutpatientEncounter result = new OutpatientEncounter();

            Dictionary<String, String> i = rr.convertResponseToInternalDict();
            Dictionary<String, String> e = rr.convertResponseToInternalDict();
            
            result.id = DictionaryUtils.safeGet(i, "IEN");
            result.date = DateUtils.parseDateTime(DictionaryUtils.safeGet(i, ".01"), _cxn.getSource().timeZoneParsed);
            result.patient = new Patient() { id = DictionaryUtils.safeGet(i, ".02"), nameString = DictionaryUtils.safeGet(e, ".02") };
            result.location = new HospitalLocation() { id = DictionaryUtils.safeGet(i, ".04"), name = DictionaryUtils.safeGet(e, ".04") };
            result.visit = new Visit() { id = DictionaryUtils.safeGet(i, ".05") };
            result.appointmentType = new AppointmentType() { id = DictionaryUtils.safeGet(i, ".1"), name = DictionaryUtils.safeGet(e, ".1") };
            

            return result;
        }

        public void addOutpatientClassifications(OutpatientEncounter parent)
        {
            ReadRangeRequest request = buildAddOutpatientClassificationsRequest(parent);
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            IList<OutpatientClassificationType> classificationTypes = getOutpatientClassificationTypes();
            addOutpatientClassificationsToParent(response, parent, classificationTypes);
        }

        internal ReadRangeRequest buildAddOutpatientClassificationsRequest(OutpatientEncounter parent)
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setFile("409.42");
            request.setFields(".01;.02;.03");
            request.setFlags("IP");
            request.setCrossRef("OE");
            request.setFrom((Convert.ToInt64(parent.id) - 1).ToString());
            request.setPart(parent.id);
            request.setMax("1000");

            return request;
        }

        internal void addOutpatientClassificationsToParent(ReadRangeResponse response, OutpatientEncounter parent, IList<OutpatientClassificationType> classTypes)
        {
            if (response.value == null || response.value.Count == 0)
            {
                return;
            }

            parent.classifications = new List<OutpatientClassification>();
            foreach (String classification in response.value)
            {
                String[] pieces = StringUtils.split(classification, StringUtils.CARAT);

                OutpatientClassificationType currentClassificationType = BaseClassUtils.matchById<OutpatientClassificationType>(classTypes, pieces[1]);
                parent.classifications.Add(
                    new OutpatientClassification()
                    {
                        id = pieces[0],
                        outpatientEncounterLink = pieces[2],
                        type = currentClassificationType,
                        value = pieces[3]

                    }
                );
            }
        }

        #endregion
    }
}