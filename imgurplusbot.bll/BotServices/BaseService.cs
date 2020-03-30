using imgurplusbot.bll.Helpers;
using imgurplusbot.bll.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace imgurplusbot.bll.BotServices
{
    public class BaseService
    {
        public BaseService(params object[] parms)
        {
            ClassLoader<IBotHandler>.SetParametersInfo(parms);
        }
    }
}
