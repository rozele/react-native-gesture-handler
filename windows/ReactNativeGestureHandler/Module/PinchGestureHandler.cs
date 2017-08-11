using Newtonsoft.Json.Linq;
using Windows.UI.Input;

namespace ReactNativeGestureHandler.Module
{
    class PinchGestureHandler : GestureHandler
    {
        private float _scale;
        private float _velocity;

        public override void Configure(JObject config)
        {
            base.Configure(config);
            GestureRecognizer.GestureSettings = GestureSettings.ManipulationScale;
            GestureRecognizer.ManipulationStarted += OnManipulationStarted;
            GestureRecognizer.ManipulationUpdated += OnManipulationUpdated;
            GestureRecognizer.ManipulationCompleted += OnManipulationCompleted;
        }

        protected override void ExtractEventData(JObject eventData)
        {
            eventData.Add("scale", _scale);
            eventData.Add("velocity", _velocity);
        }

        protected override void OnDispose()
        {
            GestureRecognizer.GestureSettings = GestureSettings.None;
            GestureRecognizer.ManipulationStarted -= OnManipulationStarted;
            GestureRecognizer.ManipulationUpdated -= OnManipulationUpdated;
            GestureRecognizer.ManipulationCompleted -= OnManipulationCompleted;
        }

        private void OnManipulationStarted(GestureRecognizer sender, ManipulationStartedEventArgs args)
        {
            Begin();
            Activate();
            _scale = args.Cumulative.Scale;
            _velocity = 0;
            TriggerGestureEvent();
        }

        private void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            _scale = args.Cumulative.Scale;
            _velocity = args.Velocities.Expansion;
            TriggerGestureEvent();
        }

        private void OnManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
            End();
        }
    }
}
