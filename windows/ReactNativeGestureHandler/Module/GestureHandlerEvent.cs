using Newtonsoft.Json.Linq;
using ReactNative.UIManager.Events;
using System;

namespace ReactNativeGestureHandler.Module
{
    class GestureHandlerEvent : Event
    {
        public const string Name = "onGestureHandlerEvent";

        private readonly JObject _eventData = new JObject();

        private GestureHandlerEvent(int viewTag, JObject eventData) 
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

        public static GestureHandlerEvent Obtain(
            int viewTag,
            JObject eventData)
        {
            return new GestureHandlerEvent(viewTag, eventData);
        }
    }
}