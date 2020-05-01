using Periscope;
using System.Windows.Input;

namespace _demoVisualizer {
    /// <summary>
    /// Interaction logic for VisualizerWindow.xaml
    /// </summary>
    public partial class VisualizerWindow : VisualizerWindowBase {
        public VisualizerWindow() {
            InitializeComponent();
        }
        protected override (ViewState<Config> window, ViewState<Config> settings) GetViewStates(object response, ICommand? OpenInNewWindow) {
            var receivedConfig = (Config)response;
            var windowState = new ViewState<Config>(
                new ViewModel { WindowNumber = receivedConfig.WindowNumber, CopyWatchExpression = Periscope.Visualizer.CopyWatchExpression, OpenInNewWindow = OpenInNewWindow },
                receivedConfig
            );

            var settingsConfig = receivedConfig.Clone();
            return (
                windowState,
                new ViewState<Config>(settingsConfig, settingsConfig)
            );
        }

        protected override void TransformConfig(Config config, object parameter) => config.WindowNumber += 1;
    }
}
