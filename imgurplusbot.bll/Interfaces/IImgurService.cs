using Imgur.API.Endpoints;
using System;
using System.Collections.Generic;
using System.Text;

namespace imgurplusbot.bll.Interfaces
{
    public interface IImgurService
    {
        ImageEndpoint ImageEndpoint { get; }
        RateLimitEndpoint RateLimitEndpoint { get; }
    }
}
