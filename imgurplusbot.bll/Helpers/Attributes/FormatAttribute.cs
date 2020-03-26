using System;
using System.Collections.Generic;
using System.Text;

namespace imgurplusbot.bll.Helpers.Attributes
{
    public class FormatAttribute : Attribute
    {
        public string Value { get; }
        public FormatAttribute(string value)
        {
            this.Value = value;
        }
    }
}
