using Imgur.API.Endpoints;
using Imgur.API.Models;
using imgurplusbot.bll.Enums;
using imgurplusbot.bll.Helpers;
using imgurplusbot.bll.Helpers.Attributes;
using imgurplusbot.bll.Helpers.Extensions;
using imgurplusbot.bll.Interfaces;
using imgurplusbot.bll.Models;
using imgurplusbot.dal.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using IMG = Imgur.API.Enums;
using ImgurModels = imgurplusbot.dal.Models;
using IO = System.IO;
using TGENUMS = Telegram.Bot.Types.Enums;

namespace imgurplusbot.bll.BotHandlers
{
    [Handler(TGENUMS.MessageType.Document, TGENUMS.MessageType.Photo, Name = "ImageHandler")]
    public class ImageHandler : BaseHandler
    {
        private readonly IImgurService _imgurService;
        #region Constructors
        public ImageHandler(IBotService botService, IImgurService imgurService) : base(botService)
        {
            _imgurService = imgurService;
        }
        #endregion
        #region Overrides
        public override Task ProcessCallback(ICallbackData callback, Message message, string callbackQueryId) => ProcessAction(callback, message, callbackQueryId);
        public override Task ProcessMessage(Message message) => ProcessUpload(message);
        #endregion
        #region Internal Methods
        private async Task ProcessUpload(Message message)
        {
            ImgurModels.User ImgurUser = Utils.AddOrGetUser(message.From);

            long chatId = message.Chat.Id;
            int messageId = message.MessageId;
            string fileId = null;
            IMG.FileType fileType = IMG.FileType.Image;

            if (message.Type == TGENUMS.MessageType.Photo)
            {
                fileId = message.Photo.Last().FileId;
            }
            else if (message.Type == TGENUMS.MessageType.Document && Utils.IsMimeTypeImage(message.Document.MimeType)) /* Image send as doc */
            {
                fileId = message.Document.FileId;
            }
            else if (message.Type == TGENUMS.MessageType.Document && Utils.IsMimeTypeVideo(message.Document.MimeType)) /* GIF or Video */
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

            if (Utils.FileSizeReached(tgFile.FileSize, fileType, out string maxFileSize))
            {
                await SendMessage(message.Chat.Id, $"Maximum file size reached! Max filesize for {fileType}: {maxFileSize}");
                return;
            }

            /* Handle File size */
            await UploadImage(ImgurUser.Id, chatId, messageId, tgFile, fileType);
        }
        private async Task ProcessAction(ICallbackData callback, Message message, string callbackQueryId)
        {
            switch (callback.GetAction<CallbackImageAction>())
            {
                case CallbackImageAction.ChangeUrl:
                    await GenerateRotateLinks(callback, message);
                    break;
                case CallbackImageAction.Delete:
                    await DeleteImage(callback, message, callbackQueryId);
                    break;
                case CallbackImageAction.ConvertToMP4:
                    await ConvertCurrentImgToMP4(callback, message);
                    break;
                case CallbackImageAction.ConvertToGIF:
                    await ConvertCurrentImgToGIF(callback, message, callbackQueryId);
                    break;
                default:
                    break;
            }
        }

        #endregion
        #region Private Methods
        private async Task UploadImage(long imgUserId, long chatId, int messageId, File tgFile, IMG.FileType fileType)
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
                List<InlineKeyboardButton[]> inlineKeyboardButtons = new List<InlineKeyboardButton[]>
                    {
                        new InlineKeyboardButton[] /* First Row */
                        {
                            InlineKeyboardButton.WithCallbackData("BB", new CallbackData(HandlerName, CallbackImageAction.ChangeUrl).AddData("urlType", "BB").ToString()),
                            InlineKeyboardButton.WithCallbackData("DELETE", new CallbackData(HandlerName, CallbackImageAction.Delete).AddData("deleteHash", imageUploaded.DeleteHash).ToString())
                        },
                        new InlineKeyboardButton[] /* Second/Third Row */
                        {
                            InlineKeyboardButton.WithUrl("Open Link", imageUploaded.Link)
                        }
                    };
                if (fileType == IMG.FileType.Video)
                    inlineKeyboardButtons.Insert(1, new InlineKeyboardButton[] /* Second Row */
                        {
                            InlineKeyboardButton.WithCallbackData("MP4", new CallbackData(HandlerName, CallbackImageAction.ConvertToMP4).AddDataRange(new []{ ("H", imageUploaded.Id), ("DH", imageUploaded.DeleteHash) }).ToString()),
                            InlineKeyboardButton.WithCallbackData("GIF", new CallbackData(HandlerName, CallbackImageAction.ConvertToGIF).AddData("URL", imageUploaded.Link).ToString())
                        });
                inlineKeyboardMarkup = new InlineKeyboardMarkup(inlineKeyboardButtons);
            }
            DbUtils.AddUserUpload(new ImgurModels.UserUpload { UserId = imgUserId, TgFileId = tgFile.FileId, UploadDate = DateTimeOffset.Now, ImgurLink = imageUploaded?.Link, ImgurDeleteHash = imageUploaded?.DeleteHash });
            await Bot.EditMessageTextAsync(chatId, reepplyMessage.MessageId, imageUploaded?.Link ?? "<pre>Something is wrong! Try again later</pre>", TGENUMS.ParseMode.Html, true, inlineKeyboardMarkup);
        }
        private async Task DeleteImage(ICallbackData callback, Message message, string callbackQueryId)
        {
            string deleteHash = callback.Data["deleteHash"];
            int rows = message.ReplyMarkup.InlineKeyboard.Count();
            
            string msg = $"{((rows == 3) ? "Video & GIF (if it is generated)," : "Image")} was deleted from imgur!";

            if (rows == 3)
            {
                Func<InlineKeyboardButton, bool> mp4Search = (btn) => btn.Text == "MP4";
                Func<InlineKeyboardButton, bool> gifSearch = (btn) => btn.Text == "GIF";
                string mp4DeleteHash = ((CallbackData)message.ReplyMarkup.InlineKeyboard.FirstOrDefault((kbRows) => kbRows.Any(mp4Search)).FirstOrDefault(mp4Search)?.CallbackData).Data["DH"];
                string gifDeleteHash = ((CallbackData)message.ReplyMarkup.InlineKeyboard.FirstOrDefault((kbRows) => kbRows.Any(gifSearch)).FirstOrDefault(gifSearch)?.CallbackData).Data.TryGetValue("DH", out string val) ? val : null;
                await DeleteImageInternal(mp4DeleteHash, message.Chat.Id, message.MessageId);
                if (!string.IsNullOrEmpty(gifDeleteHash))
                    await DeleteImageInternal(gifDeleteHash, message.Chat.Id, message.MessageId, true);
            }
            else
                await DeleteImageInternal(deleteHash, message.Chat.Id, message.MessageId);

            await Bot.AnswerCallbackQueryAsync(callbackQueryId, msg, false);

        }

        private async Task DeleteImageInternal(string deleteHash, ChatId chatId, int messageId, bool bypassDb = false)
        {
            if (!bypassDb)
                DbUtils.DeteleUserUpload((usrUpload) => usrUpload.ImgurDeleteHash == deleteHash);
            await _imgurService.ImageEndpoint.DeleteImageAsync(deleteHash);
            await Bot.DeleteMessageAsync(chatId, messageId);
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
            var (messageUrl, parseMode) = Utils.GenerateMessageAndFormatByLinkType(linkType, imgUrl);
            /* Declare next Link type */
            LinkRotateType nextLinkType = linkType.Next();
            /* Asign Property to proerty of change link type button */
            inlineKeyboardMarkup.InlineKeyboard.First().First().CallbackData = new CallbackData(HandlerName, CallbackImageAction.ChangeUrl).AddData("urlType", nextLinkType.ToString()).ToString();
            inlineKeyboardMarkup.InlineKeyboard.First().First().Text = nextLinkType.ToString();
            /* Edit message */
            await Bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, messageUrl, parseMode, true, inlineKeyboardMarkup);
        }
        private async Task ConvertCurrentImgToMP4(ICallbackData callbackData, Message message) => await ReplaceConvertAction(callbackData, message.Chat.Id, message.MessageId, message.ReplyMarkup, "mp4");
        
        private async Task ConvertCurrentImgToGIF(ICallbackData callbackData, Message message, string callbackQueryId)
        {
            InlineKeyboardMarkup inlineKeyboard = null;
            ICallbackData newCallbackData = null;
            if (!(callbackData.Data.ContainsKey("H") && callbackData.Data.ContainsKey("DH")))
            {
                string mp4Link = callbackData.Data["URL"];
                IVidToGIF vidToGIF = null;
                try
                {
                    vidToGIF = await _imgurService.VidToGIFEndpoint.ConvertVidToGIF(mp4Link);
                }
                catch { }
                if (vidToGIF != null)
                {
                    inlineKeyboard = new InlineKeyboardMarkup(message.ReplyMarkup.InlineKeyboard);
                    Func<InlineKeyboardButton, bool> gifBtnSearch = (btn) => btn.Text == "GIF";
                    inlineKeyboard.InlineKeyboard.FirstOrDefault((kbRows) => kbRows.Any(gifBtnSearch)).FirstOrDefault(gifBtnSearch).CallbackData = ((CallbackData)(newCallbackData = new CallbackData(HandlerName, CallbackImageAction.ConvertToGIF).AddDataRange(new[] { ("H", vidToGIF.Id), ("DH", vidToGIF.DelteHash) }))).ToString();
                }
                else
                {
                    await Bot.AnswerCallbackQueryAsync(callbackQueryId, "Error converting vid to GIF!");
                    return;
                }
            }
            await ReplaceConvertAction(newCallbackData ?? callbackData, message.Chat.Id, message.MessageId, inlineKeyboard ?? message.ReplyMarkup, "gif");
        }
        private async Task ReplaceConvertAction(ICallbackData callbackData, ChatId chatId, int messageId, InlineKeyboardMarkup inlineKeyboard, string fileExtension)
        {
            /* Search function Lambda expression */
            Func<InlineKeyboardButton, bool> dltSearch = (btn) => btn.Text == "DELETE";
            ICallbackData deleteBtnCallback = (CallbackData)inlineKeyboard.InlineKeyboard.FirstOrDefault((kbRows) => kbRows.Any(dltSearch)).FirstOrDefault(dltSearch).CallbackData;
            /* Check if is MP4/GIF currently */
            if (deleteBtnCallback.Data["deleteHash"] == callbackData.Data["DH"])
                return;
            /* Create a copy of current keyboard */
            InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(inlineKeyboard.InlineKeyboard);
            /* Set MP4/GIF deletehash*/
            inlineKeyboardMarkup.InlineKeyboard.FirstOrDefault((kbRows) => kbRows.Any(dltSearch)).FirstOrDefault(dltSearch).CallbackData = new CallbackData(HandlerName, CallbackImageAction.Delete).AddData("deleteHash", callbackData.Data["DH"]).ToString();
            /* Set MP4/GIF OpenLink */
            Func<InlineKeyboardButton, bool> opnLinkSearch = (btn) => btn.Text == "Open Link";
            string oldLink = inlineKeyboardMarkup.InlineKeyboard.FirstOrDefault((kbRows) => kbRows.Any(opnLinkSearch)).FirstOrDefault(opnLinkSearch).Url;
            var (oldImgId, oldimgFormt, _) = oldLink.Split('/')[^1].Split('.');
            string newLink = oldLink.Replace(oldImgId, callbackData.Data["H"]).Replace(oldimgFormt, fileExtension);
            inlineKeyboardMarkup.InlineKeyboard.FirstOrDefault((kbRows) => kbRows.Any(opnLinkSearch)).FirstOrDefault(opnLinkSearch).Url = newLink;
            /* Get Parse Mode And Link */
            LinkRotateType linkType = (LinkRotateType)Enum.Parse(typeof(LinkRotateType), ((CallbackData)inlineKeyboardMarkup.InlineKeyboard.First().First().CallbackData).Data["urlType"]);
            var (messageUrl, parseMode) = Utils.GenerateMessageAndFormatByLinkType(linkType.Prev(), newLink);
            /* Edit Message */
            await Bot.EditMessageTextAsync(chatId, messageId, messageUrl, parseMode, true, inlineKeyboardMarkup);
        }
        #endregion
    }
}
