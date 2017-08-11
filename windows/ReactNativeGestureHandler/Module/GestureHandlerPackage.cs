using System;
using System.Collections.Generic;
using ReactNative.Bridge;
using ReactNative.Modules.Core;
using ReactNative.UIManager;
using Windows.UI.Xaml;

namespace ReactNativeGestureHandler.Module
{
    public class GestureHandlerPackage : IReactPackage
    {
        public IReadOnlyList<Type> CreateJavaScriptModulesConfig()
        {
            return new List<Type>(0);
        }

        public IReadOnlyList<INativeModule> CreateNativeModules(ReactContext reactContext)
        {
            return new List<INativeModule>
            {
                new GestureHandlerModule(reactContext),
            };
        }

        public IReadOnlyList<IViewManager> CreateViewManagers(ReactContext reactContext)
        {
            return new List<IViewManager>
            {
                new DummyViewManager(),
            };
        }

        /// <summary>
        /// This is an empty implementation of <see cref="IViewManager"/>. It is used only to export direct
        /// event configuration through <see cref="UIManagerModule"/>.
        /// </summary>
        private class DummyViewManager : ViewManager<FrameworkElement, ReactShadowNode>
        {
            public override string Name
            {
                get
                {
                    return "GestureHandlerDummyView";
                }
            }

            public override IReadOnlyDictionary<string, object> ExportedCustomDirectEventTypeConstants
            {
                get
                {
                    return new Dictionary<string, object>
                    {
                        {
                            GestureHandlerEvent.Name,
                            new Dictionary<string, object>
                            {
                                { "registrationName", GestureHandlerEvent.Name },
                            }
                        },
                        {
                            GestureHandlerStateChangeEvent.Name,
                            new Dictionary<string, object>
                            {
                                { "registrationName", GestureHandlerStateChangeEvent.Name },
                            }
                        },
                    };
                }
            }

            public override ReactShadowNode CreateShadowNodeInstance()
            {
                return null;
            }

            public override void UpdateExtraData(FrameworkElement root, object extraData)
            {
            }

            protected override FrameworkElement CreateViewInstance(ThemedReactContext reactContext)
            {
                return null;
            }
        }
    }
}
