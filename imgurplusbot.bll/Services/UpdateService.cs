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

namespace imgurplusbot.bll.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly IBotService _botService;
        private readonly IImgurService _imgurService;
        private readonly ILogger<UpdateService> _logger;

        public UpdateService(IBotService botService, IImgurService imgurService, ILogger<UpdateService> logger)
        {
            _botService = botService;
            _imgurService = imgurService;
            _logger = logger;
        }

        public async Task ProcessRequest(Update update)
        {
            _logger.LogDebug("Received Updated with id: {0}", update.Id);

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
        #region Internal Methods
        internal async Task ProcessCallback(CallbackQuery callback)
        {
            ICallbackData callbackData = JsonConvert.DeserializeObject<CallbackData>(callback.Data);
            Message message = callback.Message;
            switch (callbackData.Action)
            {
                case CallbackAction.ChangeUrl:
                    await GenerateRotateLinks(callbackData, message);
                    break;
                case CallbackAction.DeleteImage:
                    await _imgurService.ImageEndpoint.DeleteImageAsync(callbackData.Data["deleteHash"]);
                    await _botService.Client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                    break;
                default:
                    break;
            }
        }
        internal async Task ProcessMessage(Message message)
        {
            switch (message.Type)
            {
                case MessageType.Photo:
                case MessageType.Document: /* GIFs and others */
                    await ProcessUpload(message);
                    break;
                case MessageType.Sticker: /* Static and Animated stickesr */
                case MessageType.Audio:
                case MessageType.Video:
                case MessageType.Voice:
                case MessageType.Location:
                case MessageType.Contact:
                case MessageType.Venue:
                case MessageType.Game:
                case MessageType.VideoNote:
                case MessageType.Invoice:
                case MessageType.SuccessfulPayment:
                case MessageType.WebsiteConnected:
                case MessageType.ChatMembersAdded:
                case MessageType.ChatMemberLeft:
                case MessageType.ChatTitleChanged:
                case MessageType.ChatPhotoChanged:
                case MessageType.MessagePinned:
                case MessageType.ChatPhotoDeleted:
                case MessageType.GroupCreated:
                case MessageType.SupergroupCreated:
                case MessageType.ChannelCreated:
                case MessageType.MigratedToSupergroup:
                case MessageType.MigratedFromGroup:
                case MessageType.Poll:
                case MessageType.Unknown:
                case MessageType.Text:
                //case MessageType.Animation:
                default:
                    await SendMessage(message.Chat.Id, "Can you send an Image?");
                    break;
            }
        }
        #endregion

        #region Private Methods
        private async Task ProcessUpload(Message message)
        {
            long chatId = message.Chat.Id;
            int messageId = message.MessageId;
            string fileId = null;
            IMG.FileType? fileType = null;

            if (message.Type == MessageType.Photo)
            {
                fileId = message.Photo.Last().FileId;
                fileType = IMG.FileType.Image;
            }
            if (message.Type == MessageType.Document && message.Document.MimeType == "video/mp4") /* GIF */
            {
                fileId = message.Document.FileId;
                fileType = IMG.FileType.Video;
            }

            if (string.IsNullOrEmpty(fileId) || !fileType.HasValue)
            {
                await SendMessage(message.Chat.Id, "Can you send an Image?");
                return;
            }
            /* Handle File size */
            await UploadImage(chatId, messageId, fileId, fileType.Value);
        }
        private async Task SendMessage(long chatId, string messageText)
        {
            await _botService.Client.SendTextMessageAsync(chatId, messageText);
        }
        private async Task UploadImage(long chatId, int messageId, string fileId, IMG.FileType fileType)
        {
            Message reepplyMessage = await _botService.Client.SendTextMessageAsync(chatId, "We are uploading your photo!", ParseMode.Default, false, false, messageId);

            IImage imageUploaded = null;

            using (IO.MemoryStream fileStream = new IO.MemoryStream())
            {
                File imageFile = await _botService.Client.GetInfoAndDownloadFileAsync(fileId, fileStream);
                try
                {
                    fileStream.Position = 0;
                    imageUploaded = await _imgurService.ImageEndpoint.UploadFileAsync(fileStream, imageFile.FilePath.Split('/').Last(), fileType, null, $"{imageFile.FileId} Upload by @imgurplusbot");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception throwed in UploadImage: {0} \n", ex.Message, ex.StackTrace);
                }
            }
            
            InlineKeyboardMarkup inlineKeyboardMarkup = null;
            if (imageUploaded != null)
            {
                inlineKeyboardMarkup = new InlineKeyboardMarkup(
                    new InlineKeyboardButton[][]
                    {
                        new InlineKeyboardButton[] /* First Row */
                        {
                            InlineKeyboardButton.WithCallbackData("BB", new CallbackData(CallbackAction.ChangeUrl).AddData("urlType", "BB").ToString()),
                            InlineKeyboardButton.WithCallbackData("DELETE", new CallbackData(CallbackAction.DeleteImage).AddData("deleteHash", imageUploaded.DeleteHash).ToString())
                        },
                        new InlineKeyboardButton[] /* Second Row */
                        {
                            InlineKeyboardButton.WithUrl("Open Link", imageUploaded.Link)
                        }
                    }

                );
            }
            await _botService.Client.EditMessageTextAsync(chatId, reepplyMessage.MessageId, imageUploaded?.Link ?? "<pre>Something is wrong! Try again later</pre>", ParseMode.Html, true, inlineKeyboardMarkup);
        }
        private async Task GenerateRotateLinks(ICallbackData callbackData, Message message)
        {
            /* Create a copy of current keyboard */
            InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(message.ReplyMarkup.InlineKeyboard);
            /* Get from keyboard info imgur url */
            string imgUrl = inlineKeyboardMarkup.InlineKeyboard.SelectMany((ik) => ik).FirstOrDefault((key) => key.Url != null).Url;
            /* Parse as enum urlType from callbackData */
            LinkRotateType linkType = (LinkRotateType)Enum.Parse(typeof(LinkRotateType), callbackData.Data["urlType"]);
            /* Get from enum attribute link format */
            string urlFormat = linkType.GetFormat();
            /* Declare message format based on MARKDOWN */
            string msgFormat = (linkType == LinkRotateType.HTML) ? "`{0}`" : "<pre>{0}</pre>";
            /* Declare next Link type */
            LinkRotateType nextLinkType = linkType.Next();
            /* Asign Property to proerty of change link type button */
            inlineKeyboardMarkup.InlineKeyboard.First().First().CallbackData = new CallbackData(CallbackAction.ChangeUrl).AddData("urlType", nextLinkType.ToString()).ToString();
            inlineKeyboardMarkup.InlineKeyboard.First().First().Text = nextLinkType.ToString();
            /* Edit message */
            await _botService.Client.EditMessageTextAsync(message.Chat.Id, message.MessageId, string.Format(linkType == LinkRotateType.TEXT ? "{0}" : msgFormat, string.Format(urlFormat, imgUrl)), (linkType == LinkRotateType.HTML) ? ParseMode.MarkdownV2 : ParseMode.Html, true, inlineKeyboardMarkup);
        }
        #endregion

    }
}
