using imgurplusbot.bll.Enums;
using imgurplusbot.bll.Models;
using System.Collections.Generic;

namespace imgurplusbot.bll.Interfaces
{
    public interface ICallbackData
    {
        CallbackAction Action { get; }
        Dictionary<string, string> Data { get; }
    }
}
