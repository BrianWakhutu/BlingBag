using System;
using System.Collections;
using System.Reflection;
using System.Threading.Tasks;

namespace BlingBag
{
    public abstract class BlingDispatcherBase : IBlingDispatcher
    {
        #region IBlingDispatcher Members

        public async Task Dispatch(object @event)
        {
            IEnumerable matchingBlingHandlers = FindHandlers(@event);
            foreach (object handler in matchingBlingHandlers)
            {
                LogInfo(handler, DateTime.UtcNow, string.Format("Dispatching {0}...", handler.GetType().Name));

                try
                {
                    await InvokeMethod("Handle", handler, @event);
                    LogInfo(handler, DateTime.UtcNow, string.Format("Finished {0}.", handler.GetType().Name));
                }
                catch (TargetInvocationException ex)
                {
                    LogException(handler, DateTime.UtcNow, ex.InnerException);
                    throw;
                }
                catch (AggregateException ex)
                {
                    LogException(handler, DateTime.UtcNow, ex.InnerException);
                    throw;
                }
                catch (Exception ex)
                {
                    LogException(handler, DateTime.UtcNow, ex);
                    throw;
                }
            }
        }

        async Task InvokeMethod(string methodName, object invokableObject, params object[] methodArgs)
        {
            try
            {
                MethodInfo handlerMethod = invokableObject.GetType().GetMethod(methodName);
                await (Task) handlerMethod.Invoke(invokableObject, methodArgs);
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
        }

        #endregion

        protected abstract IEnumerable FindHandlers(object @event);
        protected abstract void LogInfo(object handler, DateTime timeStamp, string message);
        protected abstract void LogException(object handler, DateTime timeStamp, Exception exception);
    }
}