using System;
using MihaZupan;
using Telegram.Bot;
using imgurplusbot.bll.Models;
using imgurplusbot.bll.Interfaces;
using Microsoft.Extensions.Options;

namespace imgurplusbot.bll.BotServices
{
    public class BotService : IBotService
    {
        private readonly BotConfiguration _config;
        public BotService(IOptions<BotConfiguration> config)
        {
            _config = config.Value;
            Client = string.IsNullOrEmpty(_config.Socks5Host) && !_config.Socks5Port.HasValue
                ? new TelegramBotClient(_config.BotToken)
                : new TelegramBotClient(
                    _config.BotToken,
                    new HttpToSocks5Proxy(_config.Socks5Host, _config.Socks5Port.Value));
        }
        public TelegramBotClient Client { get; }
    }
}
