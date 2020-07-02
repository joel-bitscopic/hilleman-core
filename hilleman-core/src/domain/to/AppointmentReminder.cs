using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.utils;
using System.Configuration;

namespace com.bitscopic.hilleman.core.domain.to
{
    [Serializable]
    public class AppointmentReminder
    {
        public Patient patient;
        public Appointment appointment;

        public AppointmentReminder() { }

        public String send()
        {
            String message = "";
            if (this.appointment != null && this.appointment.location != null)
            {
                message = "This message is to confirm your VA appointment at " +
                        this.appointment.start.ToString("f") + " at the following clinic: " + this.appointment.location.name;
            }

            if (this.patient != null && this.patient.emailAddresses != null && this.patient.emailAddresses.ContainsKey("personal")
                && this.appointment != null && this.appointment.location != null && !String.IsNullOrEmpty(this.appointment.location.name))
            {
                
                CalendarInvite inviteAttachment = new CalendarInvite() { appointment = this.appointment, patient = this.patient };
                EmailUtils.sendEmail(MyConfigurationManager.getValue("RemindersEmail"), 
                    this.patient.emailAddresses["personal"], 
                    "VA Appointment Reminder", 
                    message, 
                    inviteAttachment.toIcs());
            }
            if (this.patient != null && this.patient.phones != null && this.patient.phones.ContainsKey("cell")
                && this.appointment != null && this.appointment.location != null && !String.IsNullOrEmpty(this.appointment.location.name))
            {
                //SmsUtils.sendSms(.["RemindersPhone"], this.patient.phones["cell"], message); 
            }

            return new Guid().ToString();
        }
    }
}