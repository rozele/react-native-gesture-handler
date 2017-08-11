using Newtonsoft.Json.Linq;
using ReactNativeGestureHandler.Internal;
using Windows.UI.Input;

namespace ReactNativeGestureHandler.Module
{
    class TapGestureHandler : GestureHandler
    {
        private const string KeyTapNumberOfTaps = "numberOfTaps";
        private const string KeyTapMaxDurationMs = "maxDurationMs";
        private const string KeyTapMaxDelayMs = "maxDelayMs";

        private int _numberOfTaps;

        public override void Configure(JObject config)
        {
            base.Configure(config);

            // TODO: support max duration and max delay
            if (config.ContainsKey(KeyTapNumberOfTaps))
            {
                _numberOfTaps = config.Value<int>(KeyTapNumberOfTaps);
            }

            if (_numberOfTaps >= 2)
            {
                // TODO: support more than double tap
                GestureRecognizer.GestureSettings = GestureSettings.DoubleTap;
            }
            else
            {
                GestureRecognizer.GestureSettings = GestureSettings.Tap;
            }

            GestureRecognizer.Tapped += OnTapped;
        }

        protected override void OnDispose()
        {
            GestureRecognizer.GestureSettings = GestureSettings.None;
            GestureRecognizer.Tapped -= OnTapped;
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

        private void OnTapped(GestureRecognizer sender, TappedEventArgs args)
        {
            if (args.TapCount >= _numberOfTaps)
            {
                Activate();
                TriggerGestureEvent();
            }
        }
    }
}
