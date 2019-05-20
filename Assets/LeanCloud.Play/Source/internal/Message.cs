using System;
using System.Collections.Generic;

namespace LeanCloud.Play {
    internal class Message {

        private Message() { }

        internal Dictionary<string, object> Data {
            get; private set;
        }

        internal static Message NewMessage(string cmd, string op = null) {
            var message = new Message {
                Data = new Dictionary<string, object> {
                    { "cmd", cmd },
                    { "op", op }
                }
            };
            return message;
        }

        internal static Message NewRequest(string cmd, string op = null) {
            var message = NewMessage(cmd, op);
            message.I = RequestI;
            return message;
        }

        internal static Message FromJson(string json) {
            var message = new Message();
            try {
                message.Data = Json.Parse(json) as Dictionary<string, object>;
            } catch (Exception e) {
                Logger.Error(e.Message);
            }
            return message;
        }

        internal string Cmd { 
            get {
                if (Data.TryGetValue("cmd", out object cmdObj)) {
                    return cmdObj as string;
                }
                Logger.Error("No cmd in message");
                return string.Empty;
            } set {
                Data["cmd"] = value;
            }
        }

        internal string Op {
            get {
                if (Data.TryGetValue("op", out object opObj)) {
                    return opObj as string;
                }
                Logger.Error("No op in message");
                return string.Empty;
            }
            set {
                Data["op"] = value;
            }
        }

        internal int ReasonCode { 
            get {
                if (Data.TryGetValue("reasonCode", out object reasonCodeObj) &&
                    int.TryParse(reasonCodeObj.ToString(), out int reasonCode)) {
                    return reasonCode;
                }
                Logger.Error("No reason code in message");
                return 0;
            }
        }

        internal string Detail { 
            get { 
                if (Data.TryGetValue("detail", out object detailObj)) {
                    return detailObj as string;
                }
                Logger.Error("No detail in message");
                return string.Empty;
            }
        }

        internal int I {
            get {
                if (Data.TryGetValue("i", out object iObj) &&
                    int.TryParse(iObj.ToString(), out int i)) {
                    return i;
                }
                Logger.Error("No i in message");
                return -1;
            } set {
                Data["i"] = value;
            }
        }

        internal bool HasI { 
            get {
                return Data.ContainsKey("i");
            }
        }

        internal bool IsError { 
            get {
                return Data.ContainsKey("reasonCode");
            }
        }

        internal object this[string key] { 
            get {
                return Data[key];
            } set {
                Data[key] = value;
            }
        }

        internal bool TryGetValue(string key, out object value) {
            value = null;
            return Data.TryGetValue(key, out value);
        }

        internal string ToJson() {
            return Json.Encode(Data);
        }

        public override string ToString() {
            return ToJson();
        }

        static volatile int requestI = 0;
        static readonly object requestILock = new object();

        static int RequestI { 
            get { 
                lock (requestILock) {
                    return requestI++;
                }
            }
        }
    }
}
