using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using imgurplusbot.bll.Interfaces;
using static imgurplusbot.bll.Helpers.Extensions.ClassExtensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using imgurplusbot.bll.Helpers.Attributes;
using Telegram.Bot.Types.Enums;
using imgurplusbot.dal.EF;

namespace imgurplusbot.bll.BotHandlers
{
    public abstract class BaseHandler : IBaseHandler, IBotHandler
    {
        private TelegramBotClient _tgBot { get; }
        private ILogger _logger { get; }
        public BaseHandler(IBotService botService)
        {
            this._tgBot = botService.Client;
            this._logger = LoggerFactory.Create((builder) => builder.SetMinimumLevel(LogLevel.Trace)).CreateLogger(this.GetType());
        }
        #region Abstract Methods
        public abstract Task ProcessMessage(Message message);
        public abstract Task ProcessCallback(ICallbackData callback, Message message);
        #endregion
        #region Shared properties
        public TelegramBotClient Bot => _tgBot;
        public ILogger Log => _logger;
        public ImgurPlusContext DatabaseContext => new ImgurPlusContext();
        public string HandlerName
        {
            get
            {
                return this.GetType().GetAttributeValue((Handler ha) => ha.Name);
            }
        }
        public MessageType[] MessageType
        {
            get
            {
                return this.GetType().GetAttributeValue((Handler ha) => ha.MessageTypes);
            }
        }
        #endregion
        #region Shared Methods
        protected Task SendMessage(long chatId, string messageText)
        {
            return Bot.SendTextMessageAsync(chatId, messageText);
        }
        #endregion
    }
}
