using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using imgurplusbot.bll.Enums;
using Telegram.Bot.Types.Enums;
using IMG = Imgur.API.Enums;
using static imgurplusbot.bll.Helpers.Extensions.EnumExtensions;
using System.Threading.Tasks;
using System.Reflection;

namespace imgurplusbot.bll.Helpers
{
    public static class Util
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
    }
}
