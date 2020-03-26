using imgurplusbot.bll.Enums;
using imgurplusbot.bll.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace imgurplusbot.bll.Models
{
    public class CallbackData : ICallbackData
    {
        public CallbackAction Action { get; }
        public Dictionary<string, string> Data { get; }
        public CallbackData(CallbackAction action)
        {
            this.Action = action;
            this.Data = new Dictionary<string, string>();
        }
        public CallbackData AddData(string key, string value)
        {
            this.Data.Add(key, value);
            return this;
        }

        public CallbackData AddDataRange(IEnumerable<ValueTuple<string, string>> tuples)
        {
            foreach (ValueTuple<string,string> tuple in tuples)
                this.Data.Add(tuple.Item1, tuple.Item2);
            return this;
        }
        public new string ToString()
        {
            string json = JsonConvert.SerializeObject(this);
            if (json.Length > 64)
                throw new Exception("Callback data cannot be more than 64 Bytes");
            return json;
        }
    }
}
