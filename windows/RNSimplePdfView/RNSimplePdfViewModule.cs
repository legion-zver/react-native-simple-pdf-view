using ReactNative.Bridge;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Simple.Pdf.View.RNSimplePdfView
{
    /// <summary>
    /// A module that allows JS to share data.
    /// </summary>
    class RNSimplePdfViewModule : NativeModuleBase
    {
        /// <summary>
        /// Instantiates the <see cref="RNSimplePdfViewModule"/>.
        /// </summary>
        internal RNSimplePdfViewModule()
        {

        }

        /// <summary>
        /// The name of the native module.
        /// </summary>
        public override string Name
        {
            get
            {
                return "RNSimplePdfView";
            }
        }
    }
}
