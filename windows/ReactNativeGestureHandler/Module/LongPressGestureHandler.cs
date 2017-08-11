using Newtonsoft.Json.Linq;
using Windows.UI.Input;

namespace ReactNativeGestureHandler.Module
{
    class LongPressGestureHandler : GestureHandler
    {
        private const string KeyLongPressMinDurationMs = "minDurationMs";

        public override void Configure(JObject config)
        {
            // TODO: support minimum delay
            GestureRecognizer.GestureSettings = GestureSettings.Hold | GestureSettings.HoldWithMouse;
            GestureRecognizer.Holding += OnHolding;
        }

        public override void OnPointerPressed(PointerPoint pointerPoint)
        {
            base.OnPointerPressed(pointerPoint);
            Begin();
        }

        public override void OnPointerReleased(PointerPoint pointerPoint)
        {
            base.OnPointerReleased(pointerPoint);
            End();
        }

        public override void OnPointerCaptureLost()
        {
            base.OnPointerCaptureLost();
            Cancel();
        }

        protected override void OnDispose()
        {
            GestureRecognizer.GestureSettings = GestureSettings.None;
            GestureRecognizer.Holding -= OnHolding;
        }

        private void OnHolding(GestureRecognizer sender, HoldingEventArgs args)
        {
            // TODO: what about other holding states?
            if (args.HoldingState == HoldingState.Started)
            {
                Activate();
                TriggerGestureEvent();
            }
        }
    }
}
