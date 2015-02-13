using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrowserAuto.Install
{
    internal sealed class Waiter
    {
        private static readonly TimeSpan IdleTime = TimeSpan.FromSeconds(1);

        public static Task<T> Start<T>(Func<T> func, CancellationToken token) where T : class
        {
            return Task.Factory.StartNew(() => UntilNotNull(func, token), token);
        }

        public static Task Start(Func<bool> pred, CancellationToken token)
        {
            return Task.Factory.StartNew(() => Until(pred, token), token);
        }

        private static void Until(Func<bool> predicate, CancellationToken token)
        {
            while (true)
            {
                var result = predicate();

                token.ThrowIfCancellationRequested();

                if (result)
                {
                    return;
                }

                Task.Delay(IdleTime, token).Wait(token);
            }
        }

        private static T UntilNotNull<T>(Func<T> func, CancellationToken token) where T : class
        {
            while (true)
            {
                var result = func();

                token.ThrowIfCancellationRequested();

                if (result != null)
                {
                    return result;
                }

                Task.Delay(IdleTime, token).Wait(token);
            }
        }
    }
}
