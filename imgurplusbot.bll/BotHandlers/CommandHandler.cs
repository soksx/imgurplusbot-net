using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using imgurplusbot.bll.Interfaces;
using Telegram.Bot.Types;

namespace imgurplusbot.bll.BotHandlers
{
    public class CommandHandler : BaseHandler, IBotHandler
    {
        public CommandHandler(IBotService botService) : base(botService) { }
        public override Task ProcessCallback(ICallbackData callback, Message message)
        {
            throw new NotImplementedException();
        }

        public override Task ProcessMessage(Message message)
        {
            throw new NotImplementedException();
        }
    }
}
