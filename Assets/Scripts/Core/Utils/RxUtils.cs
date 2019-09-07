using System;
using UniRx;

namespace UniRx
{
    public static class RxUtils
    {

        public static void SafeUnsubscribe(ref IDisposable subscribe)
        {
            if (subscribe != null) subscribe.Dispose();
            subscribe = null;
        }

        public static void SafeUnsubscribe(ref CompositeDisposable subscribe)
        {
            if (subscribe != null) subscribe.Dispose();
            subscribe = null;
        }

    }
}