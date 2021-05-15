using System;

namespace PreCom
{
    public class MsgInLog
    {
        public int MsgInID { get; set; }
        public DateTime Timestamp { get; set; }
        public string Text { get; set; }
        public Group Group { get; set; }
    }
}
