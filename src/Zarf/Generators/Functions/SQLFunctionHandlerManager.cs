using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Zarf.Metadata.DataAnnotations;

namespace Zarf.Generators.Functions
{
    public class SQLFunctionHandlerManager : ISQLFunctionHandlerManager
    {
        private static List<ISQLFunctionHandler> _coreHandlers;

        //SQLFunctionHandlerAttribute 注册的Handler
        private static ConcurrentDictionary<Type, ISQLFunctionHandler> _atrributeHanders;

        private ConcurrentDictionary<Type, List<ISQLFunctionHandler>> _handlers;

        static SQLFunctionHandlerManager()
        {
            _coreHandlers = new List<ISQLFunctionHandler>();

            _atrributeHanders = new ConcurrentDictionary<Type, ISQLFunctionHandler>();

            foreach (var item in Assembly
                        .GetExecutingAssembly()
                        .GetTypes()
                        .Where(t => typeof(ISQLFunctionHandler).IsAssignableFrom(t) && !t.IsAbstract))
            {
                _coreHandlers.Add(Activator.CreateInstance(item) as ISQLFunctionHandler);
            }
        }

        public SQLFunctionHandlerManager()
        {
            _handlers = new ConcurrentDictionary<Type, List<ISQLFunctionHandler>>();

            RegisterCoreHandler();
        }

        protected void RegisterCoreHandler()
        {
            foreach (var item in _coreHandlers)
            {
                Register(item);
            }
        }

        public void Register(ISQLFunctionHandler handler)
        {
            _handlers.AddOrUpdate(handler.SoupportedType, new List<ISQLFunctionHandler>() { handler }, (k, handlers) =>
            {
                if (!handlers.Contains(handler))
                {
                    handlers.Insert(0, handler);
                }

                return handlers;
            });
        }

        public IEnumerable<ISQLFunctionHandler> GetHandlers(MethodInfo method)
        {
            //特性注解的Handler
            var handler = FindFunctionHandlerWithAnnotations(method);
            if (handler != null)
            {
                return new[] { handler };
            }

            _handlers.TryGetValue(method.DeclaringType, out var handlers);

            if (!typeof(IEnumerable).IsAssignableFrom(method.DeclaringType))
            {
                return handlers;
            }

            _handlers.TryGetValue(typeof(IEnumerable), out var collectionHandlers);

            return (handlers == null)
                ? collectionHandlers
                : handlers.Union(collectionHandlers);
        }

        internal static ISQLFunctionHandler FindFunctionHandlerWithAnnotations(MethodInfo method)
        {
            var attribute = method.GetCustomAttribute<SQLFunctionHandlerAttribute>();
            if (attribute == null)
            {
                return null;
            }

            return _atrributeHanders
                .GetOrAdd(attribute.Handler, k => Activator.CreateInstance(attribute.Handler) as ISQLFunctionHandler);
        }
    }
}
