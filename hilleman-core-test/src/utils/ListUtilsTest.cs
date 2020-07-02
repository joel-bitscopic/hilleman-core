using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace com.bitscopic.hilleman.core.utils
{
    [TestFixture]
    public class ListUtilsTest
    {
        [Test]
        public void testSplitInChunks()
        {
            List<String> list = new List<string>() { "a", "b", "c", "d", "e", "f", "g", "h" };
            List<List<String>> chunked = ListUtils.splitInChunks(list, 2);

            foreach (List<String> chunkedList in chunked)
            {
                Assert.IsTrue(chunkedList.Count == 2); // 8 items in list - all should have 2
            }
        }

        [Test]
        public void testJoinTwoNullLists()
        {
            Assert.IsNotNull(ListUtils.join<Int16>(null, null));
        }

        [Test]
        public void testJoinSecondNullList()
        {
            Assert.IsNotNull(ListUtils.join<Int16>(new List<Int16>() { 8 }, null));
            Assert.AreEqual(1, (ListUtils.join<Int16>(new List<Int16>() { 8 }, null)).Count);
            Assert.AreEqual(8, (ListUtils.join<Int16>(new List<Int16>() { 8 }, null))[0]);
        }

        [Test]
        public void testJoinFirstNullList()
        {
            Assert.IsNotNull(ListUtils.join<Int16>(null, new List<Int16>() { 8 }));
            Assert.AreEqual(1, (ListUtils.join<Int16>(null, new List<Int16>() { 8 })).Count);
            Assert.AreEqual(8, (ListUtils.join<Int16>(null, new List<Int16>() { 8 }))[0]);
        }

        [Test]
        public void testJoinStringLists()
        {
            Assert.IsNotNull(ListUtils.join<String>(new List<String>() { "one fish" }, new List<String>() { "two fish" }));
            Assert.AreEqual(2, (ListUtils.join<String>(new List<String>() { "one fish" }, new List<String>() { "two fish" })).Count);
            Assert.AreEqual("one fish", (ListUtils.join<String>(new List<String>() { "one fish" }, new List<String>() { "two fish" }))[0]);
            Assert.AreEqual("two fish", (ListUtils.join<String>(new List<String>() { "one fish" }, new List<String>() { "two fish" }))[1]);
        }
    }
}
