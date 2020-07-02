using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Configuration;
using com.bitscopic.hilleman.core.domain;

namespace com.bitscopic.hilleman.core.utils
{
    [TestFixture]
    public class DateUtilsTest
    {
        [Test]
        public void testGetLabShippingManifestExtractorETLBatchID()
        {
            System.Console.WriteLine(DateUtils.getETLBatchIDForLabShippingManifestExtractor().ToString());
        }

        [Test]
        public void testGetDateDiffWithWeekendsAndHolidays()
        {
            // using columbus day weekend of 2017 for edge case testing - monday oct 9, 2017 was columbus day

            // first some easy cases - days in same week
            DateTime fromDate = new DateTime(2017, 10, 10, 11, 30, 5, 0);
            DateTime toDate = new DateTime(2017, 10, 11, 9, 15, 0, 0);

            Double dateDiff = DateUtils.calculateTotalDaysDifferenceWithWeekendsAndHolidays(fromDate, toDate);
            dateDiff = Math.Floor(dateDiff * 10) / 10;
            Assert.AreEqual(0.9, dateDiff);

            fromDate = new DateTime(2017, 10, 10, 11, 30, 5, 0);
            toDate = new DateTime(2017, 10, 13, 9, 15, 0, 0);

            dateDiff = DateUtils.calculateTotalDaysDifferenceWithWeekendsAndHolidays(fromDate, toDate);
            dateDiff = Math.Floor(dateDiff * 10) / 10;
            Assert.AreEqual(2.9, dateDiff);

            // now with start date on weekend
            fromDate = new DateTime(2017, 10, 8, 11, 30, 5, 0); // sun
            toDate = new DateTime(2017, 10, 13, 9, 15, 0, 0);
            // calc should be from tues @ 12:00 midnight (1 second after)
            dateDiff = DateUtils.calculateTotalDaysDifferenceWithWeekendsAndHolidays(fromDate, toDate);
            dateDiff = Math.Floor(dateDiff * 10) / 10;
            Assert.AreEqual(3.3, dateDiff);

            // now with start date on columbus day
            fromDate = new DateTime(2017, 10, 9, 11, 30, 5, 0); // mon - columbus day
            toDate = new DateTime(2017, 10, 13, 9, 15, 0, 0);
            // calc should be from tues @ 12:00 midnight (1 second after)
            dateDiff = DateUtils.calculateTotalDaysDifferenceWithWeekendsAndHolidays(fromDate, toDate);
            dateDiff = Math.Floor(dateDiff * 10) / 10;
            Assert.AreEqual(3.3, dateDiff);

            // now with start date on columbus day but end date on the tues after
            fromDate = new DateTime(2017, 10, 9, 11, 30, 5, 0); // mon - columbus day
            toDate = new DateTime(2017, 10, 10, 9, 15, 0, 0);
            // calc should be from tues @ 12:00 midnight (1 second after)
            dateDiff = DateUtils.calculateTotalDaysDifferenceWithWeekendsAndHolidays(fromDate, toDate);
            dateDiff = Math.Floor(dateDiff * 10) / 10;
            Assert.AreEqual(0.3, dateDiff);

            // now with start date on columbus day and end date on the saturday after
            fromDate = new DateTime(2017, 10, 9, 11, 30, 5, 0); // mon - columbus day
            toDate = new DateTime(2017, 10, 14, 9, 15, 0, 0);
            // calc should be from tues @ 12:00 midnight (1 second after) to friday @ 11:59 (1 second before midnight)
            dateDiff = DateUtils.calculateTotalDaysDifferenceWithWeekendsAndHolidays(fromDate, toDate);
            dateDiff = Math.Floor(dateDiff * 10) / 10;
            Assert.AreEqual(3.9, dateDiff); // just slightly below 4 full days because of seconds - that's ok!

            // now with start date on saturday and end date on the columbus day
            fromDate = new DateTime(2017, 10, 7, 11, 30, 5, 0); 
            toDate = new DateTime(2017, 10, 9, 9, 15, 0, 0);

            dateDiff = DateUtils.calculateTotalDaysDifferenceWithWeekendsAndHolidays(fromDate, toDate);
            dateDiff = Math.Floor(dateDiff * 10) / 10;
            Assert.AreEqual(0.0, dateDiff); // no working days in range == 0 days

            // one partial working day between start and holiday
            fromDate = new DateTime(2017, 10, 6, 11, 30, 5, 0);
            toDate = new DateTime(2017, 10, 9, 9, 15, 0, 0);

            dateDiff = DateUtils.calculateTotalDaysDifferenceWithWeekendsAndHolidays(fromDate, toDate);
            dateDiff = Math.Floor(dateDiff * 10) / 10;
            Assert.AreEqual(0.5, dateDiff); // calc should be for 11:30 AM on Fri to 1 second before midnight 

            // normal weekend range
            fromDate = new DateTime(2017, 10, 12, 11, 30, 5, 0);
            toDate = new DateTime(2017, 10, 16, 9, 15, 0, 0);

            dateDiff = DateUtils.calculateTotalDaysDifferenceWithWeekendsAndHolidays(fromDate, toDate);
            dateDiff = Math.Floor(dateDiff * 10) / 10;
            Assert.AreEqual(1.9, dateDiff);

            // range of > one month with 2 holidays in range 
            fromDate = new DateTime(2017, 10, 5, 11, 30, 5, 0);
            toDate = new DateTime(2017, 11, 16, 9, 15, 0, 0);

            dateDiff = DateUtils.calculateTotalDaysDifferenceWithWeekendsAndHolidays(fromDate, toDate);
            dateDiff = Math.Floor(dateDiff * 10) / 10;
            Assert.AreEqual(27.9, dateDiff); // counted out days manually to verify!

            // range of one year (non-leap year) - should be 365 - 102 days of weekends - 10 holidays = 253
            fromDate = new DateTime(2017, 10, 5, 11, 30, 5, 0);
            toDate = new DateTime(2018, 10, 5, 11, 30, 5, 0);

            dateDiff = DateUtils.calculateTotalDaysDifferenceWithWeekendsAndHolidays(fromDate, toDate);
            dateDiff = Math.Floor(dateDiff * 10) / 10;
            Assert.AreEqual(253.0, dateDiff); // counted out days manually to verify! 
        }

        [Test]
        public void testGetWeekendsAndHolidaysTable()
        {
            List<DateTime> dates = DateUtils.getAdjustedFederalHolidaysForYear(2017);

            foreach (DateTime date in dates)
            {
                System.Console.WriteLine(DateUtils.toCommonShortDate(date, false));
            }
        }

        [Test]
        public void testCommonFormat()
        {
            Assert.AreEqual("4/1/15 4:20 PM", DateUtils.toCommonShortDate(new DateTime(2015, 4, 1, 16, 20, 1, 1), true));
            Assert.AreEqual("4/1/01 4:20 AM", DateUtils.toCommonShortDate(new DateTime(2001, 4, 1, 4, 20, 1, 1), true));
            Assert.AreEqual("12/31/15 12:20 AM", DateUtils.toCommonShortDate(new DateTime(2015, 12, 31, 0, 20, 1, 1), true));
            Assert.AreEqual("12/31/15 12:00 AM", DateUtils.toCommonShortDate(new DateTime(2015, 12, 31, 0, 0, 1, 1), true));
            Assert.AreEqual("12/31/15 12:00 PM", DateUtils.toCommonShortDate(new DateTime(2015, 12, 31, 12, 0, 1, 1), true));

            Assert.AreEqual("4/1/15", DateUtils.toCommonShortDate(new DateTime(2015, 4, 1, 16, 20, 1, 1), false));
            Assert.AreEqual("4/1/01", DateUtils.toCommonShortDate(new DateTime(2001, 4, 1, 4, 20, 1, 1), false));
            Assert.AreEqual("12/31/15", DateUtils.toCommonShortDate(new DateTime(2015, 12, 31, 0, 20, 1, 1), false));
        }

        [Test]
        public void testSpecialFormat()
        {
            DateTime aTime = new DateTime(2015, 4, 1, 16, 20, 1, 1);

            Assert.AreEqual("04/01/15 @ 16:20", aTime.ToString("MM/dd/yy @ HH:mm"));
        }

        [Test]
        public void testToIso()
        {
            DateTime argLocal = new DateTime(2015, 4, 1, 16, 20, 1, 1, DateTimeKind.Local);
            DateTime argUtc = new DateTime(2015, 4, 1, 16, 20, 1, 1, DateTimeKind.Utc);
            DateTime argOther = new DateTime(2015, 4, 1, 16, 20, 1, 1);

            Assert.AreEqual("2015-04-01T20:20:01.0010000Z", DateUtils.toIsoString(argLocal)); // hour corrected for UTC
            Assert.AreEqual("2015-04-01T16:20:01.0010000Z", DateUtils.toIsoString(argUtc));
            Assert.AreEqual("2015-04-01T20:20:01.0010000Z", DateUtils.toIsoString(argOther)); // hour corrected for UTC
        }

        [Test]
        public void testToIsoNoTimezoneConversion()
        {
            DateTime argLocal = new DateTime(2015, 4, 1, 16, 20, 1, 1, DateTimeKind.Local);
            DateTime argUtc = new DateTime(2015, 4, 1, 16, 20, 1, 1, DateTimeKind.Utc);
            DateTime argOther = new DateTime(2015, 4, 1, 16, 20, 1, 1);

            Assert.AreEqual("2015-04-01T16:20:01.0010000-04:00", DateUtils.toIsoStringNoTimeZoneConversion(argLocal)); // offset because kind was specified
            Assert.AreEqual("2015-04-01T16:20:01.0010000Z", DateUtils.toIsoStringNoTimeZoneConversion(argUtc)); // note 'Z' for UTC
            Assert.AreEqual("2015-04-01T16:20:01.0010000", DateUtils.toIsoStringNoTimeZoneConversion(argOther)); // note no timezone/offset info because timezone kind not specified
        }

        [Test]
        public void testParseDateISOFormatWithTimezone()
        {
            MyConfigurationManager.setValue("DisableTimeZoneHandling", "false");
            TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            String dt = "2015-04-01T20:20:01";
            Assert.AreEqual(DateUtils.parseDateTime(dt, zone).ToString(), new DateTime(2015, 4, 2, 0, 20, 1).ToString()); // note new day and time for adjustment to UTC!
            Assert.AreEqual(DateUtils.parseDateTime(dt, zone).Kind, DateTimeKind.Utc);
        }

        [Test]
        public void testParseDateISOFormatWithoutTimezone()
        {
            MyConfigurationManager.setValue("DisableTimeZoneHandling", "true");
            String dt = "2015-04-01T20:20:01";
            Assert.AreEqual(DateUtils.parseDateTime(dt, TimeZoneInfo.Local).ToString(), new DateTime(2015, 4, 1, 20, 20, 1).ToString()); // note SAME day and time
            Assert.AreEqual(DateUtils.parseDateTime(dt, TimeZoneInfo.Local).Kind, DateTimeKind.Unspecified);
        }

        [Test]
        public void testParseDateVistaFormat()
        {
            // DateUtils will not convert timestamp time/timezone if config setting is true
            MyConfigurationManager.setValue("DisableTimeZoneHandling", "false");
            TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            String dt = "3150304.1130";
            DateTime parsed = DateUtils.parseDateTime(dt, zone); // parsed are converted to UTC - EST is UTC-05:00
            Assert.AreEqual(parsed.Kind, DateTimeKind.Utc); 
            Assert.AreEqual(parsed.ToString(), new DateTime(2015, 3, 4, 16, 30, 0).ToString()); // 11:30 AM EST in UTC is 4:30 PM

            // now set to disabled (true) to illustrate non-change
            MyConfigurationManager.setValue("DisableTimeZoneHandling", "true");
            DateTime parsed2 = DateUtils.parseDateTime(dt, zone);

            Assert.AreEqual(parsed2.Kind, DateTimeKind.Unspecified);
            Assert.AreEqual(parsed2.ToString(), new DateTime(2015, 3, 4, 11, 30, 0).ToString()); // unchanged from original VistA timstamp string
        }

        [Test]
        public void testParseDateVariousVistaFormats()
        {
            TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById("UTC"); // use UTC for target time zone and don't need to change hours/minutes below!
            
            String dt = "3150304.153";
            Assert.AreEqual(DateUtils.parseDateTime(dt, zone).ToString(), new DateTime(2015, 3, 4, 15, 30, 0).ToString());

            dt = "3150304.1";
            Assert.AreEqual(DateUtils.parseDateTime(dt, zone).ToString(), new DateTime(2015, 3, 4, 10, 0, 0).ToString());

            dt = "3150304.10";
            Assert.AreEqual(DateUtils.parseDateTime(dt, zone).ToString(), new DateTime(2015, 3, 4, 10, 0, 0).ToString());

            dt = "3150304.10001";
            Assert.AreEqual(DateUtils.parseDateTime(dt, zone).ToString(), new DateTime(2015, 3, 4, 10, 0, 10).ToString());
        }

        [Test]
        public void testToVistaDate()
        {
            Assert.AreEqual("2900105", DateUtils.toVistaDate(new DateTime(1990, 1, 5)));
            Assert.AreEqual("3040215", DateUtils.toVistaDate(new DateTime(2004, 2, 15)));
            Assert.AreEqual("3140325", DateUtils.toVistaDate(new DateTime(2014, 3, 25)));
            Assert.AreEqual("3140405", DateUtils.toVistaDate(new DateTime(2014, 4, 5)));
            Assert.AreEqual("3140501", DateUtils.toVistaDate(new DateTime(2014, 5, 1)));
            Assert.AreEqual("3140630", DateUtils.toVistaDate(new DateTime(2014, 6, 30)));
            Assert.AreEqual("3140725", DateUtils.toVistaDate(new DateTime(2014, 7, 25)));
            Assert.AreEqual("3140812", DateUtils.toVistaDate(new DateTime(2014, 8, 12)));
            Assert.AreEqual("3140920", DateUtils.toVistaDate(new DateTime(2014, 9, 20)));
            Assert.AreEqual("3141001", DateUtils.toVistaDate(new DateTime(2014, 10, 1)));
            Assert.AreEqual("3141115", DateUtils.toVistaDate(new DateTime(2014, 11, 15)));
            Assert.AreEqual("3141225", DateUtils.toVistaDate(new DateTime(2014, 12, 25)));
        }

        [Test]
        public void testToVistaDateTime()
        {
            Assert.AreEqual("2900105", DateUtils.toVistaDateTime(new DateTime(1990, 1, 5), TimeZoneInfo.FindSystemTimeZoneById("UTC")));
            Assert.AreEqual("3040215.013000", DateUtils.toVistaDateTime(new DateTime(2004, 2, 15, 1, 30, 0), TimeZoneInfo.FindSystemTimeZoneById("UTC")));
            Assert.AreEqual("3140325.060115", DateUtils.toVistaDateTime(new DateTime(2014, 3, 25, 6, 1, 15), TimeZoneInfo.FindSystemTimeZoneById("UTC")));
            Assert.AreEqual("3140405.120000", DateUtils.toVistaDateTime(new DateTime(2014, 4, 5, 12, 0, 0), TimeZoneInfo.FindSystemTimeZoneById("UTC")));
            Assert.AreEqual("3140501.135959", DateUtils.toVistaDateTime(new DateTime(2014, 5, 1, 13, 59, 59), TimeZoneInfo.FindSystemTimeZoneById("UTC")));
            Assert.AreEqual("3140630.235959", DateUtils.toVistaDateTime(new DateTime(2014, 6, 30, 23, 59, 59), TimeZoneInfo.FindSystemTimeZoneById("UTC")));
        }

      //  [Test]
        public void testSerializationFormatting()
        {
            System.Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(new DateTime(2015, 12, 1, 10, 30, 15, DateTimeKind.Local)));
        }
    }
}
