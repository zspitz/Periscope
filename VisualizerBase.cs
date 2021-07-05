using Microsoft.VisualStudio.DebuggerVisualizers;
using Periscope.Debuggee;
using System.Diagnostics;
using ZSpitz.Util.Wpf;

namespace Periscope {
    public class VisualizerBase<TWindow, TConfig> : Visualizer
            where TWindow : VisualizerWindowBase<TWindow, TConfig>, new()
            where TConfig : ConfigBase<TConfig> {

        public VisualizerBase(IProjectInfo? projectInfo = null, string? description = null) :
            base(projectInfo, description) { }

        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider) {
            PresentationTraceSources.DataBindingSource.Listeners.Add(new DebugTraceListener());

            ConfigKey = objectProvider.GetObject() as string ?? "";

            var config = Persistence.Get<TConfig>(ConfigKey);

            var window = new TWindow();
            window.Initialize(objectProvider, config);
            if (window.IsOpen) {
                window.ShowDialog();
            }
        }
    }
}
