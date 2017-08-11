using Newtonsoft.Json.Linq;
using ReactNative.Bridge;
using ReactNative.UIManager;
using ReactNative.UIManager.Events;
using ReactNativeGestureHandler.Internal;
using System;
using System.Collections.Generic;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace ReactNativeGestureHandler.Module
{
    class GestureHandlerModule : ReactContextNativeModuleBase
    {
        private const string KeyShouldCancelWhenOutside = "shouldCancelWhenOutside";
        private const string KeyHitSlop = "hitSlop";
        private const string KeyHitSlopLeft = "left";
        private const string KeyHitSlopTop = "left";
        private const string KeyHitSlopRight = "right";
        private const string KeyHitSlopBottom = "bottom";
        private const string KeyHitSlopVertical = "vertical";
        private const string KeyHitSlopHorizontal = "horizontal";
        private const string KeyNativeViewShouldActivateOnStart = "shouldActivateOnStart";
        private const string KeyNativeViewDisallowInterruption = "disallowInterruption";

        private static IDictionary<string, Func<GestureHandler>> _gestureHandlerFactories =
            new Dictionary<string, Func<GestureHandler>>
            {
                { nameof(LongPressGestureHandler), () => new LongPressGestureHandler() },
                { nameof(PanGestureHandler), () => new PanGestureHandler() },
                { nameof(PinchGestureHandler), () => new PinchGestureHandler() },
                { nameof(RotationGestureHandler), () => new RotationGestureHandler() },
                { nameof(TapGestureHandler), () => new TapGestureHandler() },
            };

        private readonly IDictionary<int, object> _gestureHandlerGates =
            new Dictionary<int, object>();

        private readonly IDictionary<int, IList<GestureHandler>> _gestureHandlers =
            new Dictionary<int, IList<GestureHandler>>();

        private readonly IDictionary<int, IUIBlock> _dropUIBlocks =
            new Dictionary<int, IUIBlock>();

        public GestureHandlerModule(ReactContext reactContext)
            : base(reactContext)
        {
        }

        public override IReadOnlyDictionary<string, object> Constants
        {
            get
            {
                return new Dictionary<string, object>
                {
                    {
                        "State",
                        new Dictionary<string, object>
                        {
                            { "UNDETERMINED", GestureHandler.StateUndetermined },
                            { "BEGAN", GestureHandler.StateBegan },
                            { "ACTIVE", GestureHandler.StateActive },
                            { "CANCELLED", GestureHandler.StateCanceled },
                            { "FAILED", GestureHandler.StateFailed },
                            { "END", GestureHandler.StateEnd },
                        }
                    },
                };
            }
        }

        public override string Name
        {
            get
            {
                return "RNGestureHandlerModule";
            }
        }

        [ReactMethod]
        public void createGestureHandler(
            int viewTag,
            string handlerName,
            int handlerTag,
            JObject config)
        {
            var handlerFactory = default(Func<GestureHandler>);
            if (!_gestureHandlerFactories.TryGetValue(handlerName, out handlerFactory))
            {
                return;
            }

            var handler = handlerFactory();
            handler.Tag = handlerTag;
            handler.ViewTag = viewTag;

            var uiManager = Context.GetNativeModule<UIManagerModule>();
            handler.EventDispatcher = uiManager.EventDispatcher;

            var handlers = default(IList<GestureHandler>);
            var gate = default(object);
            if (!_gestureHandlers.TryGetValue(viewTag, out handlers) ||
                !_gestureHandlerGates.TryGetValue(viewTag, out gate))
            {
                handlers = new List<GestureHandler> { handler };
                gate = new object();
                _gestureHandlers.Add(viewTag, handlers);
                _gestureHandlerGates.Add(viewTag, gate);
                uiManager.AddUIBlock(
                    new CompositeUIBlock(
                        new UpdateGestureRecognizerUIBlock(handler, config),
                        new AddPointerEventsUIBlock(viewTag, handlers, gate)));

            }
            else
            {
                lock (gate)
                {
                    handlers.Add(handler);
                }

                uiManager.AddUIBlock(new UpdateGestureRecognizerUIBlock(handler, config));
            }
        }

        [ReactMethod]
        public void updateGestureHandler(
            int viewTag,
            int handlerTag,
            JObject config)
        {
            var handlers = default(IList<GestureHandler>);
            var gate = default(object);
            if (!_gestureHandlers.TryGetValue(viewTag, out handlers) || !_gestureHandlerGates.TryGetValue(viewTag, out gate))
            {
                return;
            }

            lock (gate)
            {
                foreach (var handler in handlers)
                {
                    if (handler.Tag == handlerTag)
                    {
                        Context.GetNativeModule<UIManagerModule>().AddUIBlock(new UpdateGestureRecognizerUIBlock(handler, config));
                        break;
                    }
                }
            }
        }

        [ReactMethod]
        public void dropGestureHandlersForView(int viewTag)
        {
            var handlers = default(IList<GestureHandler>);
            if (!_gestureHandlers.TryGetValue(viewTag, out handlers))
            {
                return;
            }

            var uiBlock = default(IUIBlock);
            if (!_dropUIBlocks.TryGetValue(viewTag, out uiBlock))
            {
                // TODO: throw
                return;
            }

            _gestureHandlers.Remove(viewTag);
            _gestureHandlerGates.Remove(viewTag);
            _dropUIBlocks.Remove(viewTag);

            // TODO: UI Thread?
            foreach (var handler in handlers)
            {
                handler.Dispose();
            }

            Context.GetNativeModule<UIManagerModule>().AddUIBlock(uiBlock);
        }

        [ReactMethod]
        public void handleSetJSResponder(int viewTag, bool blockNativeResponder)
        {
        }

        [ReactMethod]
        public void handleClearJSResponder()
        {
        }

        public override void OnReactInstanceDispose()
        {
        }

        class CompositeUIBlock : IUIBlock
        {
            private readonly IUIBlock[] _blocks;

            public CompositeUIBlock(params IUIBlock[] blocks)
            {
                _blocks = blocks;
            }

            public void Execute(NativeViewHierarchyManager nativeViewHierarchyManager)
            {
                foreach (var block in _blocks)
                {
                    block.Execute(nativeViewHierarchyManager);
                }
            }
        }

        class UpdateGestureRecognizerUIBlock : IUIBlock
        {
            private readonly GestureHandler _gestureHandler;
            private readonly JObject _config;

            public UpdateGestureRecognizerUIBlock(GestureHandler gestureHandler, JObject config)
            {
                _gestureHandler = gestureHandler;
                _config = config;
            }

            public void Execute(NativeViewHierarchyManager nativeViewHierarchyManager)
            {
                _gestureHandler.Configure(_config);
            }
        }

        class AddPointerEventsUIBlock : IUIBlock
        {
            private readonly HashSet<uint> _activePointers = new HashSet<uint>();
            private readonly AnonymousUIBlock _removeUiBlock = new AnonymousUIBlock();

            private readonly int _viewTag;
            private readonly IList<GestureHandler> _gestureHandlers;
            private readonly object _gate;

            public AddPointerEventsUIBlock(int viewTag, IList<GestureHandler> gestureHandlers, object gate)
            {
                _viewTag = viewTag;
                _gestureHandlers = gestureHandlers;
                _gate = gate;
            }

            public IUIBlock RemovePointerEventsUIBlock
            {
                get
                {
                    return _removeUiBlock;
                }
            }

            public void Execute(NativeViewHierarchyManager nativeViewHierarchyManager)
            {
                // Get the view instance to attach the pointer events
                var view = nativeViewHierarchyManager.ResolveView(_viewTag) as UIElement;
                if (view == null)
                {
                    throw new InvalidOperationException("Cannot attach gesture handler to view.");
                }

                PointerEventHandler onPointerPressed = (sender, e) =>
                {
                    if (view.CapturePointer(e.Pointer))
                    {
                        _activePointers.Add(e.Pointer.PointerId);

                        // Get the root view to serve as the PointerPoint container
                        var rootView = RootViewHelper.GetRootView(view) as UIElement;
                        if (rootView == null)
                        {
                            throw new InvalidOperationException("Cannot find root view for gesture handler.");
                        }

                        lock (_gate)
                        {
                            foreach (var handler in _gestureHandlers)
                            {
                                handler.OnPointerPressed(e.GetCurrentPoint(rootView));
                            }
                        }
                    }
                };

                PointerEventHandler onPointerMoved = (sender, e) =>
                {
                    if (_activePointers.Contains(e.Pointer.PointerId))
                    {
                        // Get the root view to serve as the PointerPoint container
                        var rootView = RootViewHelper.GetRootView(view) as UIElement;
                        if (rootView == null)
                        {
                            throw new InvalidOperationException("Cannot find root view for gesture handler.");
                        }

                        lock (_gate)
                        {
                            foreach (var handler in _gestureHandlers)
                            {
                                handler.OnPointerMoved(e.GetIntermediatePoints(rootView));
                            }
                        }
                    }
                };

                PointerEventHandler onPointerReleased = (sender, e) =>
                {
                    if (_activePointers.Remove(e.Pointer.PointerId))
                    {
                        // Get the root view to serve as the PointerPoint container
                        var rootView = RootViewHelper.GetRootView(view) as UIElement;
                        if (rootView == null)
                        {
                            throw new InvalidOperationException("Cannot find root view for gesture handler.");
                        }

                        lock (_gate)
                        {
                            foreach (var handler in _gestureHandlers)
                            {
                                handler.OnPointerReleased(e.GetCurrentPoint(rootView));
                            }
                        }
                    }
                };

                PointerEventHandler onPointerCaptureLost = (sender, e) =>
                {
                    if (_activePointers.Remove(e.Pointer.PointerId))
                    {
                        // Get the root view to serve as the PointerPoint container
                        var rootView = RootViewHelper.GetRootView(view) as UIElement;
                        if (rootView == null)
                        {
                            throw new InvalidOperationException("Cannot find root view for gesture handler.");
                        }

                        lock (_gate)
                        {
                            foreach (var handler in _gestureHandlers)
                            {
                                handler.OnPointerCaptureLost();
                            }
                        }
                    }
                };

                view.PointerPressed += onPointerPressed;
                view.PointerMoved += onPointerMoved;
                view.PointerReleased += onPointerReleased;
                view.PointerCaptureLost += onPointerCaptureLost;

                // TODO: eliminate race condition
                _removeUiBlock.ExecuteHandler = _ =>
                {
                    view.PointerPressed -= onPointerPressed;
                    view.PointerMoved -= onPointerMoved;
                    view.PointerReleased -= onPointerReleased;
                    view.PointerCaptureLost -= onPointerCaptureLost;
                };
            }

            class AnonymousUIBlock : IUIBlock
            {
                public Action<NativeViewHierarchyManager> ExecuteHandler
                {
                    get;
                    set;
                }

                public void Execute(NativeViewHierarchyManager nativeViewHierarchyManager)
                {
                    ExecuteHandler?.Invoke(nativeViewHierarchyManager);
                }
            }
        }
    }
}
