using System;

namespace Nent
{
    public static class EventUtilities
    {
        /// <summary>
        /// run the specified eventHandler if it is not null.
        /// Extension methods allow running methods on null objects
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void Raise(this EventHandler eventHandler,
            object sender, EventArgs e)
        {
            if (eventHandler != null)
            {
                eventHandler(sender, e);
            }
        }

        /// <summary>
        /// run the specified eventHandler if it is not null.
        /// Extension methods allow running methods on null objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventHandler"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void Raise<T>(this EventHandler<T> eventHandler,
            object sender, T e) where T : EventArgs
        {
            if (eventHandler != null)
            {
                eventHandler(sender, e);
            }
        }

        /// <summary>
        /// run the specified eventHandler if it is not null.
        /// Extension methods allow running methods on null objects
        /// </summary>
        /// <param name="eventHandler"></param>
        public static void Raise(this Action eventHandler)
        {
            if (eventHandler != null)
                eventHandler();
        }

        /// <summary>
        /// run the specified eventHandler if it is not null.
        /// Extension methods allow running methods on null objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventHandler"></param>
        /// <param name="arg"></param>
        public static void Raise<T>(this Action<T> eventHandler, T arg)
        {
            if (eventHandler != null)
                eventHandler(arg);
        }
    }
}
