using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using Imgur.API.Models;
using imgurplusbot.bll.Interfaces;
using Microsoft.Extensions.Logging;
using IO = System.IO;
using imgurplusbot.bll.Models;
using Newtonsoft.Json;
using static imgurplusbot.bll.Helpers.Extensions.EnumExtensions;
using imgurplusbot.bll.Enums;
using IMG = Imgur.API.Enums;
using imgurplusbot.bll.Helpers;
using System.Reflection;

namespace imgurplusbot.bll.BotServices
{
    public class UpdateService : BaseService, IUpdateService
    {
        private readonly IBotService _botService;
        private readonly IImgurService _imgurService;
        private readonly ILogger<UpdateService> _logger;

        public UpdateService(IBotService botService, IImgurService imgurService, ILogger<UpdateService> logger) : base(botService, imgurService, logger)
        {
            _botService = botService;
            _imgurService = imgurService;
            _logger = logger;
        }

        public async Task ProcessRequest(Update update)
        {
            _logger.LogDebug("Received Updated with id: {0}", update.Id);

            try
            {
                switch (update.Type)
                {
                    case UpdateType.CallbackQuery:
                        await ProcessCallback(update.CallbackQuery);
                        break;
                    case UpdateType.Message:
                        await ProcessMessage(update.Message);
                        break;
                    case UpdateType.Unknown:
                    case UpdateType.InlineQuery:
                    case UpdateType.ChosenInlineResult:
                    case UpdateType.EditedMessage:
                    case UpdateType.ChannelPost:
                    case UpdateType.EditedChannelPost:
                    case UpdateType.ShippingQuery:
                    case UpdateType.PreCheckoutQuery:
                    case UpdateType.Poll:
                    case UpdateType.PollAnswer:
                    default:
                        return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Main error handled");
            }
          
        }
        #region Private Methods
        private async Task ProcessCallback(CallbackQuery callback)
        {
            ICallbackData callbackData = (CallbackData)(callback.Data);
            await Task.WhenAll(ClassLoader<IBotHandler>.Instance[callbackData.Handler].Select((handler) => handler.ProcessCallback(callbackData, callback.Message, callback.Id)));
        }
        private async Task ProcessMessage(Message message)
        {
            await Task.WhenAll(ClassLoader<IBotHandler>.Instance[message.Type].Select((handler) => handler.ProcessMessage(message)));
        }
        #endregion

    }
}
