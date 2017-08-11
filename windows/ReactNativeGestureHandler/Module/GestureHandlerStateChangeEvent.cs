using Newtonsoft.Json.Linq;
using ReactNative.UIManager.Events;
using System;

namespace ReactNativeGestureHandler.Module
{
    class GestureHandlerStateChangeEvent : Event
    {
        public const string Name = "onGestureHandlerStateChange";

        private readonly JObject _eventData;

        private GestureHandlerStateChangeEvent(int viewTag, JObject eventData)
            : base(viewTag, TimeSpan.FromTicks(Environment.TickCount))
        {
            _eventData = eventData;
        }

        public override bool CanCoalesce
        {
            get
            {
                return false;
            }
        }

        public override string EventName
        {
            get
            {
                return Name;
            }
        }

        public override void Dispatch(RCTEventEmitter eventEmitter)
        {
            eventEmitter.receiveEvent(ViewTag, EventName, _eventData);
        }

        public static GestureHandlerStateChangeEvent Obtain(
            int viewTag,
            JObject eventData)
        {
            return new GestureHandlerStateChangeEvent(viewTag, eventData);
        }
    }
}