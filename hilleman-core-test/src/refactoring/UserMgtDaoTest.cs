using System;
using System.Collections.Generic;
using NUnit.Framework;
using com.bitscopic.hilleman.core.dao.vista;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.utils;

namespace com.bitscopic.hilleman.core.refactoring
{
    [TestFixture]
    public class UserMgtDaoTest
    {
        #region Setup/Teardown
        IVistaConnection _cxnToReturn;

        [TearDown]
        public void td()
        {
            if (_cxnToReturn != null)
            {
                TestHelper.returnConnection(_cxnToReturn);
            }
        }

        [OneTimeTearDown]
        public void tftd()
        {
            TestHelper.cleanupAfterAllTests();
        }
        #endregion

        #region User Search

        [Test]
        public void testFindUserByName()
        {
            String fileContents = FileIOUtils.readFile(@"Z:\downloads\users (1).csv");
            IList<String> lines = StringUtils.split(fileContents, "\r\n");

            foreach (String line in lines)
            {
                if (String.IsNullOrEmpty(line))
                {
                    continue;
                }

                String[] pieces = StringUtils.split(line, StringUtils.COMMA);
                if (pieces.Length < 4)
                {
                    continue;
                }

                String userId = pieces[0];
                String userFirst = pieces[1];
                String userLast = pieces[2];
                String email = pieces[3];
                String labId = pieces[4];

                try
                {
                    _cxnToReturn = TestHelper.getConnectionFromConnectionPool(labId);
                    List<User> potentialMatches = new UserMgtDao(_cxnToReturn).findUserByName(userLast.ToUpper() + "," + userFirst[0].ToString());
                    foreach (User u in potentialMatches)
                    {
                        if (u.nameString.Contains(userLast + ",") && u.firstName.Contains(userFirst[0].ToString())) 
                        {
                            System.Console.WriteLine(String.Format("{0}|{1}|{2}", u.nameString, u.idSet.getByName("SSN"), u.id));
                        }
                    }
                }
                catch (Exception)
                {
                    System.Console.WriteLine("Problem finding user...");
                }
                finally
                {
                    TestHelper.returnConnection(_cxnToReturn);
                }
            }
        }

        #endregion

        #region Menu Options For User

        [Test]
        public void testGetPrimaryMenuOption()
        {
            _cxnToReturn = TestHelper.getConnectionFromConnectionPool("901");
            String userId = "1";

            MenuOption option = new UserMgtDao(_cxnToReturn).getPrimaryMenuOption(userId);
            Assert.AreEqual(option.id, "10989");
            Assert.AreEqual(option.name, "OR CPRS GUI CHART");
        }

        [Test]
        public void testGetMenuOptions()
        {
            _cxnToReturn = TestHelper.getConnectionFromConnectionPool("901");
            String userId = "1";
            
            IList<MenuOption> options = new UserMgtDao(_cxnToReturn).getMenuOptions(userId);
            Assert.AreEqual(options.Count, 5);
        }

        #endregion

        #region Security Keys
        [Test]
        public void testRemoveSecurityKey()
        {
            String keyToRemove = "545";
            String userId = "1";
            _cxnToReturn = TestHelper.getConnectionFromConnectionPool("901");

            IList<SecurityKey> result = new UserMgtDao(_cxnToReturn).getUserSecurityKeys(userId);
            Assert.IsTrue(result.Count > 5, "This user is known to have more than 5 secirty keys!");

            new UserMgtDao(_cxnToReturn).removeSecurityKey(keyToRemove, userId);

            IList<SecurityKey> result2 = new UserMgtDao(_cxnToReturn).getUserSecurityKeys(userId);
            Assert.IsTrue(result.Count > result2.Count);
        }

        [Test]
        public void testGetVistaSecurityKeys()
        {
            _cxnToReturn = TestHelper.getConnectionFromConnectionPool("901");

            Dictionary<String, SecurityKey> vistaKeys = new UserMgtDao(_cxnToReturn).getVistaSecurityKeys();
            Assert.IsTrue(vistaKeys.Count > 100);
        }

        [Test]
        public void testGetUserSecurityKeys()
        {
            _cxnToReturn = TestHelper.getConnectionFromConnectionPool("901");

            String userId = "1";
            IList<SecurityKey> result = new UserMgtDao(_cxnToReturn).getUserSecurityKeys(userId);
            Assert.IsTrue(result.Count > 5, "This user is known to have more than 5 secirty keys!");
        }

        [Test]
        public void testAddSecurityKeyByName()
        {
            String userId = "1";
            String securityKeyName = "XUAUTHOR";
            String securityKeyId = "4";
            SecurityKey refKey = new SecurityKey() { id = securityKeyId, name = securityKeyName };

            _cxnToReturn = TestHelper.getConnectionFromConnectionPool("901");

            // we already gave this user this security key so let's remove it first
            new UserMgtDao(_cxnToReturn).removeSecurityKeyByName(securityKeyName, userId);

            // now fetch keys
            IList<SecurityKey> result = new UserMgtDao(_cxnToReturn).getUserSecurityKeys(userId);
            Assert.IsTrue(result.Count > 5, "This user is known to have more than 5 security keys!");

            // add it!
            new UserMgtDao(_cxnToReturn).addSecurityKeyByName(securityKeyName, userId, ".5");

            IList<SecurityKey> result2 = new UserMgtDao(_cxnToReturn).getUserSecurityKeys(userId);
            Assert.IsTrue(result2.Count == (result.Count + 1));

            foreach (SecurityKey key in result2)
            {
                if (key.name == securityKeyName)
                {
                    System.Console.WriteLine("Found " + key.name + " in user's keys!");
                    break;
                }
            }
        }

        [Test]
        public void testAddSecurityKey()
        {
            _cxnToReturn = TestHelper.getConnectionFromConnectionPool("901");

            String userId = "1";
            IList<SecurityKey> result = new UserMgtDao(_cxnToReturn).getUserSecurityKeys(userId);
            Assert.IsTrue(result.Count > 5, "This user is known to have more than 5 secirty keys!");

            // find a key user doesn't have
            String keyIdToAdd = "";
            Dictionary<String, SecurityKey> vistaKeys = new UserMgtDao(_cxnToReturn).getVistaSecurityKeys();
            foreach (String key in vistaKeys.Keys)
            {
                bool hasThisKey = false;
                foreach (SecurityKey userKey in result)
                {
                    if (userKey.id == key)
                    {
                        hasThisKey = true;
                        break; // already has it!
                    }
                }

                if (!hasThisKey)
                {
                    System.Console.WriteLine(String.Format("Looks like user doesn't have {0} (ID: {1}) - Gonna add it!", vistaKeys[key].name, key));
                    keyIdToAdd = key;
                    break;
                }
            }

            // and add it!
            new UserMgtDao(_cxnToReturn).addSecurityKey(keyIdToAdd, userId, ".5");

            IList<SecurityKey> result2 = new UserMgtDao(_cxnToReturn).getUserSecurityKeys(userId);
            Assert.IsTrue(result2.Count == (result.Count + 1));
            
            foreach (SecurityKey key in result2)
            {
                if (key.id == keyIdToAdd)
                {
                    System.Console.WriteLine("Found " + key.name + " in user's keys!");
                    break;
                }
            }

        }

        #endregion
    }
}
