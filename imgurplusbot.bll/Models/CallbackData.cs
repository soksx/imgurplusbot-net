using System;
using System.Linq;
using System.Collections.Generic;
using imgurplusbot.bll.Interfaces;
using System.Text.RegularExpressions;
using static imgurplusbot.bll.Helpers.Extensions.StringExtensions;

namespace imgurplusbot.bll.Models
{
    public class CallbackData : ICallbackData
    {
        private int _enumValue;
        public string Handler { get; }
        public Dictionary<string, string> Data { get; }
        public CallbackData(string handler, Enum action)
        {
            if (string.IsNullOrWhiteSpace(handler) || string.IsNullOrEmpty(handler))
                throw new ArgumentNullException(nameof(handler));
            this.Handler = handler;
            this._enumValue = Convert.ToInt32(action);
            this.Data = new Dictionary<string, string>();
        }
        private CallbackData(string handler, int action)
        {
            if (string.IsNullOrWhiteSpace(handler) || string.IsNullOrEmpty(handler))
                throw new ArgumentNullException(nameof(handler));
            this.Handler = handler;
            this._enumValue = action;
            this.Data = new Dictionary<string, string>();
        }
        public CallbackData AddData(string key, string value)
        {
            this.Data.Add(key, value);
            return this;
        }
        public CallbackData AddDataRange(IEnumerable<ValueTuple<string, string>> tuples)
        {
            foreach (ValueTuple<string, string> tuple in tuples)
                this.Data.Add(tuple.Item1, tuple.Item2);
            return this;
        }
        public new string ToString()
        {
            string serializedText = $"({Handler}){{{_enumValue}}}[{string.Join(';', Data.Select((el) => $"'{el.Key}':'{el.Value}'").ToArray())}]"; //(Handler){1}['aa':'bb';'cc':'dd']
            if (serializedText.Length > 64)
                throw new Exception("Callback data cannot be bigger than 64 Bytes");
            return serializedText;
        }
        public static explicit operator CallbackData(string cast)
        {
            if (string.IsNullOrWhiteSpace(cast) || string.IsNullOrEmpty(cast))
                throw new ArgumentNullException(nameof(cast));
            if (!new Regex(@"\(\w+\)\{\d+\}\[(\'\w+\'\:\'\w+\'\;?)+\]").IsMatch(cast))
                throw new InvalidCastException("Cannot parse input string correct format: (Handler){1}['aa':'bb';'cc':'dd']");
            Regex handlerRegex = new Regex(@"\(\w+\)");
            Regex actionRegex = new Regex(@"\{\d+\}");
            Regex dataRegex = new Regex(@"\'\w+\'\:\'\w+\'");
            CallbackData returnData = new CallbackData(handlerRegex.Match(cast).Value.RemoveCharXCharStr("()"), int.Parse(actionRegex.Match(cast).Value.RemoveCharXCharStr("{}")));
            returnData.AddDataRange(dataRegex.Matches(cast).Select((el) => (el.Value.RemoveCharXCharStr("'").Split(":")[0], el.Value.RemoveCharXCharStr("'").Split(":")[1])));
            return returnData;
        }
        public T GetAction<T>() where T : Enum
        {
            return (T)Enum.ToObject(typeof(T), _enumValue);
        }
    }
}
