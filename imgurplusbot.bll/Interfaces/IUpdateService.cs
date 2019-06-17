using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace imgurplusbot.bll.Interfaces
{
    public interface IUpdateService
    {
        Task ProcessRequest(Update update);
    }
}
