using imgurplusbot.dal.Models;
using tg = Telegram.Bot.Types;

namespace imgurplusbot.bll.Helpers.Extensions
{
    public static class CustomContextExtension
    {
        public static User ToImgUser(this tg.User user)
        {
            return new User { TgId = user.Id, IsAdmin = false, UserName = user.Username };
        }
    }
}
