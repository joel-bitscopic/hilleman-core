using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class CalendarInvite
    {
        public Patient patient;
        public Appointment appointment;

        public CalendarInvite() { }

        public byte[] toIcs()
        {
            String icsBody = "BEGIN:VCALENDAR" + Environment.NewLine +
                "PRODID:-//Google Inc//Google Calendar 70.9054//EN" + Environment.NewLine +
                "VERSION:2.0" + Environment.NewLine +
                "CALSCALE:GREGORIAN" + Environment.NewLine +
                "METHOD:REQUEST" + Environment.NewLine +
                "BEGIN:VEVENT" + Environment.NewLine +
                "DTSTART:" + appointment.start.ToUniversalTime().ToString("yyyyMMdd\\THHmmss\\Z") + Environment.NewLine +
                "DTEND:" + appointment.end.ToUniversalTime().ToString("yyyyMMdd\\THHmmss\\Z") + Environment.NewLine +
                "DTSTAMP:" + DateTime.Now.ToUniversalTime().ToString("yyyyMMdd\\THHmmss\\Z") + Environment.NewLine +
                "ORGANIZER;CN=Joel Mewton:mailto:joel@bitscopic.com" + Environment.NewLine +
                "UID:" + new Guid().ToString() + Environment.NewLine +
                "ATTENDEE;CUTYPE=INDIVIDUAL;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=" + Environment.NewLine +
                " TRUE;CN=" + patient.firstName + " " + patient.lastName + ";X-NUM-GUESTS=0:mailto:" + patient.emailAddresses["personal"] + Environment.NewLine +
                "CREATED:" + DateTime.Now.ToUniversalTime().ToString("yyyyMMdd\\THHmmss\\Z") + Environment.NewLine +
                "DESCRIPTION:Your appoinment reminder" + Environment.NewLine +
                "LAST-MODIFIED:" + DateTime.Now.ToUniversalTime().ToString("yyyyMMdd\\THHmmss\\Z") + Environment.NewLine +
                "LOCATION:" + this.appointment.location.name + Environment.NewLine +
                "SEQUENCE:0" + Environment.NewLine +
                "STATUS:CONFIRMED" + Environment.NewLine +
                "SUMMARY:Appointment Reminder" + Environment.NewLine +
                "TRANSP:OPAQUE" + Environment.NewLine +
                "END:VEVENT" + Environment.NewLine +
                "END:VCALENDAR" + Environment.NewLine;

            return System.Text.Encoding.UTF8.GetBytes(icsBody);
        }

    }
}