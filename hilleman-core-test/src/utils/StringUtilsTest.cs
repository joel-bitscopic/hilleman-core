using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Text.RegularExpressions;
using System.Linq;

namespace com.bitscopic.hilleman.core.utils
{
    public class IdAndText
    {
        public IdAndText() { }

        public String id;
        public String text;

        public override bool Equals(object obj)
        {
            return String.Equals(this.id, ((IdAndText)obj).id);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class IdAndTextEqualityComparer : IEqualityComparer<IdAndText>
    {
        public bool Equals(IdAndText x, IdAndText y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(IdAndText obj)
        {
            return obj.GetHashCode();
        }
    }

    [TestFixture]
    public class StringUtilsTest
    {
        [Test]
        public void testWildcardMatch()
        {
            String wildcardStr = "*";
            Assert.IsFalse(StringUtils.isWildcardMatch(wildcardStr, null));
            Assert.IsTrue(StringUtils.isWildcardMatch(wildcardStr, "anything"));
            Assert.IsTrue(StringUtils.isWildcardMatch(wildcardStr, "123"));

            wildcardStr = "*.txt";
            Assert.IsFalse(StringUtils.isWildcardMatch(wildcardStr, null));
            Assert.IsTrue(StringUtils.isWildcardMatch(wildcardStr, "file.txt"));
            Assert.IsFalse(StringUtils.isWildcardMatch(wildcardStr, "file.xls"));
            Assert.IsFalse(StringUtils.isWildcardMatch(wildcardStr, ".txt2"));

            wildcardStr = "*corona*";
            Assert.IsFalse(StringUtils.isWildcardMatch(wildcardStr, "virus"));
            Assert.IsTrue(StringUtils.isWildcardMatch(wildcardStr, "NOVEL CORONAVIRUS"));
            Assert.IsTrue(StringUtils.isWildcardMatch(wildcardStr, "2019-CORONAVIRUS"));

            wildcardStr = "*novel*corona*";
            Assert.IsFalse(StringUtils.isWildcardMatch(wildcardStr, "novel"));
            Assert.IsFalse(StringUtils.isWildcardMatch(wildcardStr, "coronavirus"));
            Assert.IsTrue(StringUtils.isWildcardMatch(wildcardStr, "NOVEL CORONAVIRUS"));
            Assert.IsTrue(StringUtils.isWildcardMatch(wildcardStr, "NOVEL CORONAVIRUS-2019"));
        }

        [Test]
        public void testTrimLeading()
        {
            Assert.AreEqual("foo", StringUtils.trimLeading("foo", '?'));
            Assert.AreEqual("az", StringUtils.trimLeading("baz", 'b'));
            Assert.AreEqual("h la la", StringUtils.trimLeading("oooh la la", 'o'));
            Assert.AreEqual("oop", StringUtils.trimLeading("poop", 'p'));
            Assert.AreEqual("25", StringUtils.trimLeading("0025", '0'));
            Assert.AreEqual(null, StringUtils.trimLeading(null, '0'));
        }

        [Test]
        public void testGetLines()
        {
            Assert.IsNull(StringUtils.getLines(null));
            Assert.IsNotEmpty(StringUtils.getLines("A string with one line"));
            Assert.AreEqual(2, StringUtils.getLines("A string with\r\ntwo lines using CR/LF").Length);
            Assert.AreEqual(2, StringUtils.getLines("A string with\ntwo lines using newline").Length);
            Assert.AreEqual(2, StringUtils.getLines("A string with\rtwo lines using carriage return").Length);

            Assert.AreEqual(4, StringUtils.getLines("A string with \r 4 lines using\n multiple delimiters \r\n").Length);
        }


        [Test]
        public void testExtractQuoteStrings()
        {
            Assert.IsEmpty(StringUtils.extractQuotedStrings(null));
            Assert.IsEmpty(StringUtils.extractQuotedStrings(String.Empty));
            Assert.IsEmpty(StringUtils.extractQuotedStrings(""));

            Assert.IsEmpty(StringUtils.extractQuotedStrings("a string with no quotes"));
            Assert.IsEmpty(StringUtils.extractQuotedStrings("s"));
            Assert.IsEmpty(StringUtils.extractQuotedStrings("string"));

            Assert.IsEmpty(StringUtils.extractQuotedStrings("a string with one\" quotes"));

            //joel-aug7 2018 -- Assert.IsNullOrEmpty(StringUtils.extractQuotedStrings("a string with empty \"\" double quotes")[0]);
            Assert.AreEqual(" ", StringUtils.extractQuotedStrings("a string with single space in \" \" double quotes")[0]);

            Assert.AreEqual("in", StringUtils.extractQuotedStrings("a string with single word \"in\" double quotes")[0]);
            Assert.AreEqual("end", StringUtils.extractQuotedStrings("a string with quotes at the \"end\"")[0]);
            Assert.AreEqual("starting", StringUtils.extractQuotedStrings("\"starting\" a string with quotes")[0]);

            Assert.AreEqual("starting a string", StringUtils.extractQuotedStrings("\"starting a string\" with quotes including multiple words")[0]);
            Assert.AreEqual("zika virus igm", StringUtils.extractQuotedStrings("\"zika virus igm\"")[0]);

            Assert.AreEqual("orphaned quotes", StringUtils.extractQuotedStrings("\"orphaned quotes\" shouldn't \" be ignored")[0]);

            IList<String> list = StringUtils.extractQuotedStrings("\"starting a string\" \"with quotes\" and including multiple \"words\"");
            Assert.AreEqual(list.Count, 3);
            Assert.AreEqual(list[0], "starting a string");
            Assert.AreEqual(list[1], "with quotes");
            Assert.AreEqual(list[2], "words");

            String searchTarget = "\"starting a string\" \"with quotes\" and including multiple \"words\"";
            IList<String> searchTargets = StringUtils.splitToList(searchTarget, new String[] { " " }, StringSplitOptions.RemoveEmptyEntries, true);

            if (searchTarget.Contains("\"")) // if our search contains quotes, we should use the quoted string as one search criteria
            {
                if (!(searchTarget.Count(c => c == '"') % 2 == 0))
                {
                    throw new ArgumentException("Invalid search criteria - use matching quotes");
                }

                IList<String> quotedSearchTargets = StringUtils.extractQuotedStrings(searchTarget);
                foreach (String s in quotedSearchTargets) // now remove all quoted targets from searchTarget string, re-split it and add the lists together
                {
                    if (String.IsNullOrEmpty(s) || String.IsNullOrEmpty(s.Trim()))
                    {
                        continue;
                    }
                    searchTarget = searchTarget.Replace(s, ""); // "this is" a quote "set of" strings -> a quote strings
                }
                searchTarget = searchTarget.Replace("\"", "");
                searchTargets = quotedSearchTargets.Concat(StringUtils.splitToList(searchTarget, new String[] { " " }, StringSplitOptions.RemoveEmptyEntries, true)).ToList();
            }

            Assert.AreEqual(searchTargets.Count, 6);
        }

        [Test]
        public void testToDictFromDelim()
        {
            String delimitedStr = "sEcho=2&iColumns=15&sColumns=%2C%2C%2C%2C%2C%2C%2C%2C%2C%2C%2C%2C%2C%2C&iDisplayStart=0&iDisplayLength=10&mDataProp_0=0&sSearch_0=&bRegex_0=false&bSearchable_0=false&bSortable_0=true&mDataProp_1=1&sSearch_1=&bRegex_1=false&bSearchable_1=false&bSortable_1=true&mDataProp_2=2&sSearch_2=&bRegex_2=false&bSearchable_2=false&bSortable_2=true&mDataProp_3=3&sSearch_3=&bRegex_3=false&bSearchable_3=false&bSortable_3=true&mDataProp_4=4&sSearch_4=&bRegex_4=false&bSearchable_4=false&bSortable_4=true&mDataProp_5=5&sSearch_5=&bRegex_5=false&bSearchable_5=false&bSortable_5=true&mDataProp_6=6&sSearch_6=&bRegex_6=false&bSearchable_6=false&bSortable_6=true&mDataProp_7=7&sSearch_7=&bRegex_7=false&bSearchable_7=false&bSortable_7=true&mDataProp_8=8&sSearch_8=&bRegex_8=false&bSearchable_8=false&bSortable_8=true&mDataProp_9=9&sSearch_9=&bRegex_9=false&bSearchable_9=false&bSortable_9=true&mDataProp_10=10&sSearch_10=&bRegex_10=false&bSearchable_10=false&bSortable_10=true&mDataProp_11=11&sSearch_11=&bRegex_11=false&bSearchable_11=false&bSortable_11=true&mDataProp_12=12&sSearch_12=&bRegex_12=false&bSearchable_12=false&bSortable_12=true&mDataProp_13=13&sSearch_13=&bRegex_13=false&bSearchable_13=true&bSortable_13=true&mDataProp_14=14&sSearch_14=&bRegex_14=false&bSearchable_14=true&bSortable_14=true&sSearch=&bRegex=false&iSortCol_0=7&sSortDir_0=desc&iSortCol_1=0&sSortDir_1=desc&iSortingCols=2";

            Dictionary<String, String> dict = StringUtils.toDictFromDelimited(delimitedStr, "&", "=");

            Assert.IsTrue(dict.ContainsKey("sEcho"));
            Assert.AreEqual(dict["sEcho"], "2");
            Assert.IsTrue(dict.ContainsKey("iColumns"));
            Assert.AreEqual(dict["iColumns"], "15");
            Assert.IsTrue(dict.ContainsKey("sColumns"));
            Assert.AreEqual(dict["sColumns"], "%2C%2C%2C%2C%2C%2C%2C%2C%2C%2C%2C%2C%2C%2C");
            Assert.IsTrue(dict.ContainsKey("iDisplayStart"));
            Assert.AreEqual(dict["iDisplayStart"], "0");
            Assert.IsTrue(dict.ContainsKey("iDisplayLength"));
            Assert.AreEqual(dict["iDisplayLength"], "10");
            Assert.IsTrue(dict.ContainsKey("mDataProp_0"));
            Assert.AreEqual(dict["mDataProp_0"], "0");
            Assert.IsTrue(dict.ContainsKey("sSearch_0"));
            Assert.AreEqual(dict["sSearch_0"], "");
            Assert.IsTrue(dict.ContainsKey("bRegex_0"));
            Assert.AreEqual(dict["bRegex_0"], "false");
            Assert.IsTrue(dict.ContainsKey("bSearchable_0"));
            Assert.AreEqual(dict["bSearchable_0"], "false");
            Assert.IsTrue(dict.ContainsKey("bRegex_3"));
            Assert.AreEqual(dict["bRegex_3"], "false");
            Assert.IsTrue(dict.ContainsKey("bSearchable_3"));
            Assert.AreEqual(dict["bSearchable_3"], "false");
            Assert.IsTrue(dict.ContainsKey("bSortable_3"));
            Assert.AreEqual(dict["bSortable_3"], "true");
            Assert.IsTrue(dict.ContainsKey("mDataProp_4"));
            Assert.AreEqual(dict["mDataProp_4"], "4");
            Assert.IsTrue(dict.ContainsKey("sSearch_4"));
            Assert.AreEqual(dict["sSearch_4"], "");
            Assert.IsTrue(dict.ContainsKey("bRegex_4"));
            Assert.AreEqual(dict["bRegex_4"], "false");
            Assert.IsTrue(dict.ContainsKey("bSearchable_4"));
            Assert.AreEqual(dict["bSearchable_4"], "false");
            Assert.IsTrue(dict.ContainsKey("bSortable_4"));
            Assert.AreEqual(dict["bSortable_4"], "true");
            Assert.IsTrue(dict.ContainsKey("mDataProp_5"));
            Assert.AreEqual(dict["mDataProp_5"], "5");
            Assert.IsTrue(dict.ContainsKey("sSearch_5"));
            Assert.AreEqual(dict["sSearch_5"], "");
            Assert.IsTrue(dict.ContainsKey("bRegex_5"));
            Assert.AreEqual(dict["bRegex_5"], "false");
            Assert.IsTrue(dict.ContainsKey("bSearchable_5"));
            Assert.AreEqual(dict["bSearchable_5"], "false");
            Assert.AreEqual(dict["iSortCol_0"], "7");
            Assert.IsTrue(dict.ContainsKey("sSortDir_0"));
            Assert.AreEqual(dict["sSortDir_0"], "desc");
            Assert.IsTrue(dict.ContainsKey("iSortCol_1"));
            Assert.AreEqual(dict["iSortCol_1"], "0");
            Assert.IsTrue(dict.ContainsKey("sSortDir_1"));
            Assert.AreEqual(dict["sSortDir_1"], "desc");
            Assert.IsTrue(dict.ContainsKey("iSortingCols"));
            Assert.AreEqual(dict["iSortingCols"], "2");
        }

        [Test]
        public void testRegex()
        {
            Assert.IsFalse(new Regex("svc/[a-zA-z0-9]*[.]svc(/help)?$").IsMatch("svc/PatientSearch.svc/help/foo"));
            Assert.IsFalse(new Regex("svc/[a-zA-z0-9]*[.]svc(/help)?$").IsMatch("svc/PatientSearch.svc/101/patients/M"));
            Assert.IsTrue(new Regex("svc/[a-zA-z0-9]*[.]svc(/help)?$").IsMatch("svc/PatientSearch.svc"));
            Assert.IsTrue(new Regex("svc/[a-zA-z0-9]*[.]svc(/help)?$").IsMatch("svc/Scheduling.svc/help"));
            Assert.IsTrue(new Regex("svc/[a-zA-z0-9]*[.]svc(/help)?$").IsMatch("svc/PatientSearch.svc/help"));
            Assert.IsTrue(new Regex("svc/[a-zA-z0-9]*[.]svc(/help)?$").IsMatch("svc/Scheduling.svc"));
        }

        [Test]
        public void testCount()
        {
            Assert.AreEqual(0, StringUtils.count("foo", 'Z'));
            Assert.AreEqual(2, StringUtils.count("foo", 'o'));
            Assert.AreEqual(4, StringUtils.count("foobarooh", 'o'));
        }

        [Test]
        public void testExtractNonNumeric()
        {
            Assert.AreEqual("foo", StringUtils.extractNonNumeric("foo123bar"));
            Assert.AreEqual("bar", StringUtils.extractNonNumeric("123.05bar"));
            Assert.AreEqual("foob.", StringUtils.extractNonNumeric("foob.45ar"));
            Assert.AreEqual("foo", StringUtils.extractNonNumeric("123foo456bar"));
            Assert.AreEqual(".", StringUtils.extractNonNumeric(".123bar"), "Dot should only be treated as part of a number if preceded by zero");
            Assert.AreEqual("foo", StringUtils.extractNonNumeric("foo0.123bar"));
            Assert.AreEqual("bar", StringUtils.extractNonNumeric("0.123bar"));
            Assert.AreEqual("?+", StringUtils.extractNonNumeric("?+3151231.1030"));
        }

        [Test]
        public void testExtractNumeric()
        {
            Assert.AreEqual("123", StringUtils.extractNumeric("foo123bar"));
            Assert.AreEqual("123.05", StringUtils.extractNumeric("foo123.05bar"));
            Assert.AreEqual("123", StringUtils.extractNumeric("foo123b.45ar"));
            Assert.AreEqual("123", StringUtils.extractNumeric("123foo456bar"));
            Assert.AreEqual("123", StringUtils.extractNumeric(".123bar"), "Should enforce leading zero if decimal only");
            Assert.AreEqual("0.123", StringUtils.extractNumeric("foo0.123bar"));
            Assert.AreEqual("123", StringUtils.extractNumeric("foo123"));
        }

        [Test]
        public void testPiece()
        {
            String testString = "Red fish^Blue fish^Star fish^You fish";
            Assert.IsTrue("Red fish".Equals(StringUtils.piece(testString, "^", 0, true)));
            Assert.IsTrue("Red fish".Equals(StringUtils.piece(testString, "^", 1)));
            Assert.IsTrue("Star fish".Equals(StringUtils.piece(testString, "^", 2, true)));
            Assert.IsTrue("Star fish".Equals(StringUtils.piece(testString, "^", 3)));
            Assert.IsTrue(String.Empty.Equals(StringUtils.piece(testString, "^", 5)));
        }

        [Test]
        public void testGetIndex01()
        {
            String[] coll = new String[] { "Line 1", "Line 2", "Line 3" };
            Assert.AreEqual(0, StringUtils.firstIndexOf(coll, "Line 1"));
            Assert.AreEqual(1, StringUtils.firstIndexOf(coll, "Line 2"));
            Assert.AreEqual(2, StringUtils.firstIndexOf(coll, "Line 3"));
            Assert.AreEqual(-1, StringUtils.firstIndexOf(coll, "I like chocolate"));

            IList<String> listColl = new List<String>() { "Line 1", "Line 2", "Line 3" };
            Assert.AreEqual(0, StringUtils.firstIndexOf(listColl, "Line 1"));
            Assert.AreEqual(1, StringUtils.firstIndexOf(listColl, "Line 2"));
            Assert.AreEqual(2, StringUtils.firstIndexOf(listColl, "Line 3"));
            Assert.AreEqual(-1, StringUtils.firstIndexOf(listColl, "I like chocolate"));
        }

        [Test]
        public void testLastIndexOf()
        {
            Int32 iterations = 1000000;
            IList<String> trash = new List<String>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            String searchTarget = "10";
            Int32 indexTrash = 0;

            DateTime start = DateTime.Now;
            for (int i = 0; i < iterations; i++)
            {
                indexTrash = StringUtils.lastIndexOf(trash, searchTarget);
            }
            DateTime stop = DateTime.Now;

            System.Console.WriteLine(String.Format("Last index of took {0} for list", stop.Subtract(start).ToString()));

            start = DateTime.Now;
            for (int i = 0; i < iterations; i++)
            {
                indexTrash = StringUtils.firstIndexOf(trash, searchTarget);
            }
            stop = DateTime.Now;

            System.Console.WriteLine(String.Format("First index of took {0} for list", stop.Subtract(start).ToString()));

        }

        [Test]
        public void testStringSplit()
        {
            String myStr = "One fish, two fish, red fish, blue fish";

            String[] pieces = StringUtils.split(myStr, " ", StringSplitOptions.None);

            Assert.AreEqual(pieces[0], "One");
            Assert.AreEqual(pieces[1], "fish,");
            Assert.AreEqual(pieces[7], "fish");
        }

        [Test]
        public void testIsNumeric()
        {
            Assert.IsTrue(StringUtils.isInteger("0"));
            Assert.IsTrue(StringUtils.isInteger("1"));
            Assert.IsTrue(StringUtils.isInteger("9"));
            Assert.IsTrue(StringUtils.isNumeric("123.45"));
            Assert.IsTrue(StringUtils.isNumeric(".45"));
            Assert.IsTrue(StringUtils.isNumeric("123.0"));
            Assert.IsTrue(StringUtils.isNumeric("1234567890123456789012345678901234567890123456789012345678901234567890"));
            Assert.IsTrue(StringUtils.isNumeric("1234567890123456789012345678901234567890123456789012345678901234567890.1234567890"));
            Assert.IsTrue(StringUtils.isNumeric("123.45"));

            Assert.IsFalse(StringUtils.isNumeric(""));
            Assert.IsFalse(StringUtils.isNumeric("."));
            Assert.IsFalse(StringUtils.isNumeric("?"));
            Assert.IsFalse(StringUtils.isNumeric("!"));
            Assert.IsFalse(StringUtils.isNumeric("#$%"));
            Assert.IsFalse(StringUtils.isNumeric("String of characters"));
            Assert.IsFalse(StringUtils.isNumeric("FOO.123"));
            Assert.IsFalse(StringUtils.isNumeric("123."));
        }

        [Test]
        public void testIsInteger()
        {
            Assert.IsTrue(StringUtils.isInteger("0"));
            Assert.IsTrue(StringUtils.isInteger("1"));
            Assert.IsTrue(StringUtils.isInteger("9"));
            Assert.IsTrue(StringUtils.isInteger("1234567890123456789012345678901234567890123456789012345678901234567890"));

            Assert.IsFalse(StringUtils.isInteger("123.45"));
            Assert.IsFalse(StringUtils.isInteger(".45"));
            Assert.IsFalse(StringUtils.isInteger("123.0"));
            Assert.IsFalse(StringUtils.isInteger("1234567890123456789012345678901234567890123456789012345678901234567890.1234567890"));
            Assert.IsFalse(StringUtils.isInteger("123.45"));
            Assert.IsFalse(StringUtils.isNumeric(""));
            Assert.IsFalse(StringUtils.isNumeric("."));
            Assert.IsFalse(StringUtils.isInteger("?"));
            Assert.IsFalse(StringUtils.isInteger("!"));
            Assert.IsFalse(StringUtils.isInteger("#$%"));
            Assert.IsFalse(StringUtils.isInteger("String of characters"));
            Assert.IsFalse(StringUtils.isInteger("FOO.123"));
            Assert.IsFalse(StringUtils.isNumeric("123."));
        }

        [Test]
        public void testWrapText()
        {
            String text = "This is my first novel. It's not very good. I'm a terrible writer. I do however curse a lot and can pen a pretty saucy love scene so...";

            IList<String> wrapped = StringUtils.wrapText(text, 40);

            Assert.AreEqual(4, wrapped.Count);
            Assert.AreEqual(wrapped[0], "This is my first novel. It's not very");
            Assert.IsTrue(wrapped[0].Length <= 40);
            Assert.AreEqual(wrapped[1], "good. I'm a terrible writer. I do");
            Assert.IsTrue(wrapped[1].Length <= 40);
            Assert.AreEqual(wrapped[2], "however curse a lot and can pen a pretty");
            Assert.IsTrue(wrapped[2].Length <= 40);
            Assert.AreEqual(wrapped[3], "saucy love scene so...");
            Assert.IsTrue(wrapped[3].Length <= 40);
        }

        [Test]
        public void testWrapTextWithWordsGreaterThanMaxCharsPerLine()
        {
            String text = "AABBCCDDEEFFGGHHIIJJKK This is my first SMALLPEQUENOTINYITTYBITTY novel. TYPOSSSSSS!!!!";

            Int32 maxCharsPerLine = 10;
            IList<String> wrapped = StringUtils.wrapText(text, maxCharsPerLine);

            Assert.AreEqual(11, wrapped.Count);
            Assert.AreEqual(wrapped[0], "AABBCCDDE-");
            Assert.IsTrue(wrapped[0].Length <= maxCharsPerLine);
            Assert.AreEqual(wrapped[1], "EFFGGHHII-");
            Assert.IsTrue(wrapped[1].Length <= maxCharsPerLine);
            Assert.AreEqual(wrapped[2], "JJKK This");
            Assert.IsTrue(wrapped[2].Length <= maxCharsPerLine);

            Assert.AreEqual(wrapped[3], "is my");
            Assert.IsTrue(wrapped[3].Length <= maxCharsPerLine);
            Assert.AreEqual(wrapped[4], "first");
            Assert.IsTrue(wrapped[4].Length <= maxCharsPerLine);

            Assert.AreEqual(wrapped[5], "SMALLPEQU-");

            Assert.AreEqual(wrapped[6], "ENOTINYIT-");

            Assert.AreEqual(wrapped[7], "TYBITTY");

            Assert.AreEqual(wrapped[8], "novel.");

            Assert.AreEqual(wrapped[9], "TYPOSSSSS-");

            Assert.AreEqual(wrapped[10], "S!!!!");
        }

        [Test]
        public void testJoinWithWrap()
        {
            IList<String> listToJoin = new List<String>();

            Assert.IsTrue(String.IsNullOrEmpty(StringUtils.join(listToJoin, ",")));

            listToJoin.Add("640");
            Assert.AreEqual("640", StringUtils.join(listToJoin, ", "));

            Assert.AreEqual("'640'", StringUtils.join(listToJoin, ", ", 0, 0, "'"));

            listToJoin.Add("612");
            listToJoin.Add("691");

            Assert.AreEqual("'640', '612', '691'", StringUtils.join(listToJoin, ", ", 0, 0, "'"));
        }

        [Test]
        public void testRightPack()
        {
            Assert.AreEqual("0000001", StringUtils.rightPack("1", 7));
            Assert.AreEqual("0010000", StringUtils.rightPack("10000", 7));
            Assert.AreEqual("FOO", StringUtils.rightPack("FOO", 3));
            Assert.AreEqual("FOO", StringUtils.rightPack("FOO", 2));
            Assert.AreEqual("0000000001", StringUtils.rightPack("1"));
            Assert.AreEqual("0000012345", StringUtils.rightPack("12345"));
            Assert.AreEqual("-----12345", StringUtils.rightPack("12345", 10, '-'));
        }

        [Test]
        public void testExtractWrappedString()
        {
            Assert.AreEqual("please excuse my dear aunt sally", StringUtils.extractWrappedString("please excuse my dear aunt sally", '<', '>'));
            Assert.AreEqual("please <excuse my dear aunt sally", StringUtils.extractWrappedString("please <excuse my dear aunt sally", '<', '>'));
            Assert.AreEqual("excuse my dear aunt sally", StringUtils.extractWrappedString("please <excuse my dear aunt sally>", '<', '>'));
            Assert.AreEqual("please \"<excuse my dear aunt>\" sally", StringUtils.extractWrappedString("please \"<excuse my dear aunt>\" sally", '<', '>'));
            Assert.AreEqual("excuse my \"dear\" aunt", StringUtils.extractWrappedString("please <excuse my \"dear\" aunt> sally", '<', '>'));
            Assert.AreEqual("please <excuse my \"dear\" aunt sally", StringUtils.extractWrappedString("please <excuse my \"dear\" aunt sally", '<', '>'));
            Assert.AreEqual("ID=DB,Number=0,Type=Flag,Description=\"dbSNP membership, build 129\"", StringUtils.extractWrappedString("##INFO=<ID=DB,Number=0,Type=Flag,Description=\"dbSNP membership, build 129\">", '<', '>'));
        }

        [Test]
        public void testExtractKeyValuePairs()
        {
            Dictionary<String, String> parsed = StringUtils.toCaseInsensitiveDictFromDelimited("ID=DB,Number=0,Type=Flag,Source=\"\\\"Made up\\\" source\",Description=\"dbSNP membership, build 129\"", ',', '=');
            Assert.IsTrue(parsed.ContainsKey("id"));
            Assert.AreEqual(parsed["id"], "DB");
            Assert.IsTrue(parsed.ContainsKey("NUMBER"));
            Assert.AreEqual(parsed["NUMBER"], "0");
            Assert.IsTrue(parsed.ContainsKey("Type"));
            Assert.AreEqual(parsed["Type"], "Flag");
            Assert.IsTrue(parsed.ContainsKey("source"));
            Assert.AreEqual(parsed["source"], "\"Made up\" source");
            Assert.IsTrue(parsed.ContainsKey("description"));
            Assert.AreEqual(parsed["description"], "dbSNP membership, build 129");
        }
    }
}
