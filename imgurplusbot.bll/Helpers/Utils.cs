using System;
using System.Reflection;
using System.Collections.Generic;
using IMG = Imgur.API.Enums;
using imgurplusbot.bll.Enums;
using imgurplusbot.bll.Helpers.Extensions;
using imgurplusbot.dal.Helpers;
using ImgurModels = imgurplusbot.dal.Models;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;

namespace imgurplusbot.bll.Helpers
{
    public static class Utils
    {
        public static (string, ParseMode) GenerateMessageAndFormatByLinkType(LinkRotateType linkRotateType, string msgLink)
        {
            return (string.Format((linkRotateType == LinkRotateType.TEXT) ? "{0}" : ((linkRotateType == LinkRotateType.HTML) ? "`{0}`" : "<pre>{0}</pre>"), string.Format(linkRotateType.GetFormat(), msgLink)), linkRotateType == LinkRotateType.HTML ? ParseMode.MarkdownV2 : ParseMode.Html);
        }
        public static bool IsMimeTypeImage(string mimeType)
        {
            return  mimeType.ToLowerInvariant().Contains("image") || mimeType.ToLowerInvariant() == "application/octet-stream";
        }
        public static bool IsMimeTypeVideo(string mimeType)
        {
            return mimeType.ToLowerInvariant().Contains("video");
        }
        public static bool FileSizeReached(int fileSize, IMG.FileType fileType, out string maxFileSize)
        {
            int maxFileSizeByType = (fileType == IMG.FileType.Image ? 10 : 200);
            maxFileSize = $"{maxFileSizeByType} MB";
            return fileSize > maxFileSizeByType * 1000 * 1000;
        }
        public static IEnumerable<T> GetTypesWithHelpAttribute<T>(Assembly assembly, Type helperAttribute) where T: Type
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(helperAttribute, true).Length > 0)
                {
                    yield return (T)(object)type;
                }
            }
        }
        public static ImgurModels.User AddOrGetUser(User tgUser) => DbUtils.GetUser((usr) => usr.TgId == tgUser.Id) ?? DbUtils.AddUser(tgUser.ToImgUser());
    }
}
