using System;

namespace PreCom
{
    /*
"MsgOutID": 1,
    "ControlID": "A",
    "MsgInID": 1,
    "Timestamp": "2019-04-22T20:41:44.3496587+02:00",
    "ValidTo": "2019-04-22T20:41:44.3496587+02:00",
    "Text": "sample string 5",
    "MsgIn": {
      "$id": "2",
      "MsgInID": 1,
      "Text": "sample string 2"
    }     */
    public class MsgOut
    {
        public int MsgOutID { get; set; }
        public string ControlID { get; set; }
        public int MsgInID { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime ValidTo { get; set; }
        public string Text { get; set; }
        public MsgIn MsgIn { get; set; }
    }
}
