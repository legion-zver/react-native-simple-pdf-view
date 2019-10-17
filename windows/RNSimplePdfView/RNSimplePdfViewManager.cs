using Newtonsoft.Json.Linq;
using ReactNative.UIManager;
using ReactNative.UIManager.Annotations;
using Windows.UI.Xaml.Media;

namespace RNSimplePdfView
{
    class RNSimplePdfViewManager : SimpleViewManager<AGSimplePdfView>
    {
        private const int CommandScrollTo = 1;

        public override string Name
        {
            get
            {
                return "RNSimplePdfView";
            }
        }

        public override JObject ViewCommandsMap
        {
            get
            {
                return new JObject
                {
                    { "scrollTo", CommandScrollTo },
                };
            }
        }

        protected override AGSimplePdfView CreateViewInstance(ThemedReactContext reactContext)
        {
            return new AGSimplePdfView();
        }

        [ReactProp(ViewProps.BackgroundColor, CustomType = "Color")]
        public void SetBackgroundColor(AGSimplePdfView view, uint? color)
        {
            view.Background = color.HasValue
                ? new SolidColorBrush(ColorHelpers.Parse(color.Value))
                : null;
        }

        [ReactProp("source")]
        public void SetZoomScale(AGSimplePdfView view, string source)
        {
            view.Load(source);
        }

    }
}
