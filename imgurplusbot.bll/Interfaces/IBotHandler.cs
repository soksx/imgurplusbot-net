using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace imgurplusbot.bll.Interfaces
{
    public interface IBotHandler
    {
        abstract Task ProcessMessage(Message message);
        abstract Task ProcessCallback(ICallbackData callback, Message message);
    }
}
