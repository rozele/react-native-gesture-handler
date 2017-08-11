using Newtonsoft.Json.Linq;
using System;
using Windows.UI.Input;

namespace ReactNativeGestureHandler.Module
{
    class RotationGestureHandler : GestureHandler
    {
        private float _rotation;
        private float _velocity;

        public override void Configure(JObject config)
        {
            base.Configure(config);
            GestureRecognizer.GestureSettings = GestureSettings.ManipulationRotate;
            GestureRecognizer.ManipulationStarted += OnManipulationStarted;
            GestureRecognizer.ManipulationUpdated += OnManipulationUpdated;
            GestureRecognizer.ManipulationCompleted += OnManipulationCompleted;
        }

        protected override void ExtractEventData(JObject eventData)
        {
            eventData.Add("rotation", _rotation * Math.PI / 180);
            eventData.Add("velocity", _velocity * Math.PI / 180);
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
            _rotation = args.Cumulative.Rotation;
            _velocity = 0;
            TriggerGestureEvent();
        }

        private void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            _rotation = args.Cumulative.Rotation;
            _velocity = args.Velocities.Angular;
            TriggerGestureEvent();
        }

        private void OnManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
            End();
        }
    }
}
