using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.dao.vista;
using com.bitscopic.hilleman.core.domain;
using System.Linq;
using System.Collections.Concurrent;

namespace com.bitscopic.hilleman.core.utils
{
    public static class DateUtils
    {
        public static Int32 getETLBatchIDForLabShippingManifestExtractor()
        {
            return Convert.ToInt32(DateTime.Now.ToString("yyMMddHHmm"));
        }

        public static double calculateTotalDaysDifferenceWithWeekendsAndHolidays(DateTime from, DateTime to, bool useAbsoluteValue = false)
        {
            if (to < from && !useAbsoluteValue)
            {
                throw new ArgumentException("Invalid date range - from should be before to");
            }

            if (to < from && useAbsoluteValue) // don't *really* need to check if useAbsoluteValue is true here but doing so for clarity
            {
                DateTime temp = from;
                from = to;
                to = temp;
            }

            Int32 daysToSubtract = 0;
            DateTime adjustedStartDate = new DateTime(from.Year, from.Month, from.Day, from.Hour, from.Minute, from.Second);
            DateTime adjustedEndDate = new DateTime(to.Year, to.Month, to.Day, to.Hour, to.Minute, to.Second);

            // first adjust to/from in case they were holidays or weekends - *shouldn't* be but the day shouldn't count if it was
            if (DateUtils.isWeekendOrHoliday(from))
            {
                DateTime newStart = new DateTime(from.Year, from.Month, from.Day);
                while (DateUtils.isWeekendOrHoliday(newStart))
                {
                    newStart = newStart.AddDays(1);
                }

                // if start date was on weekend or holiday then we won't count that first partial day of work (for example, if start date was sun @ 11:00 AM
                // and end date was tues @ 10:00 AM then adjusted start date would be mon giving date diff of < 1 days) - i think we should count the first
                // day from the beginning of the day - so... by setting start to 1st second of the day, we'll include the day in the calc
                adjustedStartDate = new DateTime(newStart.Year, newStart.Month, newStart.Day, 0, 0, 1);
            }

            if (DateUtils.isWeekendOrHoliday(to))
            {
                DateTime newTo = new DateTime(to.Year, to.Month, to.Day);
                while (DateUtils.isWeekendOrHoliday(newTo))
                {
                    newTo = newTo.AddDays(-1);
                }

                // if end date was on weekend or holiday then we won't count that last partial day of work (for example, if start date was thurs @ 8:00 AM
                // and end date was satur @ 10:00 AM then adjusted end date would be friday giving date diff of ~ 1.1 days) - i think we should count the last
                // day but not the part of the off day - so... by setting end to last second of the day, we'll include the last working day in the calc
                adjustedEndDate = new DateTime(newTo.Year, newTo.Month, newTo.Day, 23, 59, 59);
            }

            if (adjustedEndDate <= adjustedStartDate) // guard against case where both start and end were on consecutive weekend/holidays and adjustment moves to < from
            {
                return 0.0;
            }

            //if (to.Subtract(from).TotalDays <= 1)
            //{
            //    return to.Subtract(from).TotalDays;
            //}


            DateTime currentDate = new DateTime(adjustedStartDate.Year, adjustedStartDate.Month, adjustedStartDate.Day);
            while (currentDate <= adjustedEndDate)
            {
                if (DateUtils.isWeekendOrHoliday(currentDate))
                {
                    daysToSubtract++;
                }
                currentDate = currentDate.AddDays(1);
            }

            return (adjustedEndDate.Subtract(adjustedStartDate).TotalDays) - daysToSubtract;
        }

        internal static DateTime safeParse(string fileDate)
        {
            try
            {
                return DateUtils.parseDateTime(fileDate, null);
            }
            catch (Exception) { }

            return default(DateTime);
        }

        /// <summary>
        /// Returns #/#/## date format
        /// Or #/#/## h:mm format if including time
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static String toCommonShortDate(DateTime dt, bool includeTime = false)
        {
            if (includeTime)
            {
                return dt.ToString("M/d/yy h:mm tt");
            }
            else
            {
                return dt.ToString("M/d/yy");
            }
        }

        /// <summary>
        /// Convert the current system time to UTC
        /// </summary>
        /// <returns></returns>
        public static DateTime convertSystemTimeToUTC()
        {
            return DateTime.Now.ToUniversalTime();
        }

        /// <summary>
        /// Convert a UTC time to the specified destination time zone
        /// </summary>
        /// <param name="utcTime"></param>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        public static DateTime convertUTCToTimeZone(DateTime utcTime, TimeZoneInfo timeZone)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZone);
        }

        public static String toExcelFriendlyDate(DateTime dt, bool includeTime = true)
        {
            if (includeTime)
            {
                return dt.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else if (!includeTime)
            {
                return dt.ToString("yyyy-MM-dd");
            }
            else
            {
                return dt.ToString();
            }
        }

        public static String toPdfDate(DateTime dt, bool includeTime = true)
        {
            return includeTime ? dt.ToString("MMM dd, yyyy@HH:mm") : dt.ToString("MMM dd, yyyy");
        }

        /// <summary>
        /// Convert DateTime object to fomat: MM/dd/yy@HHmmss
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static String toVistaExternalDateTime(DateTime dt, TimeZoneInfo vistaTimeZone, bool includeTime = true, bool includeSeconds = false)
        {
            // if disabled, don't assume dt argument is UTC
            if (StringUtils.parseBool(MyConfigurationManager.getValue("DisableTimeZoneHandling")))
            {
                if (!includeTime)
                {
                    return dt.ToString("MMM d,yyyy");
                }
                if (includeSeconds)
                {
                    return dt.ToString("MMM d,yyyy@HHmmss");
                }
                return dt.ToString("MMM d,yyyy@HHmm");
            }
            else
            {
                dt = TimeZoneInfo.ConvertTimeFromUtc(dt, vistaTimeZone);
                if (!includeTime)
                {
                    return dt.ToString("MMM d,yyyy");
                }
                if (includeSeconds)
                {
                    return dt.ToString("MMM d,yyyy@HHmmss");
                }
                return dt.ToString("MMM d,yyyy@HHmm");
            }
        }

        public static String toVistaExternalDateTimeNoTimeZoneConversion(DateTime dt, bool includeTime = true, bool includeSeconds = false)
        {
            if (!includeTime)
            {
                return dt.ToString("MMM d,yyyy");
            }
            if (includeSeconds)
            {
                return dt.ToString("MMM d,yyyy@HH:mm:ss");
            }
            return dt.ToString("MMM d,yyyy@HH:mm");
        }

        /// <summary>
        /// 1) Get UTC for local DateTime passed in and 2) convert to ISO 8601 string
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static String toIsoString(DateTime dt, bool convertToUTC = true, bool includeTime = true)
        {
            if (!includeTime)
            {
                return new DateTime(dt.Year, dt.Month, dt.Day).ToString("o").Split(new char[] { 'T' })[0];
            }
            if (!convertToUTC || String.Equals("true", MyConfigurationManager.getValue("DisableTimeZoneHandling"), StringComparison.CurrentCultureIgnoreCase))
            {
                return DateUtils.toIsoStringNoTimeZoneConversion(dt);
            }
            else
            {
                return dt.ToUniversalTime().ToString("o");
            }
        }

        public static String toIsoStringNoTimeZoneConversion(DateTime dt)
        {
           return dt.ToString("o");
        }

        public static String toVistaDate(DateTime dt)
        {
            // e.g. 12/25/2014 ==> 2014-1700=314 ==> 314/100=3 
            return String.Concat(((dt.Year - 1700) / 100).ToString(), 
                dt.ToString("yyMMdd"));
        }

        /// <summary>
        /// DateTime object should be UTC!! Convert DateTime to Vista internal FileMan format (e.g. 3151231.115959)
        /// and remove all the trailing zeroes from the time piece (e.g. 3150101.1200 -> 3150101.12)
        /// </summary>
        /// <param name="dt">UTC DateTime</param>
        /// <returns></returns>
        public static String toVistaDateShortTime(DateTime dt, TimeZoneInfo vistaTimeZone)
        {
            String dateStr = DateUtils.toVistaDateTime(dt, vistaTimeZone);
            // remove trailing zeroes if time component is present
            if (dateStr.Contains(".") && dateStr.EndsWith("0"))
            {
                while (dateStr.EndsWith("0"))
                {
                    dateStr = dateStr.Substring(0, dateStr.Length - 1);
                }
            }
            return dateStr;
        }

        /// <summary>
        /// Convert a UTC DateTime to VistA format after adjusting for VistA time zone. Time zone
        /// conversions can be disabled via configuration
        /// </summary>
        /// <param name="dt">UTC DateTime</param>
        /// <param name="vistaTimeZone"></param>
        /// <returns></returns>
        public static String toVistaDateTime(DateTime dt, TimeZoneInfo vistaTimeZone)
        {
            // don't convert unless this is disabled or not configured
            if (false == StringUtils.parseBool(MyConfigurationManager.getValue("DisableTimeZoneHandling")))
            {
                dt = TimeZoneInfo.ConvertTimeFromUtc(dt, vistaTimeZone);
            }

            String datePart = toVistaDate(dt);
            String timePart = dt.ToString("HHmmss");
            if (!String.Equals(timePart, "000000"))
            {
                return String.Concat(datePart, ".", timePart);
            }
            else
            {
                return datePart;
            }
        }

        /// <summary>
        /// Convert a VistA date or date/time string to a DateTime object
        /// </summary>
        /// <param name="vistaDateTime">yyyMMdd, yyyMMdd.HH, yyyMMdd.HHmm or yyyMMdd.HHmmss</param>
        /// <returns></returns>
        public static DateTime toDateTime(String vistaDateTime, TimeZoneInfo vistaTimeZone)
        {
            if (String.IsNullOrEmpty(vistaDateTime))
            {
                return TimeZoneInfo.ConvertTimeToUtc(new DateTime(), vistaTimeZone);
            }

            String[] pieces = vistaDateTime.Split(new char[] { '.' });

            // 3141231.235959
            Int32 year = Convert.ToInt32(pieces[0].Substring(0, 3)) + 1700;
            Int32 month = Convert.ToInt32(pieces[0].Substring(3, 2));
            Int32 day = Convert.ToInt32(pieces[0].Substring(5, 2));

            Int32 hour = 0;
            Int32 minute = 0;
            Int32 second = 0;
            if (pieces.Length > 1)
            {
                if (pieces[1].Length > 0)
                {
                    if (pieces[1].Length > 1)
                    {
                        hour = Convert.ToInt32(pieces[1].Substring(0, 2));
                    }
                    else
                    {
                        hour = Convert.ToInt32(pieces[1].Substring(0, 1)) * 10;
                    }
                }
                if (pieces[1].Length > 2)
                {
                    if (pieces[1].Length > 3)
                    {
                        minute = Convert.ToInt32(pieces[1].Substring(2, 2));
                    }
                    else
                    {
                        minute = Convert.ToInt32(pieces[1].Substring(2, 1)) * 10;
                    }
                }
                if (pieces[1].Length > 4)
                {
                    if (pieces[1].Length > 5)
                    {
                        second = Convert.ToInt32(pieces[1].Substring(4, 2));
                    }
                    else
                    {
                        second = Convert.ToInt32(pieces[1].Substring(4, 1)) * 10;
                    }
                }
            }

            if (StringUtils.parseBool(MyConfigurationManager.getValue("DisableTimeZoneHandling")))
            {
                return new DateTime(year, month, day, hour, minute, second);
            }
            else
            {
                return TimeZoneInfo.ConvertTimeToUtc(
                    new DateTime(year, month, day, hour, minute, second),
                    vistaTimeZone);
            }
           // return new DateTime(year, month, day, hour, minute, second);
        }

        public static DateTime parseDateTime(String dt, TimeZoneInfo timeZone)
        {
            DateTime parsed = new DateTime();
            if (String.IsNullOrEmpty(dt))
            {
                return default(DateTime);
            }

            if (DateTime.TryParse(dt, out parsed))
            {
                if (timeZone != null && false == StringUtils.parseBool(MyConfigurationManager.getValue("DisableTimeZoneHandling")))
                {
                    return TimeZoneInfo.ConvertTimeToUtc(parsed, timeZone);
                }
                else
                {
                    return parsed;
                }
            }
            else
            {
                return DateUtils.toDateTime(dt, timeZone);
            }
        }

        public static DateTime getVistaSystemTime(IVistaConnection cxn)
        {
            String vistaDt = new com.bitscopic.hilleman.core.dao.vista.ToolsDaoFactory().getToolsDao(cxn).getVistaSystemTime();
            return DateUtils.toDateTime(vistaDt, cxn.getSource().timeZoneParsed);
        }

        public static DateTime getVistaSystemTimeWithTimeZoneConversion(TimeZoneInfo vistaTimeZone)
        {
            return TimeZoneInfo.ConvertTime(DateTime.Now, vistaTimeZone);
        }

        public static DateTime parseDateTimeToUTC(String dt, TimeZoneInfo zone)
        {
            DateTime parsedWithoutOffset = DateUtils.parseDateTime(dt, zone);
            return TimeZoneInfo.ConvertTime(parsedWithoutOffset, zone).ToUniversalTime();
        }

        public static TimeZoneInfo getTimeZoneFromSourceSystem(SourceSystem ss)
        {
            return ss.timeZoneParsed; // TimeZoneInfo.FindSystemTimeZoneById(ss.timeZone);
        }

        public static List<DateTime> getAdjustedFederalHolidaysForYear(int year)
        {
            List<DateTime> holidays = new List<DateTime>();

            // New Years
            DateTime newYearsDate = adjustForWeekendHoliday(new DateTime(year, 1, 1));
            holidays.Add(newYearsDate);

            // MLK Day - 3rd Monday in Jan
            DateTime mlkDay = new DateTime(year, 1, 15); // *earliest* it can be is 15th - if not 15th, add days until day is monday
            while (mlkDay.DayOfWeek != DayOfWeek.Monday)
            {
                mlkDay = mlkDay.AddDays(1);
            }

            // Washington's Birthday - 3rd Mon in Feb
            DateTime washingtonsBday = new DateTime(year, 2, 15); // *earliest* it can be is 15th - if not 15th, add days until day is monday
            while (washingtonsBday.DayOfWeek != DayOfWeek.Monday)
            {
                washingtonsBday = washingtonsBday.AddDays(1);
            }

            // Memorial Day -- last monday in May 
            DateTime memorialDay = new DateTime(year, 5, 31);
            while (memorialDay.DayOfWeek != DayOfWeek.Monday)
            {
                memorialDay = memorialDay.AddDays(-1);
            }
            holidays.Add(memorialDay);

            // Independence Day
            DateTime independenceDay = adjustForWeekendHoliday(new DateTime(year, 7, 4));
            holidays.Add(independenceDay);

            // Labor Day -- 1st Monday in September 
            DateTime laborDay = new DateTime(year, 9, 1);
            while (laborDay.DayOfWeek != DayOfWeek.Monday)
            {
                laborDay = laborDay.AddDays(1);
            }
            holidays.Add(laborDay);

            // Columbus Day - 2nd Monday in Oct
            DateTime columbusDay = new DateTime(year, 10, 8); // *earliest* it can be is 8th - if not 8th, add days until day is monday
            while (columbusDay.DayOfWeek != DayOfWeek.Monday)
            {
                columbusDay = columbusDay.AddDays(1);
            }
            holidays.Add(columbusDay);

            // Veterans Day - Nov 11
            DateTime veteransDay = adjustForWeekendHoliday(new DateTime(year, 11, 11));
            holidays.Add(veteransDay);

            // Thanksgiving Day -- 4th Thursday in November 
            var thanksgiving = (from day in Enumerable.Range(1, 30)
                                where new DateTime(year, 11, day).DayOfWeek == DayOfWeek.Thursday
                                select day).ElementAt(3);
            DateTime thanksgivingDay = new DateTime(year, 11, thanksgiving);
            holidays.Add(thanksgivingDay);

            // Christmas Day 
            DateTime christmasDay = adjustForWeekendHoliday(new DateTime(year, 12, 25));
            holidays.Add(christmasDay);

            // Next year's new years check
            DateTime nextYearNewYearsDate = adjustForWeekendHoliday(new DateTime(year + 1, 1, 1));
            if (nextYearNewYearsDate.Year == year)
                holidays.Add(nextYearNewYearsDate);

            return holidays;
        }

        public static DateTime adjustForWeekendHoliday(DateTime holiday)
        {
            if (holiday.DayOfWeek == DayOfWeek.Saturday)
            {
                return holiday.AddDays(-1);
            }
            else if (holiday.DayOfWeek == DayOfWeek.Sunday)
            {
                return holiday.AddDays(1);
            }
            else
            {
                return holiday;
            }
        }

        static ConcurrentDictionary<Int32, List<DateTime>> _holidaysByYear = new ConcurrentDictionary<int, List<DateTime>>();
        static readonly object _locker = new object();

        public static bool isWeekendOrHoliday(DateTime dt)
        {
            if (!_holidaysByYear.ContainsKey(dt.Year)) // lazy load holidays into shared dict
            {
                lock (_locker)
                {
                    if (!_holidaysByYear.ContainsKey(dt.Year))
                    {
                        _holidaysByYear.TryAdd(dt.Year, new List<DateTime>());
                        _holidaysByYear[dt.Year] = getAdjustedFederalHolidaysForYear(dt.Year);
                    }
                }
            }

            DateTime dtDateOnly = new DateTime(dt.Year, dt.Month, dt.Day);
            if (dtDateOnly.DayOfWeek == DayOfWeek.Saturday || dtDateOnly.DayOfWeek == DayOfWeek.Sunday
                || _holidaysByYear[dt.Year].Contains(dtDateOnly))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static DateTime getMax(IEnumerable<DateTime> dates)
        {
            DateTime max = new DateTime(); // year 1

            foreach (DateTime dt in dates)
            {
                if (dt > max)
                {
                    max = dt;
                }
            }

            return max;
        }

        public static DateTime getMin(IEnumerable<DateTime> dates)
        {
            DateTime min = DateTime.MaxValue;

            foreach (DateTime dt in dates)
            {
                if (dt < min)
                {
                    min = dt;
                }
            }

            return min;
        }

        public static DateTime getBeginningOfMonth()
        {
            DateTime now = DateTime.Now;
            return new DateTime(now.Year, now.Month, 1);
        }

        public static DateTime getBeginningOfLastMonth()
        {
            DateTime now = DateTime.Now;
            if (now.Month == 1)
            {
                return new DateTime(now.Year - 1, 12, 1);
            }
            else
            {
                return new DateTime(now.Year, now.Month - 1, 1);
            }
        }

        public static DateTime getLastDayOfMonth(Int32 year, Int32 month)
        {
            return new DateTime(year, month, DateTime.DaysInMonth(year, month));
        }

    }
}