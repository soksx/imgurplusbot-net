using System;
namespace imgurplusbot.bll.Models
{
    public class BotConfiguration
    {
        public string BotToken { get; set; }

        public string Socks5Host { get; set; }

        public int? Socks5Port { get; set; }
        public string ImgurClientId { get; set; }
        public string ImgurApiKey{ get; set; }
    }
}
