using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Logging;
using imgurplusbot.dal.EF;


namespace imgurplusbot.bll.Interfaces
{
    public interface IBaseHandler
    {
        TelegramBotClient Bot { get; }
        ImgurPlusContext DatabaseContext { get; }
        ILogger Log { get; }
        string HandlerName { get; }
        MessageType[] MessageType { get; }
    }
}
