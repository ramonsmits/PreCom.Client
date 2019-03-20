using System;
using System.Collections.Generic;

namespace PreCom
{
    public class ServiceFuntion
    {
        public long ServiceFunctionID { get; set; }
        public string Label { get; set; }
        public int NumberNeeded { get; set; }
        public string NoOccupancy { get; set; }
        public Dictionary<DateTime, Dictionary<string, bool?>> OccupancyDays { get; set; }
        public Dictionary<DateTime, Dictionary<string, int>> DayTotals { get; set; }
        public List<User> Users { get; set; }
    }
}