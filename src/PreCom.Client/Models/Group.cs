using System;
using System.Collections.Generic;

namespace PreCom
{
    public class Group
    {
        public long GroupID { get; set; }
        public string Code { get; set; }
        public string Label { get; set; }
        public TimeSpan TimeCalculation { get; set; }
        public Dictionary<DateTime, Dictionary<string, bool?>> SchedulerDays { get; set; }
        public ServiceFuntion[] ServiceFuntions { get; set; }
    }
}