using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using imgurplusbot.bll.Helpers;

namespace imgurplusbot.bll.Interfaces
{
    public interface IClassLoader
    {
       static ThreadSafeCache<Type> Instance { get; }
    }
}
