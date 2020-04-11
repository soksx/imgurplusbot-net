using System;
using System.Collections.Generic;
using System.Text;

namespace imgurplusbot.bll.Helpers.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Command : Attribute
    {
        public string Name { get; }
        public string Format { get; }
        public bool AdminOnly { get; }
        public Command(string name, string format = null, bool adminOnly = false)
        {
            this.Name = name;
            this.Format = format;
            this.AdminOnly = adminOnly;
        }
    }
}
