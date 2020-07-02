using System;
using System.Collections.Generic;
using NUnit.Framework;
using com.bitscopic.hilleman.core.domain;

namespace com.bitscopic.hilleman.core.utils
{
    [TestFixture]
    public class BaseClassUtilsTest
    {
        [Test]
        public void testDefaultCaseInsensitiveDicts()
        {
            Person p1 = new Person();
            p1.phones.Add("work", "555 867 5309");
            p1.phones.Add("OFFICE", "911 867 5309");
            p1.phones.Add("Cell", "999 867 5309");

            Assert.IsTrue(p1.phones.ContainsKey("work"));
            Assert.IsTrue(p1.phones.ContainsKey("woRK"));
            Assert.IsTrue(p1.phones.ContainsKey("WORK"));
            Assert.IsTrue(p1.phones.ContainsKey("cell"));
            Assert.IsTrue(p1.phones.ContainsKey("CELL"));
            Assert.IsTrue(p1.phones.ContainsKey("office"));
        }


        [Test]
        public void testMatch()
        {
            IList<Person> people = new List<Person>()
            {
                new Person() { id = "1" },
                new Person() { id = "2" }
            };

            Person matchResult = BaseClassUtils.matchById<Person>(people, "2");

            Assert.IsNotNull(matchResult);
            Assert.IsTrue(String.Equals("2", matchResult.id));
        }

        [Test]
        public void testMatchNotFound()
        {
            IList<Person> people = new List<Person>()
            {
                new Person() { id = "1" },
                new Person() { id = "2" }
            };

            Person matchResult = BaseClassUtils.matchById<Person>(people, "3");

            Assert.IsNull(matchResult);
        }

        [Test]
        //[ExpectedException(typeof(InvalidCastException))]
        public void testMatchNonBaseClassImpl()
        {
            IList<String> stringList = new List<String>() // String objects aren't BaseClass implementations
            {
                "1",
                "2"
            };

            Assert.Throws<InvalidCastException>(() => BaseClassUtils.matchById<String>(stringList, "2"));
        }


    }
}
