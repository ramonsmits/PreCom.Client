using System;
using System.Collections.Generic;

namespace PreCom
{
    public class User
    {
        public int UserID { get; set; }
        public int HomeNodeID { get; set; }
        public string FullName { get; set; }
        public int LangID { get; set; }
        public bool NotAvailable { get; set; }
        public string NotAvailableTimestamp { get; set; }
        public bool NotAvailalbeScheduled { get; set; }
        public bool NoOccupancy { get; set; }
        public string Homescreen { get; set; }
        public long AppNotifications { get; set; }
        public string SoundAlarm { get; set; }
        public string SoundInfo { get; set; }
        public string SoundUnderstaffing { get; set; }
        public Dictionary<DateTime, Dictionary<string, bool?>> SchedulerDays { get; set; }
        public List<ConsignmentAppointment> ConsignmentAppointments { get; set; }
    }
}