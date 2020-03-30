using Imgur.API.Authentication;
using Imgur.API.Endpoints;
using imgurplusbot.bll.Interfaces;
using imgurplusbot.bll.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace imgurplusbot.bll.BotServices
{
    public class ImgurService : IImgurService
    {
        private readonly BotConfiguration _config;
        public ImgurService(IOptions<BotConfiguration> config)
        {
            _config = config.Value;
            _ImgurClient = new ImgurClient(_config.ImgurClientId, _config.ImgurApiKey);
            ImageEndpoint = new ImageEndpoint(_ImgurClient);
            RateLimitEndpoint = new RateLimitEndpoint(_ImgurClient);
        }

        private ImgurClient _ImgurClient { get; }

        public ImageEndpoint ImageEndpoint { get; }

        public RateLimitEndpoint RateLimitEndpoint { get; }
    }
}
