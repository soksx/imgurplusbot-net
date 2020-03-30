using System;
using System.Linq;
using IO = System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using imgurplusbot.bll.Interfaces;
using Telegram.Bot.Types;
using TGENUMS = Telegram.Bot.Types.Enums;
using imgurplusbot.bll.Helpers;
using Telegram.Bot.Types.ReplyMarkups;
using Imgur.API.Models;
using IMG = Imgur.API.Enums;
using imgurplusbot.bll.Models;
using imgurplusbot.bll.Enums;
using Microsoft.Extensions.Logging;
using imgurplusbot.bll.Helpers.Extensions;
using imgurplusbot.bll.Helpers.Attributes;

namespace imgurplusbot.bll.BotHandlers
{
    [Handler(TGENUMS.MessageType.Document, TGENUMS.MessageType.Photo, Name = "ImageHandler")]
    public class ImageHandler : BaseHandler, IBotHandler
    {
        private readonly IImgurService _imgurService;
        #region Constructors
        public ImageHandler(IBotService botService, IImgurService imgurService) : base(botService)
        {
            _imgurService = imgurService;
        }
        #endregion
        #region Overrides
        public override Task ProcessCallback(ICallbackData callback, Message message) => ProcessAction(callback, message);
        public override Task ProcessMessage(Message message) => ProcessUpload(message);
        #endregion
        #region Internal Methods
        internal async Task ProcessUpload(Message message)
        {
            long chatId = message.Chat.Id;
            int messageId = message.MessageId;
            string fileId = null;
            IMG.FileType fileType = IMG.FileType.Image;

            if (message.Type == TGENUMS.MessageType.Photo)
            {
                fileId = message.Photo.Last().FileId;
            }
            else if (message.Type == TGENUMS.MessageType.Document && Util.IsMimeTypeImage(message.Document.MimeType)) /* Image send as doc */
            {
                fileId = message.Document.FileId;
            }
            else if (message.Type == TGENUMS.MessageType.Document && Util.IsMimeTypeVideo(message.Document.MimeType)) /* GIF or Video */
            {
                fileId = message.Document.FileId;
                fileType = IMG.FileType.Video;
            }

            if (string.IsNullOrEmpty(fileId))
            {
                await SendMessage(message.Chat.Id, "Can you send an Image?");
                return;
            }

            File tgFile = await Bot.GetFileAsync(fileId);

            if (Util.FileSizeReached(tgFile.FileSize, fileType, out string maxFileSize))
            {
                await SendMessage(message.Chat.Id, $"Maximum file size reached! Max filesize for {fileType}: {maxFileSize}");
                return;
            }

            /* Handle File size */
            await UploadImage(chatId, messageId, tgFile, fileType);
        }
        internal async Task ProcessAction(ICallbackData callback, Message message)
        {
            switch (callback.GetAction<CallbackImageAction>())
            {
                case CallbackImageAction.ChangeUrl:
                    await GenerateRotateLinks(callback, message);
                    break;
                case CallbackImageAction.DeleteImage:
                    await _imgurService.ImageEndpoint.DeleteImageAsync(callback.Data["deleteHash"]);
                    await Bot.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                    break;
                default:
                    break;
            }
        }
        #endregion
        #region Private Methods
        private async Task UploadImage(long chatId, int messageId, File tgFile, IMG.FileType fileType)
        {
            Message reepplyMessage = await Bot.SendTextMessageAsync(chatId, "We are uploading your photo!", TGENUMS.ParseMode.Default, false, false, messageId);

            IImage imageUploaded = null;

            using (IO.MemoryStream fileStream = new IO.MemoryStream())
            {
                File imageFile = await Bot.GetInfoAndDownloadFileAsync(tgFile.FileId, fileStream);
                try
                {
                    fileStream.Position = 0;
                    imageUploaded = await _imgurService.ImageEndpoint.UploadFileAsync(fileStream, imageFile.FilePath.Split('/').Last(), fileType, null, $"{imageFile.FileId} Upload by @imgurplusbot");
                }
                catch (Exception ex)
                {
                    Log.LogError("Exception throwed in UploadImage: {0} \n", ex.Message, ex.StackTrace);
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
                            InlineKeyboardButton.WithCallbackData("BB", new CallbackData(HandlerName, CallbackImageAction.ChangeUrl).AddData("urlType", "BB").ToString()),
                            InlineKeyboardButton.WithCallbackData("DELETE", new CallbackData(HandlerName, CallbackImageAction.DeleteImage).AddData("deleteHash", imageUploaded.DeleteHash).ToString())
                        },
                        new InlineKeyboardButton[] /* Second Row */
                        {
                            InlineKeyboardButton.WithUrl("Open Link", imageUploaded.Link)
                        }
                    }
                );
            }
            await Bot.EditMessageTextAsync(chatId, reepplyMessage.MessageId, imageUploaded?.Link ?? "<pre>Something is wrong! Try again later</pre>", TGENUMS.ParseMode.Html, true, inlineKeyboardMarkup);
        }
        private async Task GenerateRotateLinks(ICallbackData callbackData, Message message)
        {
            /* Create a copy of current keyboard */
            InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(message.ReplyMarkup.InlineKeyboard);
            /* Get from keyboard info imgur url */
            string imgUrl = inlineKeyboardMarkup.InlineKeyboard.SelectMany((ik) => ik).FirstOrDefault((key) => key.Url != null).Url;
            /* Parse as enum urlType from callbackData */
            LinkRotateType linkType = (LinkRotateType)Enum.Parse(typeof(LinkRotateType), callbackData.Data["urlType"]);
            /* Call function to generate url and parse mode */
            var (messageUrl, parseMode) = Util.GenerateMessageAndFormatByLinkType(linkType, imgUrl);
            /* Declare next Link type */
            LinkRotateType nextLinkType = linkType.Next();
            /* Asign Property to proerty of change link type button */
            inlineKeyboardMarkup.InlineKeyboard.First().First().CallbackData = new CallbackData(HandlerName, CallbackImageAction.ChangeUrl).AddData("urlType", nextLinkType.ToString()).ToString();
            inlineKeyboardMarkup.InlineKeyboard.First().First().Text = nextLinkType.ToString();
            /* Edit message */
            await Bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, messageUrl, parseMode, true, inlineKeyboardMarkup);
        }
        #endregion
    }
}
