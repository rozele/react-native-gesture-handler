using Newtonsoft.Json.Linq;
using Windows.UI.Input;

namespace ReactNativeGestureHandler.Module
{
    class PanGestureHandler : GestureHandler
    {
        private const string KeyPanMinDeltaX = "minDeltaX";
        private const string KeyPanMinDeltaY = "minDeltaY";
        private const string KeyPanMaxDeltaX = "maxDeltaX";
        private const string KeyPanMaxDeltaY = "maxDeltaY";
        private const string KeyPanMinOffsetX = "minOffsetX";
        private const string KeyPanMinOffsetY = "minOffsetY";
        private const string KeyPanMinDist = "minDist";
        private const string KeyPanMinVelocity = "minVelocity";
        private const string KeyPanMinVelocityX = "minVelocityX";
        private const string KeyPanMinVelocityY = "minVelocityY";
        private const string KeyPanMinPointers = "minPointers";
        private const string KeyPanMaxPointers = "maxPointers";
        private const string KeyPanAvgTouches = "avgTouches";

        private double _translationX;
        private double _translationY;
        private double _velocityX;
        private double _velocityY;

        public override void Configure(JObject config)
        {
            // TODO: use parameters
            base.Configure(config);
            GestureRecognizer.GestureSettings = GestureSettings.ManipulationTranslateX | GestureSettings.ManipulationTranslateY;
            GestureRecognizer.ManipulationStarted += OnManipulationStarted;
            GestureRecognizer.ManipulationUpdated += OnManipulationUpdated;
            GestureRecognizer.ManipulationCompleted += OnManipulationCompleted;
        }

        protected override void ExtractEventData(JObject eventData)
        {
            eventData.Add("translationX", _translationX);
            eventData.Add("translationY", _translationY);
            eventData.Add("velocityX", _velocityX);
            eventData.Add("velocityY", _velocityY);
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
            _translationX = args.Cumulative.Translation.X;
            _translationY = args.Cumulative.Translation.Y;
            _velocityX = 0;
            _velocityY = 0;
            TriggerGestureEvent();
        }

        private void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            _translationX = args.Cumulative.Translation.X;
            _translationY = args.Cumulative.Translation.Y;
            _velocityX = args.Velocities.Linear.X;
            _velocityY = args.Velocities.Linear.Y;
            TriggerGestureEvent();
        }

        private void OnManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
            End();
        }
    }
}
