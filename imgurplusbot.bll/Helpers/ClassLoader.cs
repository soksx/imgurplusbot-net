using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using imgurplusbot.bll.Interfaces;
using imgurplusbot.bll.Helpers.Attributes;
using imgurplusbot.bll.Helpers.Extensions;
using imgurplusbot.bll.BotHandlers;

namespace imgurplusbot.bll.Helpers
{
    public sealed class ClassLoader<T> : IClassLoader
    {
        #region Private Properties
        private static readonly ThreadSafeCache<T> _instance = new ThreadSafeCache<T>();
        private static object[] _parameters;
        private static Dictionary<Type, object> mappings = new Dictionary<Type, object>();
        #endregion
        #region Constructor
        private ClassLoader()
        {
        }
        #endregion
        #region Public Properties
        public static ThreadSafeCache<T> Instance
        {
            get
            {
                if (_instance.Size() == 0)
                    LoadHandlerClass();
                return _instance;
            }
        }
        #endregion
        #region Public Methods
        public static void SetParametersInfo(params object[] classParams)
        {
            if (_parameters == null)
                _parameters = classParams;
        }
        #endregion
        #region Internal Methods
        internal static void LoadHandlerClass()
        {
            if (_parameters == null)
                throw new NullReferenceException($"{nameof(_parameters)} is not fullified. Cannot instance any class without parameters");
            Parallel.ForEach(Util.GetTypesWithHelpAttribute<Type>(Assembly.GetExecutingAssembly(), typeof(Handler)), (loaded) =>
            {
                object[] matchParameters = _parameters.Where((pa) => loaded.GetConstructors()[0].GetParameters().Any((conspa) => conspa.ParameterType.IsAssignableFrom(pa.GetType()))).ToArray();
                IBaseHandler handler = (IBaseHandler)loaded.GetConstructors()[0].Invoke(matchParameters);
                _instance.Add(handler.HandlerName, handler.MessageType, (T)handler);
            });
        }
        #endregion
    }
}
