using System;
using Telegram.Bot;

namespace imgurplusbot.bll.Interfaces
{
    public interface IBotService
    {
        TelegramBotClient Client { get; }
    }
}
