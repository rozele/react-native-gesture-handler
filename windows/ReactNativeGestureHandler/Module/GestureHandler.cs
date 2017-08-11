using Newtonsoft.Json.Linq;
using ReactNative.Bridge;
using ReactNative.UIManager;
using ReactNative.UIManager.Events;
using System;
using System.Collections.Generic;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace ReactNativeGestureHandler.Module
{
    abstract class GestureHandler : IDisposable
    {
        public const int StateUndetermined = 0;
        public const int StateFailed = 2;
        public const int StateBegan = 3;
        public const int StateCanceled = 4;
        public const int StateActive = 5;
        public const int StateEnd = 6;

        private GestureRecognizer _gestureRecognizer;

        private int _state = StateUndetermined;

        public int Tag { get; set; }

        public int ViewTag { get; set; }

        public EventDispatcher EventDispatcher { get; set; }

        public PointerEventHandler PointerPressedEventHandler { get; set; }

        public PointerEventHandler PointerMovedEventHandler { get; set; }

        public PointerEventHandler PointerReleasedEventHandler { get; set; }

        public PointerEventHandler PointerCaptureLostEventHandler { get; set; }

        protected GestureRecognizer GestureRecognizer
        {
            get
            {
                DispatcherHelpers.AssertOnDispatcher();
                if (_gestureRecognizer == null)
                {
                    _gestureRecognizer = new GestureRecognizer();
                }

                return _gestureRecognizer;
            }
        }

        public void Begin()
        {
            // TODO: introduce state machine constraints
            if (_state != StateBegan)
            {
                MoveToState(StateBegan);
            }
        }

        public void Activate()
        {
            // TODO: introduce state machine constraints
            if (_state != StateActive)
            {
                MoveToState(StateActive);
            }
        }

        public void End()
        {
            // TODO: introduce state machine constraints
            if (_state != StateEnd)
            {
                MoveToState(StateEnd);
            }
        }

        public void Cancel()
        {
            // TODO: introduce state machine constraints
            if (_state != StateCanceled)
            {
                MoveToState(StateCanceled);
            }
        }

        public virtual void Configure(JObject config)
        {
            GestureRecognizer.CompleteGesture();
        }

        public virtual void OnPointerPressed(PointerPoint pointerPoint)
        {
            GestureRecognizer.ProcessDownEvent(pointerPoint);
        }

        public virtual void OnPointerMoved(IList<PointerPoint> pointerPoints)
        {
            GestureRecognizer.ProcessMoveEvents(pointerPoints);
        }

        public virtual void OnPointerReleased(PointerPoint pointerPoint)
        {
            GestureRecognizer.ProcessUpEvent(pointerPoint);
        }

        public virtual void OnPointerCaptureLost()
        {
            GestureRecognizer.CompleteGesture();
        }

        public void Dispose()
        {
            GestureRecognizer.CompleteGesture();
            OnDispose();
        }

        protected void TriggerGestureEvent()
        {
            var eventData = new JObject
                {
                    { "handlerTag", Tag },
                    { "state", _state },
                };

            ExtractEventData(eventData);

            EventDispatcher.DispatchEvent(
                GestureHandlerEvent.Obtain(ViewTag, eventData));
        }

        protected virtual void ExtractEventData(JObject eventData)
        {
        }

        protected virtual void OnDispose()
        {
        }

        private void MoveToState(int state)
        {
            var oldState = _state;
            _state = state;

            var eventData = new JObject
                {
                    { "handlerTag", Tag },
                    { "oldState", oldState },
                    { "state", _state },
                };

            ExtractEventData(eventData);

            EventDispatcher.DispatchEvent(
                GestureHandlerStateChangeEvent.Obtain(ViewTag, eventData));
        }
    }

}
