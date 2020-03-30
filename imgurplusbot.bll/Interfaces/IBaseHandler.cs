using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace imgurplusbot.bll.Interfaces
{
    public interface IBaseHandler
    {
        TelegramBotClient Bot { get; }
        ILogger Log { get; }
        string HandlerName { get; }
        MessageType[] MessageType { get; }
    }
}
