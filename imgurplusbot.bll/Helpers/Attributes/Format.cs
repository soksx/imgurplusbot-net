using System;
using System.Collections.Generic;
using System.Text;

namespace imgurplusbot.bll.Helpers.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class Format : Attribute
    {
        public string Value { get; }
        public Format(string value)
        {
            this.Value = value;
        }
    }
}
