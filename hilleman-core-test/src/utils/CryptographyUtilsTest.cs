using System;
using NUnit.Framework;
using com.bitscopic.hilleman.core.domain.security;
using com.bitscopic.hilleman.core.domain;
namespace com.bitscopic.hilleman.core.utils
{
    [TestFixture]
    public class CryptographyUtilsTest
    {
        [Test]
        public void testCreateRandomBase64Hash()
        {
            // performance is ~ 80-90K random hashes/second on VM w 4 GB memory, 2012 CPU
            for (int i = 0; i < 10000; i++)
            {
                String result = CryptographyUtils.createRandomHashBase64();
                Assert.IsFalse(String.IsNullOrEmpty(result));
                Assert.IsTrue(result.Length > 30);
            }
        }

        [Test]
        public void testHashObject()
        {
            String key = "2468;WDWA??";
            
            Token myToken = new Token()
            {
                immutableExpiration = new DateTime(2017, 12, 31),
                issued = new DateTime(2017, 6, 26),
                lastAccessed = DateTime.Now,
                maxAccesses = Int32.MinValue,
                value = CryptographyUtils.createRandomHashBase64()
            };
            
            String result = CryptographyUtils.hmac256Hash(key, myToken);

           // System.Console.Write(SerializerUtils.serializeForPrinting(result));
            Assert.IsFalse(String.IsNullOrEmpty(result));
            Assert.IsTrue(result.Length > 30);
        }

        [Test]
        public void testHashUniqueness()
        {
            String key = "It's a secret!";
            
            Token myToken1 = new Token()
            {
                immutableExpiration = DateTime.Now.AddYears(1),
                issued = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, 1)),
                lastAccessed = DateTime.Now,
                maxAccesses = Int32.MinValue,
                state = new User() { id = "1", nameString = "MEWTON,JOEL" },
                value = CryptographyUtils.createRandomHashBase64() // note this string *should* be different resulting in different hashes...
            };

            Token myToken2 = new Token()
            {
                immutableExpiration = DateTime.Now.AddYears(1),
                issued = DateTime.Now,
                lastAccessed = DateTime.Now,
                maxAccesses = Int32.MinValue,
                state = new User() { id = "1", nameString = "MEWTON,JOEL" },
                value = CryptographyUtils.createRandomHashBase64() // note this string *should* be different resulting in different hashes...
            };

            String result1 = CryptographyUtils.hmac256Hash(key, myToken1);
            String result2 = CryptographyUtils.hmac256Hash(key, myToken2);
            Assert.AreNotEqual(result1, result2, 
                String.Format("Should have been different... Maybe token values are the same: token1.value = {0}, token2.value = {1}", myToken1.value, myToken2.value));
        }

        [Test]
        public void testHashSameness()
        {
            String key = "It's a secret!";
            DateTime oneTime = DateTime.Now;
            Token myToken1 = new Token()
            {
                immutableExpiration = oneTime.AddDays(1),
                issued = oneTime,
                lastAccessed = oneTime,
                maxAccesses = Int32.MinValue,
                state = new User() { id = "1", nameString = "MEWTON,JOEL" },
                value = "Keeping this the same!"
            };
            String result1 = CryptographyUtils.hmac256Hash(key, myToken1);

            for (int i = 0; i < 100000; i++)
            {

                Token myToken2 = new Token()
                {
                    immutableExpiration = oneTime.AddDays(1),
                    issued = oneTime,
                    lastAccessed = oneTime,
                    maxAccesses = Int32.MinValue,
                    state = new User() { id = "1", nameString = "MEWTON,JOEL" },
                    value = "Keeping this the same!"
                };

                String result2 = CryptographyUtils.hmac256Hash(key, myToken2);
                Assert.AreEqual(result1, result2);
            }
        }

        [Test]
        public void testSha256Hashing()
        {
            String target = "\"Mmmm... Machine gun bacon!\", muttered the sociopath.";
            String hashed = CryptographyUtils.sha256Hash(target);
            Assert.AreEqual("57e91bf10f58c42e82db6fa2c91feec3297ad9250067ef7124b8c9ce51e99dbf", hashed);
        }

        [Test]
        public void testSha256HashingSimilarityToVista()
        {
            String target = "FIRST TRY";
            String hashed = CryptographyUtils.sha256HashBase64Encoded(target);
            Assert.AreEqual("Q7yfJxGgRDB5iDUh766EQD9P00bG2esQbMAYASfBPVo=", hashed);
        }
    }
}
