using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.domain.session;
using com.bitscopic.hilleman.core.dao;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.domain.security;
using System.Configuration;
using com.bitscopic.hilleman.core.dao.vista.rpc;
using com.bitscopic.hilleman.core.dao.vista;
using com.bitscopic.hilleman.core.utils;

namespace com.bitscopic.hilleman.core.refactoring
{
    public class UserMgtDao : IRefactoringApi
    {
        IVistaConnection _cxn;

        public UserMgtDao(IVistaConnection cxn)
        {
            _cxn = cxn;
        }

        public void setTarget(IVistaConnection target)
        {
            _cxn = target;
        }

        /// <summary>
        /// Fetch a user by ID (the user's IEN in the NEW PERSON file - aka their "DUZ")
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public User getUser(String userId)
        {
            ReadRequest request = buildGetUserRequest(userId);
            ReadResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).read(request);
            return toUser(response);
        }

        internal ReadRequest buildGetUserRequest(string userId)
        {
            ReadRequest request = new ReadRequest(_cxn.getSource());
            request.setFile("200");
            request.setIens(userId);
            return request;
        }

        internal User toUser(ReadResponse response)
        {
            Dictionary<String, String> responseInternalDict = response.convertResponseToInternalDict();

            User result = new User();
            result.nameString = DictionaryUtils.safeGet(responseInternalDict, ".01");
            result.idSet = new IdentifierSet();
            result.idSet.add(DictionaryUtils.safeGet(responseInternalDict, "IEN"), "DUZ");
            result.idSet.add(DictionaryUtils.safeGet(responseInternalDict, "9"), "SSN");
            result.phones.Add("OFFICE", DictionaryUtils.safeGet(responseInternalDict, ".132"));
            return result;
        }


        public List<User> findUserByName(String target)
        {
            ReadRangeRequest request = buildFindUserByNameRequest(target);
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toUsers(response);
        }

        internal ReadRangeRequest buildFindUserByNameRequest(string target)
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setFile("200");
            request.setFields(".01;.132;5;9");
            request.setCrossRef("B");
            request.setMax("44");
            request.setFrom(target);
            return request;
        }

        internal List<User> toUsers(ReadRangeResponse response)
        {
            List<User> result = new List<User>();

            foreach (String line in response.value)
            {
                if (String.IsNullOrEmpty(line))
                {
                    continue;
                }
                String[] pieces = StringUtils.split(line, StringUtils.CARAT);
                if (pieces == null || pieces.Length < 4)
                {
                    continue;
                }

                User u = new User();
                u.id = pieces[0];
                u.nameString = pieces[1];
                u.phones.Add("office", pieces[2]);
                u.dateOfBirthVistA = pieces[3];
                u.idSet = new IdentifierSet();
                u.idSet.add(new Identifier() { id = pieces[4], name = "SSN" });
                u.idSet.add(new Identifier() { id = pieces[0], name = "DUZ", sourceSystemId = _cxn.getSource().id });

                result.Add(u);
            }

            return result;
        }

        #region Menu Options For Users

        public MenuOption getPrimaryMenuOption(String userId)
        {
            ReadRequest request = buildGetPrimaryMenuOptionRequest(userId);
            ReadResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).read(request);
            return toPrimaryMenuOption(response);
        }

        internal ReadRequest buildGetPrimaryMenuOptionRequest(String userId)
        {
            ReadRequest request = new ReadRequest(_cxn.getSource());
            request.setIens(userId);
            request.setFile("200");
            request.setFields("201");
            request.setFlags("IE");
            return request;
        }

        internal MenuOption toPrimaryMenuOption(ReadResponse readResponse)
        {
            if (readResponse == null || readResponse.value == null || readResponse.value.Count == 0)
            {
                return null;
            }

            Dictionary<String, String> responseExternalDict = readResponse.convertResponseToExternalDict();
            Dictionary<String, String> responseInternalDict = readResponse.convertResponseToInternalDict();

            return new MenuOption() { id = responseInternalDict["201"], name = responseExternalDict["201"], isPrimary = true };
        }

        public IList<MenuOption> getMenuOptions(String userId)
        {
            ReadRangeRequest request = buildGetMenuOptionsRequest(userId);
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toMenuOptions(response);
        }

        internal ReadRangeRequest buildGetMenuOptionsRequest(String userId)
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setFile("200.03");
            request.setFields(".01");
            request.setIens(userId);
            return request;
        }

        internal IList<MenuOption> toMenuOptions(ReadRangeResponse response)
        {
            IList<MenuOption> options = new List<MenuOption>();

            Dictionary<String, String> lookupTable = LookupTableUtils.getLookupTable(_cxn, "19");

            foreach (String line in response.value)
            {
                if (String.IsNullOrEmpty(line))
                {
                    continue;
                }
                String[] pieces = StringUtils.split(line, StringUtils.CARAT);
                if (pieces == null || pieces.Length < 2 || !lookupTable.ContainsKey(pieces[0]))
                {
                    continue;
                }

                options.Add(new MenuOption() { id = pieces[1], name = lookupTable[pieces[1]] });
            }

            return options;
        }

        #endregion

        #region Security Keys

        public void removeSecurityKeyByName(String keyName, String userId)
        {
            SecurityKey keyToRemove = this.getSecurityKeyByName(keyName);
            this.removeSecurityKey(keyToRemove.id, userId);
        }

        public void removeSecurityKey(String keyId, String userId)
        {
            DeleteRequest request = buildRemoveSecurityKeyRequest(keyId, userId);
            new CrrudDaoFactory().getCrrudDao(_cxn).delete(request);
        }

        internal DeleteRequest buildRemoveSecurityKeyRequest(String keyId, String userId)
        {
            DeleteRequest request = new DeleteRequest(_cxn.getSource());
            request.setFile("200.051");
            request.setIens(keyId + "," + userId);
            return request;
        }

        public IList<SecurityKey> getUserSecurityKeys(String userId)
        {
            ReadRangeRequest request = buildGetUserSecurityKeysRequest(userId);
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(request);
            return toUserSecurityKeys(response);
        }

        internal ReadRangeRequest buildGetUserSecurityKeysRequest(String userId)
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setFile("200.051");
            request.setIens(userId);
            request.setFields(".01");
            return request;
        }

        /// <summary>
        /// Parses ReadRangeResponse --AND-- fetches security key lookup table and uses it to supplement key names
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        internal IList<SecurityKey> toUserSecurityKeys(ReadRangeResponse response)
        {
            IList<SecurityKey> result = new List<SecurityKey>();
            Dictionary<String, SecurityKey> lookupTable = this.getVistaSecurityKeys();

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

                if (!lookupTable.ContainsKey(pieces[1])) // don't add key if IEN isn't present in lookup table
                {
                    continue;
                }

                result.Add(lookupTable[pieces[1]]);
            }

            return result;
        }

        public SecurityKey getSecurityKeyByName(String keyName)
        {
            Dictionary<String, SecurityKey> allKeys = this.getVistaSecurityKeys();
            foreach (SecurityKey val in allKeys.Values)
            {
                if (String.Equals(val.name, keyName))
                {
                    return val;
                }
            }
            throw new KeyNotFoundException(keyName + " not found!");
        }

        public Dictionary<String, SecurityKey> getVistaSecurityKeys()
        {
            Dictionary<String, String> lookupTable = LookupTableUtils.getLookupTable(_cxn, "19.1");
            Dictionary<String, SecurityKey> result = new Dictionary<string, SecurityKey>();
            foreach (String key in lookupTable.Keys)
            {
                result.Add(key, new SecurityKey() { id = key, sourceSystemId = _cxn.getSource().id, name = lookupTable[key] });
            }
            return result;
        }

        public String addSecurityKeyByName(String keyName, String userId, String grantedBy)
        {
            SecurityKey keyToAdd = this.getSecurityKeyByName(keyName);
            return addSecurityKey(keyToAdd.id, userId, grantedBy);
        }

        public String addSecurityKey(String keyId, String userId, String grantedBy)
        {
            CreateRequest request = buildAddSecurityKeyRequest(keyId, userId, grantedBy);
            CreateResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).create(request);
            return toNewSecurityKeyId(response);
        }

        internal string toNewSecurityKeyId(CreateResponse response)
        {
            return response.value[0];
        }

        internal CreateRequest buildAddSecurityKeyRequest(String keyId, String userId, String grantedBy)
        {
            CreateRequest request = new CreateRequest(_cxn.getSource());
            request.setFile("200.051");
            request.setExactIens(true);
            request.setIens(CreateRequest.FILER_FIND_OR_CREATE + keyId + "," + userId);
            request.addFieldAndValue(".01", keyId);
            request.addFieldAndValue("1", grantedBy);
            // don't need a time component so no need to add VistA directly for it's current time - can simply convert system time
            request.addFieldAndValue("2", DateUtils.toVistaDate(DateUtils.getVistaSystemTimeWithTimeZoneConversion(_cxn.getSource().timeZoneParsed)));

            return request;
        }

        #endregion

        #region Authentication
        // TODO - refactor to more flexibly handle source system types
        public User validateCredentials(Credentials credentials)
        {
            if (credentials.permission == null || String.IsNullOrEmpty(credentials.permission.name))
            {
                credentials.permission = new Permission("", MyConfigurationManager.getValue("SchedulerKey"));
            }
            if (credentials.provider == null || String.IsNullOrEmpty(credentials.provider.id))
            {
                throw new ArgumentException("Missing source system");
            }

            SourceSystem targetSrc = new SourceSystemTable(MyConfigurationManager.getValue("SourceSystemTable")).getSourceSystem(credentials.provider.id);

            User loggedInUser = null;
            String sst = ""; // serialized symbol table

            // weird!! hacked this in here when trying to use SQLite cache for vista queries... working ok but not pretty. consider refactoring in a better solution
            if (targetSrc.type == SourceSystemType.SQLITE_CACHE)
            {
                if (credentials.username == targetSrc.credentials.username && credentials.password == targetSrc.credentials.password)
                {
                    loggedInUser = new User() { nameString = "SQLite User", id = "1" };
                    _cxn = new com.bitscopic.hilleman.core.dao.vista.sql.sqlite.VistaSqliteCacheConnection(targetSrc);
                }
                else
                {
                    throw new UnauthorizedAccessException("Invalid credentials for SQLite Cache");
                }
            }

            else
            {
                _cxn = new VistaRpcConnection(targetSrc);
                _cxn.connect();
                loggedInUser = new VistaRpcCrrudDao(_cxn).login(new VistaRpcLoginCredentials()
                {
                    provider = new SourceSystem() { id = credentials.provider.id, name = credentials.provider.name },
                    username = credentials.username,
                    password = credentials.password
                }, new VistaRpcConnectionBrokerContext("", credentials.permission.name));
                sst = new VistaRpcToolsDao(_cxn).serializeSymbolTable();
                _cxn.disconnect(); // not using connection for anything else!
            }

            HillemanSession mySession = new HillemanSession() 
            { 
                endUser = loggedInUser,
                plaintextSerializedSymbolTable = sst,
                encryptedSerializedSymbolTable = CryptographyUtils.hmac256Hash(MyConfigurationManager.getValue("EncryptionKey"), sst)
            };
            mySession.addAuthorizedConnection(_cxn);
            Token userToken = TokenStoreFactory.getTokenStore().createNewToken(mySession);
            mySession.sessionToken = userToken.value;

            loggedInUser.token = userToken;

            return loggedInUser;
        }

        #endregion
    }
}