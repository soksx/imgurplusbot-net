using System;
using System.Collections.Generic;

namespace imgurplusbot.bll.Interfaces
{
    public interface ICallbackData
    {
        string Handler { get; }
        Dictionary<string, string> Data { get; }
        T GetAction<T>() where T : Enum;
    }
}
