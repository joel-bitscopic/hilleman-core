using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.dao.vista;
using com.bitscopic.hilleman.core.dao;
using com.bitscopic.hilleman.core.utils;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.domain.to;
using System.Linq;
using System.Text;

namespace com.bitscopic.hilleman.core.refactoring
{
    public class LabsDao : IRefactoringApi
    {
        IVistaConnection _cxn;
        public LabsDao(IVistaConnection cxn)
        {
            this.setTarget(cxn);
        }
    
        public void setTarget(dao.vista.IVistaConnection target)
        {
            _cxn = target;
        }

        public Dictionary<String, String> fetchMultipleHowdyUIDCrossRefReceivedDate(List<String> allUIDs)
        {
            // can only combine around 20 UIDs in a single call - chunk them up
            List<List<String>> chunksOf20 = ListUtils.splitInChunks(allUIDs, 20);
            Dictionary<String, String> result = new Dictionary<string, string>();

            foreach (List<String> uids in chunksOf20)
            {
                StringBuilder sb = new StringBuilder();
                foreach (String uid in uids)
                {
                    sb.Append(String.Format("$O(^LRHY(69.87,\"B\",\"{0}\",\"\"))_\"_\"_", uid));
                }

                String combinedCall = sb.ToString().TrimEnd(new char[] { '_', '"' }); // remove last underscore + quoted underscore + underscore

                String gvvResult = new ToolsDaoFactory().getToolsDao(_cxn).gvv(combinedCall);

                String[] individualIENs = StringUtils.split(gvvResult, '_');

                if (individualIENs.Length != uids.Count)
                {
                    throw new Exception("Unexpected results from VistA when attempting to fetch UID cross references from file 69.87");
                }

                Dictionary<String, String> iensByUID = new Dictionary<string, string>();
                for (int i = 0; i < uids.Count; i++)
                {
                    if (!iensByUID.ContainsKey(uids[i]))
                    {
                        iensByUID.Add(uids[i], individualIENs[i]); // IENs should be in same order
                    }
                }

                if (iensByUID.All(kvp => String.IsNullOrEmpty(kvp.Value)))
                {
                    return new Dictionary<string, string>(); // protect against case where no UID cross references were found
                }

                // now fetch received date field for each UID which was present in 69.87
                sb = new StringBuilder();
                List<String> uidDateList = new List<string>();
                foreach (String key in iensByUID.Keys)
                {
                    if (!String.IsNullOrEmpty(iensByUID[key]))
                    {
                        uidDateList.Add(key);
                        sb.Append(String.Format("$G(^LRHY(69.87,\"{0}\",\"10\"))_\"_\"_", iensByUID[key]));
                    }
                }

                combinedCall = sb.ToString().TrimEnd(new char[] { '_', '"' }); // remove last underscore + quoted underscore + underscore

                gvvResult = new ToolsDaoFactory().getToolsDao(_cxn).gvv(combinedCall);

                String[] individualDates = StringUtils.split(gvvResult, '_');


                for (int i = 0; i < uidDateList.Count; i++)
                {
                    if (String.IsNullOrEmpty(individualDates[i]))
                    {
                        continue;
                    }
                    if (!result.ContainsKey(uidDateList[i]))
                    {
                        result.Add(uidDateList[i], individualDates[i]); // dates should be in same order
                    }
                }
            }
            return result;
        }

        public List<SpecimenType> getSpecimenTypes()
        {
            ReadRangeRequest request = buildGetSpecimenTypesRequest();
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toSpecimenTypes(response);
        }

        internal ReadRangeRequest buildGetSpecimenTypesRequest()
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setFields(".01;6;20");
            request.setFile("61");
            request.setMax("10000");
            return request;
        }

        internal List<SpecimenType> toSpecimenTypes(ReadRangeResponse response)
        {
            List<SpecimenType> result = new List<SpecimenType>();

            foreach (String delimitedRecord in response.value)
            {
                String[] pieces = StringUtils.split(delimitedRecord, StringUtils.CARAT);
                if (pieces == null || pieces.Length < 4)
                {
                    continue;
                }

                result.Add(new SpecimenType() { id = pieces[0], name = pieces[1], abbreviation = pieces[2], snomed = pieces[3] });
            }

            return result;
        }

        public String get62x8IENFromManifestID(String shippingManifestID)
        {
            return new ToolsDaoFactory().getToolsDao(_cxn).gvv("$O(^LAHM(62.8,\"B\",\"" + shippingManifestID + "\",\"\"))");
        }

        public LabPendingOrder getLabShippingManifestFromSendingSite(String ien62x8)
        {
            ReadRequest request = buildGetLabShippingManifestFromSendingSiteRequest(ien62x8);
            ReadResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).read(request);
            return toLabPendingOrderFrom62x8(response);
        }

        public LabPendingOrder toLabPendingOrderFrom62x8(ReadResponse response)
        {
            List<VistaRecord> topLevelRec = response.getRecordsForFile("62.8");
            if (topLevelRec == null || topLevelRec.Count != 1)
            {
                throw new ArgumentException("Unable to parse response from VistA - unexpected format");
            }

            Dictionary<String, String> internalDict = topLevelRec[0].convertFieldsToDict(false); //  getDictFromVistaRecord(topLevelRec[0], false);
            Dictionary<String, String> externalDict = topLevelRec[0].convertFieldsToDict(true); // getDictFromVistaRecord(topLevelRec[0], true);

            LabPendingOrder result = new LabPendingOrder();

            String shippingManifest = DictionaryUtils.safeGet(internalDict, ".01");
            String shippingManifestConfig = DictionaryUtils.safeGet(internalDict, ".02");
            String shippingManifestConfigName = DictionaryUtils.safeGet(externalDict, ".02");

            result.shippingManifest = shippingManifest;

            // ordered tests from subfile
            List<VistaRecord> subfileRecs = response.getRecordsForFile("62.801");

            result.orderedTests = new List<LabPendingOrderTest>();
            foreach (VistaRecord vr in subfileRecs)
            {
                String patientLRDFN = safeGetFieldValue(vr, ".01");
                String testIen = safeGetFieldValue(vr, ".02");
                String testName = safeGetFieldValue(vr, ".02", true);
                String specimenIEN = safeGetFieldValue(vr, ".03");
                String specimenName = safeGetFieldValue(vr, ".03", true);
                String uid = safeGetFieldValue(vr, ".05");

                LabPendingOrderTest newTest = new LabPendingOrderTest();
                newTest.id = vr.iens;
                newTest.localTestId = testIen;
                newTest.localTestName = testName;
                newTest.localTestUID = uid;
                newTest.specimen = new SpecimenType() { id = specimenIEN, name = specimenName };

                result.orderedTests.Add(newTest);
            }

            return result;
        }

        internal ReadRequest buildGetLabShippingManifestFromSendingSiteRequest(String ien)
        {
            ReadRequest request = new ReadRequest(_cxn.getSource());
            request.setFile("62.8");
            request.setFields("**");
            request.setIens(ien);
            request.setFlags("IEN");
            return request;
        }

        public String get69x6IENFromRemoteUID(String uid)
        {
            return new ToolsDaoFactory().getToolsDao(_cxn).gvv(String.Format("$O(^LRO(69.6,\"E\",\"{0}\",0))", uid));
        }

        public String get69x6IENFromShippingManifestAndUID(String shippingManifest, String uid)
        {
            String ien = new ToolsDaoFactory().getToolsDao(_cxn).gvv(String.Format("$O(^LRO(69.6,\"AD\",\"{0}\",\"{1}\",0))", shippingManifest, uid));
            if (String.IsNullOrEmpty(ien))
            {
                throw new ArgumentException("Shipping manifest/UID not found!");
            }
            return ien;
        }

        public List<String> get69x6IENsFromShippingManifest(String shippingManifest)
        {
            String uid = new ToolsDaoFactory().getToolsDao(_cxn).gvv(String.Format("$O(^LRO(69.6,\"AD\",\"{0}\",0))", shippingManifest));
            if (String.IsNullOrEmpty(uid))
            {
                throw new ArgumentException("Shipping manifest not found!");
            }

            List<String> iensForManifest = new List<string>();
            while (!String.IsNullOrEmpty(uid))
            {
                String currentIen = new ToolsDaoFactory().getToolsDao(_cxn).gvv(String.Format("$O(^LRO(69.6,\"AD\",\"{0}\",\"{1}\",0))", shippingManifest, uid));
                if (String.IsNullOrEmpty(currentIen))
                {
                    break;
                }
                iensForManifest.Add(currentIen);
                uid = new ToolsDaoFactory().getToolsDao(_cxn).gvv(String.Format("$O(^LRO(69.6,\"AD\",\"{0}\",\"{1}\"))", shippingManifest, uid)); // get next UID, if available
            }

            return iensForManifest;
        }

        public LabPendingOrder getLabPendingOrder(String ien)
        {
            ReadRequest request = buildGetTestsByRemoteUID(ien);
            ReadResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).read(request);
            return toLabPendingOrder(response);
        }

        internal LabPendingOrder toLabPendingOrder(ReadResponse response)
        {
            List<VistaRecord> topLevelRec = response.getRecordsForFile("69.6");
            if (topLevelRec == null || topLevelRec.Count != 1)
            {
                throw new ArgumentException("Unable to parse response from VistA - unexpected format");
            }

            Dictionary<String, String> internalDict = topLevelRec[0].convertFieldsToDict(false); //  getDictFromVistaRecord(topLevelRec[0], false);
            Dictionary<String, String> externalDict = topLevelRec[0].convertFieldsToDict(true); // getDictFromVistaRecord(topLevelRec[0], true);

            LabPendingOrder result = new LabPendingOrder() { source = LabPendingOrderSource.VISTA_LEDI }; // MAKE SURE TO SET SOURCE!!!

            result.patient = new Patient();

            result.patient.nameString = DictionaryUtils.safeGet(internalDict, ".01");
            result.patient.dateOfBirthVistA = DictionaryUtils.safeGet(internalDict, ".03");
            if (!String.IsNullOrEmpty(result.patient.dateOfBirthVistA))
            {
                result.patient.dateOfBirth = DateUtils.parseDateTime(result.patient.dateOfBirthVistA, TimeZoneInfo.Utc);
            }

            String ssn = DictionaryUtils.safeGet(internalDict, ".09");
            if (!String.IsNullOrEmpty(ssn))
            {
                result.patient.idSet = new IdentifierSet();
                result.patient.idSet.add(ssn, "SSN");
            }

            result.orderingSite = new Facility() { id = DictionaryUtils.safeGet(internalDict, "1"), name = DictionaryUtils.safeGet(externalDict, "1") };
            result.orderingSiteUID = DictionaryUtils.safeGet(internalDict, "3");
            result.orderingSiteAccessionNumber = DictionaryUtils.safeGet(internalDict, "3.2");
            result.collected = DateUtils.parseDateTime(DictionaryUtils.safeGet(internalDict, "11"), TimeZoneInfo.Utc);
            result.incomingMessageNumber = DictionaryUtils.safeGet(internalDict, "17");
            result.shippingManifest = DictionaryUtils.safeGet(internalDict, "18");

            result.comments = DictionaryUtils.safeGet(internalDict, "99");

            result.specimenType = new SpecimenType()
            {
                id = DictionaryUtils.safeGet(internalDict, "4"), // commenting this out because the UI will not care about the vista IEN of the specimen type but only the PG config's specimen which makes this ID confusing
                name = DictionaryUtils.safeGet(externalDict, "4")
            };
            
            // ordered tests from subfile
            List<VistaRecord> subfileRecs = response.getRecordsForFile("69.64");
            result.orderedTests = new List<LabPendingOrderTest>();
            foreach (VistaRecord vr in subfileRecs)
            {
                LabPendingOrderTest newTest = new LabPendingOrderTest();
                newTest.localTestId = newTest.id = vr.iens;
                newTest.nltId = safeGetFieldValue(vr, "1");
                newTest.nltName = safeGetFieldValue(vr, ".01");
                newTest.remoteTestId = safeGetFieldValue(vr, "3");
                newTest.remoteTestName = safeGetFieldValue(vr, "2");
                newTest.localTestUID = safeGetFieldValue(vr, "9");
                newTest.localTestId = safeGetFieldValue(vr, "11");
                newTest.localTestName = safeGetFieldValue(vr, "11", true);

                if (result.orderedBy == null)
                {
                    // these names typically look like: TRUETT,APRIL A [1013988195:NPI:NPI:USDHHS]
                    // parse out the stuff in square brackets!
                    String nameStr = safeGetFieldValue(vr, "13");
                    if (nameStr.Contains("[") && nameStr.Contains("]"))
                    {
                        nameStr = nameStr.Substring(0, nameStr.IndexOf("[")).Trim(); ;                        
                    }
                    result.orderedBy = new Person() { nameString = nameStr };
                }

                result.orderedTests.Add(newTest);
            }

            return result;
        }

        String safeGetFieldValue(VistaRecord record, String fieldNo, bool externalValue = false)
        {
            if (record.fields.Any(vf => vf.number == fieldNo))
            {
                if (externalValue)
                {
                    return record.fields.First(vf => vf.number == fieldNo).externalValue;
                }
                else
                {
                    return record.fields.First(vf => vf.number == fieldNo).value;
                }
            }

            return null;
        }

        internal ReadRequest buildGetTestsByRemoteUID(String ien)
        {
            ReadRequest request = new ReadRequest(_cxn.getSource());
            request.setFile("69.6");
            request.setFields("**");
            request.setIens(ien);
            return request;
        }

        public Dictionary<String, Dictionary<String, Object>> getAccessionsByUIDs(List<String> uids)
        {
            Dictionary<String, Dictionary<String, object>> resultsByUID = new Dictionary<string, Dictionary<string, object>>();

            foreach (String uid in uids)
            {
                Dictionary<String, Object> currentResult = getAccessionByUID(uid);
                resultsByUID.Add(uid, currentResult);
            }

            return resultsByUID;

            /*
            // build list of codes from first two chars - only need to fetch each once
            List<String> distinctAccessionAreaCodes = new List<string>();
            foreach (String uid in uids)
            {
                if (!distinctAccessionAreaCodes.Contains(uid.Substring(0, 2)))
                {
                    distinctAccessionAreaCodes.Add(uid.Substring(0, 2));
                }
            }

            // then check middle piece (either 2 or 4 chars) to see if they all match as they *somehow* map to dates. i.e. if they're all the same then only need to fetch date multiple IEN once
            bool allUIDsShareMiddlePiece = false;
            String firstUID = uids[0];
            String middlePiece = firstUID.Substring(2, 4);
            if (middlePiece.EndsWith("00") && uids.All(mp => String.Equals(mp.Substring(2, 2), firstUID.Substring(2, 2)))) // year date format usually ends in 00 -- think extra chars are used for more serial #'s -- if all UIDs have this same middle piece then only need to fetch one date multiple
            {
                allUIDsShareMiddlePiece = true;
            }
            else if (uids.All(mp => String.Equals(mp.Substring(2, 4), middlePiece)))
            {
                allUIDsShareMiddlePiece = true;
            }

            // first build dict of file 68 IENs
            Dictionary<String, String> accessionAreaIensByCode = new Dictionary<string, string>();
            foreach (String code in distinctAccessionAreaCodes)
            {
                foreach (String uid in uids)
                {
                    if (!String.Equals(uid.Substring(0, 2), code) || accessionAreaIensByCode.ContainsKey(uid.Substring(0, 2)))
                    {
                        continue;
                    }
                    // ^LRO(68,"C","C211720005",11,3110621,5)=""
                    String accessionAreaIen = new ToolsDaoFactory().getToolsDao(_cxn).gvv(String.Format("$O(^LRO(68,\"C\",\"{0}\",0))", uid));
                    if (String.IsNullOrEmpty(accessionAreaIen))
                    {
                        throw new ArgumentException("UID cross reference " + uid + " not found!");
                    }
                    accessionAreaIensByCode.Add(code, accessionAreaIen);
                }
            }

            Dictionary<String, Dictionary<String, object>> resultsByUID = new Dictionary<string, Dictionary<string, object>>();

            String datePiece = null;

            foreach (String uidCode in accessionAreaIensByCode.Keys)
            {
                foreach (String uid in uids)
                {
                    if (!uid.StartsWith(uidCode))
                    {
                        continue;
                    }

                    if (String.IsNullOrEmpty(datePiece) || !allUIDsShareMiddlePiece)
                    {
                        datePiece = new ToolsDaoFactory().getToolsDao(_cxn).gvv(String.Format("$O(^LRO(68,\"C\",\"{0}\",\"{1}\",0))", uid, accessionAreaIensByCode[uidCode]));
                    }
                    String adjustedRecordIen = uid.Substring(uid.Substring(2, 4).EndsWith("00") ? 4 : 6); // format 'IX19000025' or format 'IX30650025'?? serial number piece is last 4 or last 6 chars
                    adjustedRecordIen = StringUtils.trimLeading(adjustedRecordIen, '0');
                    ReadRequest currentUIDRequest = buildGetAccessionDetail(accessionAreaIensByCode[uidCode], datePiece, adjustedRecordIen);
                    ReadResponse currentUIDResponse = new CrrudDaoFactory().getCrrudDao(_cxn).read(currentUIDRequest);

                    Dictionary<String, Object> currentResult = toSomething(currentUIDResponse);

                    resultsByUID.Add(uid, currentResult);
                }
            }

            return resultsByUID;
            */
        }


        public Dictionary<String, Object> getAccessionByUID(String uid)
        {
            String allPiecesCombinedCall = String.Format("$O(^LRO(68,\"C\",\"{0}\",0))_\"_\"_$O(^LRO(68,\"C\",\"{0}\",$O(^LRO(68,\"C\",\"{0}\",0)),0))_\"_\"_$O(^LRO(68,\"C\",\"{0}\",$O(^LRO(68,\"C\",\"{0}\",0)),$O(^LRO(68,\"C\",\"{0}\",$O(^LRO(68,\"C\",\"{0}\",0)),0)),0))", uid);

            // ^LRO(68,"C","C211720005",11,3110621,5)=""
            String allPiecesCombined = new ToolsDaoFactory().getToolsDao(_cxn).gvv(allPiecesCombinedCall);
            if (String.IsNullOrEmpty(allPiecesCombined) || !allPiecesCombined.Contains("_"))
            {
                throw new ArgumentException("UID cross reference " + uid + " not found!");
            }

            String[] pieces = StringUtils.split(allPiecesCombined, '_');
            if (pieces.Length != 3 || String.IsNullOrEmpty(pieces[0]) || String.IsNullOrEmpty(pieces[1]) || String.IsNullOrEmpty(pieces[2]))
            {
                throw new ArgumentException("Invalid response from VistA: " + allPiecesCombined + " - unable to find accession for UID: " + uid);
            }

            String accessionAreaIen = pieces[0];
            String datePiece = pieces[1];
            String recordIen = pieces[2];

            ReadRequest request = buildGetAccessionDetail(accessionAreaIen, datePiece, recordIen);
            ReadResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).read(request);

            return toSomething(response);
        }


        public Dictionary<String, Object> getTestByAccession(String accessionNumber)
        {
            if (String.IsNullOrEmpty(accessionNumber))
            {
                throw new ArgumentNullException("Must supply accession number");
            }
            String[] accessionPieces = StringUtils.split(accessionNumber, " ");
            if (accessionPieces.Length < 3)
            {
                throw new ArgumentException("Invalid accession number - must have at least 3 pieces");
            }

            Accession accession = this.getAccessionByAbbreviation(accessionPieces[0]);
            String accessionIen = accession.id;
            String adjustedDatePiece = this.adjustAccessionDateFromNumber(accessionPieces[1]);

            ReadRequest request = buildGetAccessionDetail(accessionIen, adjustedDatePiece, accessionPieces[2]);
            ReadResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).read(request);

            if (response == null || response.value == null || response.value.Count < 2)
            {
                throw new KeyNotFoundException("No record found with that accession number!");
            }

            return toSomething(response);
        }

        private Dictionary<String, Object> toSomething(ReadResponse response)
        {
            List<VistaRecord> record = response.getRecordsForFile("68.02");

            if (record == null || record.Count != 1)
            {
                throw new ArgumentException("Invalid response format from VistA - unable to parse accession");
            }

            Dictionary<String, String> responseDict = record[0].convertFieldsToDict(false); //   response.convertResponseToInternalDict();
            Dictionary<String, String> responseExternalDict = record[0].convertFieldsToDict(true); //  response.convertResponseToExternalDict();

            Patient p = new Patient(){ idSet = new IdentifierSet() { ids = new List<Identifier>() { new Identifier() { name = "LRDFN", id = DictionaryUtils.safeGet(responseDict, ".01") } } } };

            String accessionNumberFromRecord = DictionaryUtils.safeGet(responseDict, "15");
            DateTime vistaOrderDate = DateUtils.parseDateTime(DictionaryUtils.safeGet(responseDict, "3"), _cxn.getSource().timeZoneParsed);
            
            String providerId = DictionaryUtils.safeGet(responseDict, "6.5");
            String providerName = DictionaryUtils.safeGet(responseExternalDict, "6.5");
            User provider = new User() { id = providerId, nameString = providerName };

            String enteredById = DictionaryUtils.safeGet(responseDict, "6.7");
            String enteredByName = DictionaryUtils.safeGet(responseExternalDict, "6.7");
            User enteredBy = new User() { id = enteredById, nameString = enteredByName };

            DateTime specimenTakenDateTime = DateUtils.parseDateTime(DictionaryUtils.safeGet(responseDict, "9"), _cxn.getSource().timeZoneParsed);
            DateTime labArrivalTime = DateUtils.parseDateTime(DictionaryUtils.safeGet(responseDict, "12"), _cxn.getSource().timeZoneParsed);
            DateTime resultsAvailableDate = DateUtils.parseDateTime(DictionaryUtils.safeGet(responseDict, "13"), _cxn.getSource().timeZoneParsed);
            String orderNumber = DictionaryUtils.safeGet(responseDict, "14");
            String accessionNumber = DictionaryUtils.safeGet(responseDict, "15");
            String UID = DictionaryUtils.safeGet(responseDict, "16");

            Dictionary<String, object> resultAsDict = new Dictionary<string, object>();
            resultAsDict.Add("patient", p);
            resultAsDict.Add("accessionNumber", accessionNumberFromRecord);
            resultAsDict.Add("vistaOrderDate", vistaOrderDate);
            resultAsDict.Add("provider", provider);
            resultAsDict.Add("enteredBy", enteredBy);

            resultAsDict.Add("specimenTakenDateTime", specimenTakenDateTime);
            resultAsDict.Add("labArrivalTime", labArrivalTime);
            resultAsDict.Add("orderNumber", orderNumber);
            resultAsDict.Add("UID", UID);
            resultAsDict.Add("accession", accessionNumber);
            resultAsDict.Add("resultsAvailable", resultsAvailableDate);

            String orderingSiteId = DictionaryUtils.safeGet(responseDict, "16.1");
            String orderingSiteName = DictionaryUtils.safeGet(responseExternalDict, "16.1");
            if (!String.IsNullOrEmpty(orderingSiteId))
            {
                resultAsDict.Add("orderingSite", new Facility() { id = orderingSiteId, name = orderingSiteName });
            }

            String collectingSiteId = DictionaryUtils.safeGet(responseDict, "16.2");
            String collectingSiteName = DictionaryUtils.safeGet(responseExternalDict, "16.2");
            if (!String.IsNullOrEmpty(collectingSiteId))
            {
                resultAsDict.Add("collectingSite", new Facility() { id = collectingSiteId, name = collectingSiteName });
            }

            String divIEN = DictionaryUtils.safeGet(responseDict, "16.1"); // first look in sending site field as remote specimens have a value here
            if (String.IsNullOrEmpty(divIEN)) // if not a remote specimen, use DIV field 26
            {
                divIEN = DictionaryUtils.safeGet(responseDict, "26");
            }
            if (!String.IsNullOrEmpty(divIEN))
            {
                resultAsDict.Add("div", divIEN);
            }

            // turns out this is a multiple of the accession... not sure how it should match up with the tests if # of specimens > 1...
            // so... for now we're just concerning ourself with the first specimen!
            List<VistaRecord> specimenRecords = response.getRecordsForFile("68.05");
            if (specimenRecords != null && specimenRecords.Count > 0 && 
                specimenRecords[0].fields != null && specimenRecords[0].fields.Count > 0)
            {
                foreach (VistaField vf in specimenRecords[0].fields)
                {
                    if (vf.number == ".01")
                    {
                        resultAsDict.Add("specimen", new SpecimenType() { id = vf.value, name = vf.externalValue });
                        break;
                    }
                }
            }

            List<VistaRecord> vistaTestRecords = response.getRecordsForFile("68.04");
            if (vistaTestRecords != null && vistaTestRecords.Count > 0)
            {
                List<LabTest> tests = new List<LabTest>();
                foreach (VistaRecord testRecord in vistaTestRecords)
                {
                    if (!testRecord.fields.Any(vf => vf.number == ".01"))
                    {
                        continue;
                    }

                    VistaField testRecField = testRecord.fields.First(vf => vf.number == ".01");
                    LabTest currentTest = new LabTest() { id = testRecField.value, name = testRecField.externalValue };

                    if (testRecord.fields.Any(vf => vf.number == "4"))
                    {
                        currentTest.completed = DateUtils.parseDateTime(testRecord.fields.First(vf => vf.number == "4").value, _cxn.getSource().timeZoneParsed);
                    }
                    if (testRecord.fields.Any(vf => vf.number == "8.1"))
                    {
                        VistaField parentTestField = testRecord.fields.First(vf => vf.number == "8.1");
                        currentTest.parent = new LabTest() { id = parentTestField.value, name = parentTestField.externalValue };
                    }

                    tests.Add(currentTest);
                }
                resultAsDict.Add("tests", tests);
            }

            return resultAsDict;
        }

        internal string adjustAccessionDateFromNumber(String accessionNumberDatePiece)
        {
            if (accessionNumberDatePiece.Length == 2)
            {
                return String.Format("3{0}0000", accessionNumberDatePiece); // e.g. 3150000
            }
            else if (accessionNumberDatePiece.Length == 4) // note can only fetch accession for current year in this format!!
            {
                Int32 monthPiece = Int32.Parse(accessionNumberDatePiece.Substring(0, 2));
                DateTime vistaTime = DateTime.Now; // DateUtils.convertUTCToTimeZone(DateUtils.convertSystemTimeToUTC(), _cxn.getSource().timeZoneParsed);
                // if the current month is before the accession number month piece (e.g. it's Jan 15 but the accession number date piece is '0905' - September obviously hasn't happened this year yet so subtract one year)
                if (vistaTime.Month < monthPiece)
                {
                    return String.Format("3{0}{1}", (vistaTime.Year - 2000 - 1).ToString(), accessionNumberDatePiece); 
                }
                return String.Format("3{0}{1}", (vistaTime.Year - 2000).ToString(), accessionNumberDatePiece); // e.g. 3150920
            }

            throw new ArgumentException("TODO - need to improve accession number date conversion so more date formats are supported! Can't convert: " + accessionNumberDatePiece);
        }

        internal ReadRequest buildGetAccessionDetail(String accessionAreaIen, String adjustedDate, String number)
        {
            ReadRequest request = new ReadRequest(_cxn.getSource());
            request.setFile("68.02");
            request.setFields("**");
            request.setIens(String.Format("{0},{1},{2}", number, adjustedDate, accessionAreaIen)); // e.g. 27,3150914,37
            return request;
        }

        public List<Accession> getAccessionAreas()
        {
            throw new NotImplementedException();
            ReadRangeRequest request = buildGetAccessionAreasRequest();
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
          //  return toAccessionsAreas(response);
        }

        public Accession getAccessionByAbbreviation(String accessionAbbreviation)
        {
            Dictionary<String, Accession> accessionDict = getAccessions(accessionAbbreviation);
            if (!accessionDict.ContainsKey(accessionAbbreviation))
            {
                throw new ArgumentException(accessionAbbreviation + " not found!");
            }
            return accessionDict[accessionAbbreviation];
        }

        /// <summary>
        /// Get a dictionary of Accession organized by the abbreviation (NOT IEN!!)
        /// </summary>
        /// <returns></returns>
        public Dictionary<String, Accession> getAccessions(String ignoreDupesForAllExcept = null)
        {
            ReadRangeRequest request = buildGetAccessionsByAbbreviationRequest();
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toAccessionsByAbbreviation(response, ignoreDupesForAllExcept);
        }

        internal ReadRangeRequest buildGetAccessionsByAbbreviationRequest()
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setFile("68");
            request.setFields(".01;.09");
            return request;
        }

        internal ReadRangeRequest buildGetAccessionAreasRequest()
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setFile("68");
            return request;
        }

        internal List<Accession> toAccessionAreas(ReadRangeResponse response)
        {
            throw new NotImplementedException();
            List<Accession> result = new List<Accession>();

            foreach (String line in response.value)
            {
                String[] pieces = StringUtils.split(line, StringUtils.CARAT);

                Accession newAccession = new Accession();
                newAccession.id = pieces[0];
                newAccession.area = pieces[1];

               // result.Add(pieces[2], newAccession);
            }

            return result;
        }


        internal Dictionary<String, Accession> toAccessionsByAbbreviation(ReadRangeResponse response, String ignoreDupesForAllExcept = null)
        {
            Dictionary<String, Accession> result = new Dictionary<string, Accession>();

            foreach (String line in response.value)
            {
                String[] pieces = StringUtils.split(line, StringUtils.CARAT);

                Accession newAccession = new Accession();
                newAccession.id = pieces[0];
                newAccession.area = pieces[1];
                // Battle Creek has two records with same accession abbreviation - one has the full name "ZZ"d -
                // adding some logic here to check for that
                if (result.ContainsKey(pieces[2]))
                {
                    // turns out a small handful of sites have dupes... since we're often looking for a specific accession area, added this logic to 
                    // skip dupes if we pass in the area we're actually searching for and the dupe isn't that area
                    // note this means dupes WON'T BE ADDED to the dictionary here!!!
                    if (!String.IsNullOrEmpty(ignoreDupesForAllExcept) && !String.Equals(pieces[2], ignoreDupesForAllExcept, StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }

                    if (result[pieces[2]].area.StartsWith("ZZ")) // if we already added this key and the accession in the dict is ZZ'd then remove it
                    {
                        result.Remove(pieces[2]);
                    }
                    else if (newAccession.area.StartsWith("ZZ")) // if the previous wasn't ZZ'd but this one is, just continue
                    {
                        continue;
                    }
                    else
                    {
                        throw new ArgumentException("There are multiple ACCESSION records with the same abbreviation (" + pieces[2] + ") -- Unable to continue");
                    }
                }

                result.Add(pieces[2], newAccession);
            }

            return result;
        }

        /// <summary>
        /// Fetch the patient's DFN from the "B" cross reference (LRDFN) on the LAB DATA file (#63, ^LR) - 3rd piece
        /// </summary>
        /// <param name="lrdfn"></param>
        /// <returns></returns>
        public String getZeroNodeForLRDFN(String lrdfn)
        {
            try
            {
                // return StringUtils.piece(
                return new ToolsDaoFactory().getToolsDao(_cxn).gvv("$G(^LR($O(^LR(\"B\"," + lrdfn + ",\"\")),0))");
                //, "^", 3);
            }
            catch (Exception exc) // found LDR DFN 6787 in Durham that doesn't appear in the "B" cross reference - since IEN and .01 fields are often the same, try fetching DFN using LR DFN
            {
                String retryGvv = new ToolsDaoFactory().getToolsDao(_cxn).gvv("$G(^LR(" + lrdfn + ",0))");
                if (String.IsNullOrEmpty(retryGvv))
                {
                    throw exc;
                }

                if (StringUtils.piece(retryGvv, "^", 1) == lrdfn)
                {
                    //return StringUtils.piece(
                    return retryGvv;
                        //, "^", 3);
                }
                else
                {
                    throw exc;
                }
            }
        }

        public Patient getReferralPatient(String referralPatientId)
        {
            ReadRequest request = buildGetReferralPatientEequest(referralPatientId);
            ReadResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).read(request);
            return toReferralPatient(response);
        }

        internal Patient toReferralPatient(ReadResponse response)
        {
            Dictionary<String, String> responseDict = response.convertResponseToInternalDict();

            Patient result = new Patient();
            result.nameString = DictionaryUtils.safeGet(responseDict, ".01");
            result.dateOfBirthVistA = DictionaryUtils.safeGet(responseDict, ".03");
            if (!String.IsNullOrEmpty(result.dateOfBirthVistA))
            {
                result.dateOfBirth = DateUtils.toDateTime(result.dateOfBirthVistA, TimeZoneInfo.Utc);
            }
            result.idSet = new IdentifierSet();
            result.idSet.add(DictionaryUtils.safeGet(responseDict, ".09"), "SSN");
            result.idSet.add(DictionaryUtils.safeGet(responseDict, "63"), "LRDFN");

            return result;
        }

        internal ReadRequest buildGetReferralPatientEequest(String referralPatientId)
        {
            ReadRequest request = new ReadRequest(_cxn.getSource());
            request.setFile("67");
            request.setIens(referralPatientId);
            return request;
        }

        public String getLRDFNFromSSN(String ssn)
        {
            String dfn = new PatientSearchDao(_cxn).getPatientIDFromSSN(ssn);
            Patient patientRec = new PatientSearchDao(_cxn).getPatient(dfn);
            String lrdfn = patientRec.idSet.getByName("LRDFN").id;

            if (String.IsNullOrEmpty("lrdfn"))
            {
                throw new ArgumentException("Patient record does not appear to have a LRDFN!");
            }

            return lrdfn;
        }

        /// <summary>
        /// Fetch all chem labs for patient beginning at Jan 1, 1970
        /// </summary>
        /// <param name="lrdfn"></param>
        /// <returns></returns>
        public IList<LabTest> getChemLabsForPatient(String lrdfn)
        {
            return getChemLabsForPatient(lrdfn, new DateTime(1970, 1, 1));
        }

        public IList<LabTest> getChemLabsForPatient(String lrdfn, DateTime fromDate)
        {
            ReadRangeRequest request = buildGetChemLabsForPatientRequest(lrdfn, fromDate);
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toChemLabsForPatient(response);
        }

        internal ReadRangeRequest buildGetChemLabsForPatientRequest(String lrdfn, DateTime fromDate)
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setFile("63.04");
            request.setFrom(DateUtils.toVistaDate(fromDate));
            request.setIens(lrdfn);
            request.setFlags("IP");
            request.setCrossRef("B");
            request.setFields(".01;.03;.05;.06;.1;.11;.112");
            request.setIdentifierParam("N PNAME=$P(^(0),U,10),PNAME=$P(^VA(200,PDUZ,0),U,1) D EN^DDIOL(PDUZ_U_PNAME)");
           // request.setIdentifierParam("N GBL,IEN,SIEN,STIDX,IDX,VALS,X S X=$D(^(1)) D EN^DDIOL(X) S GBL=$NA(^(0)) I GBL'=\"\" S IEN=$QS(GBL,1) S SIEN=$QS(GBL,3) S STIDX=$O(^LR(IEN,\"CH\",SIEN,1)) I +STIDX'<1 S CNT=0 F IDX=STIDX:0 S CNT=CNT+1 S VALS=$G(^LR(IEN,\"CH\",SIEN,IDX)) D EN^DDIOL(IDX_\":\"_VALS) S IDX=$O(^LR(IEN,\"CH\",SIEN,IDX)) I ((+IDX<1)!(CNT>1000)) Q");
            return request;
        }

        internal IList<LabTest> toChemLabsForPatient(ReadRangeResponse response)
        {
            IList<LabTest> result = new List<LabTest>();
            IList<Specimen> specimens = new List<Specimen>();

            foreach (String line in response.value)
            {
                String[] pieces = StringUtils.split(line, StringUtils.CARAT);
                LabTest current = new LabTest();
                current.id = pieces[0];

                Specimen specimen = new Specimen();
                specimen.collectionDate = DateUtils.toDateTime(pieces[1], _cxn.getSource().timeZoneParsed);
                //specimen.requestedBy = pieces[
            }

            return result;
        }
    }
}