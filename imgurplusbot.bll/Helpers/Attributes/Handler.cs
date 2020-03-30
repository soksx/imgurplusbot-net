using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.Enums;

namespace imgurplusbot.bll.Helpers.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Handler : Attribute
    {
        public string Name { get; set; }
        public MessageType[] MessageTypes { get; }
        public Handler(params MessageType[] messageTypes)
        {
            this.MessageTypes = messageTypes;
        }
        public Handler() { }
    }
}
