using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using ZSpitz.Util.Wpf;
using Periscope.Debuggee;
using static Periscope.Debuggee.ConfigDiffStates;

namespace Periscope {
    [ContentProperty("MainContent")]
    public abstract class VisualizerWindowBase<TWindow, TConfig> : Window 
            where TWindow : VisualizerWindowBase<TWindow, TConfig>, new()
            where TConfig : ConfigBase<TConfig> {

        private VisualizerWindowChrome chrome = new VisualizerWindowChrome();

        private IVisualizerObjectProvider? objectProvider;
        private TConfig? config;

        public void Initialize(IVisualizerObjectProvider objectProvider, TConfig config) => Initialize(objectProvider, config, false);

        private void Initialize(IVisualizerObjectProvider objectProvider, TConfig config, bool isConfigModified) {
            var modified = isConfigModified;
            if (this.objectProvider != objectProvider) {
                this.objectProvider = objectProvider;
                modified = true;
            }
            if (this.config != config) {
                this.config = config;
                modified = true;
            }
            if (!modified || config is null || objectProvider is null) { return; }

            if (config is null || objectProvider is null) { return; }
            var response = objectProvider.TransferObject(config);
            object model;
            switch (response) {
                case Response r:
                    if (r.ExceptionData is { }) {
                        MessageBox.Show(r.ExceptionData.ToString(), "Debuggee-side exception:");
                    }
                    if (r.Model is null) { return; }
                    model = r.Model;
                    break;
                case null:
                    throw new InvalidOperationException("Unspecified error while serializing/deserializing");
                default: // for the case when the specific project doesn't inherit VisualizerObjectSource
                    model = response;
                    break;
            }

            object windowContext;
            object optionsContext;
            (windowContext, optionsContext, this.config) = GetViewState(model, openInNewWindow);

            Persistence.Write(this.config, Visualizer.ConfigKey);

            chrome.optionsPopup.DataContext = optionsContext;
            DataContext = windowContext;
        }

        protected abstract (object windowContext, object optionsContext, TConfig config) GetViewState(object response, ICommand? OpenInNewWindow);
        protected abstract void TransformConfig(TConfig config, object parameter);

        private class _OpenInNewWindow : ICommand {
            private readonly TWindow window;
            public _OpenInNewWindow(TWindow window) => this.window = window;
            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) {
                if (window.objectProvider is null) { throw new ArgumentNullException("Missing object provider"); }
                if (window.config is null) { throw new ArgumentNullException("Missing config."); }

                var newConfig = window.config.Clone();
                window.TransformConfig(newConfig, parameter);
                var newWindow = new TWindow();
                newWindow.Initialize(window.objectProvider, newConfig);
                newWindow.ShowDialog();
            }

            public event EventHandler? CanExecuteChanged;
        }

        private ICommand openInNewWindow;

        public UIElement MainContent { 
            get => chrome.mainContent.Child;
            set => chrome.mainContent.Child = value; 
        }
        public UIElement OptionsPopup {
            get => chrome.optionsBorder.Child;
            set => chrome.optionsBorder.Child = value; 
        }

        public VisualizerWindowBase() {
            openInNewWindow = new _OpenInNewWindow((TWindow)this);

            SizeToContent = SizeToContent.WidthAndHeight;

            // if we could find out which is the current monitor, that would be better
            var workingAreas = Monitor.AllMonitors.Select(x => x.WorkingArea).ToList();
            MaxWidth = workingAreas.Min(x => x.Width) * .90;
            MaxHeight = workingAreas.Min(x => x.Height) * .90;

            PreviewKeyDown += (s, e) => {
                if (e.Key == Key.Escape) { Close(); }
            };

            // we need to set this explicitly, otherwise the popup inherits the data context from the parent's DataContext, which points to Periscope.Visualizer.Current
            chrome.optionsPopup.DataContext = null;
            Content = chrome;

            Loaded += (s, e) => {
                TConfig? _baseline = null;

                void popupOpenHandler(object s, EventArgs e) {
                    if (config is null) { throw new ArgumentNullException(nameof(config)); }
                    _baseline = config.Clone();
                }

                void popupClosedHandler(object s, EventArgs e) {
                    if (objectProvider is null) { throw new ArgumentNullException(nameof(objectProvider)); }
                    if (config is null) { throw new ArgumentNullException(nameof(config)); }
                    if (_baseline is null) { throw new ArgumentNullException(nameof(_baseline)); }

                    var configState = config.Diff(_baseline);
                    switch (configState) {
                        case NeedsTransfer:
                            Initialize(objectProvider, config, true);
                            break;
                        case NeedsWrite:
                            // only "else if", because Iniitialize writes the config on its own
                            Persistence.Write(config, Visualizer.ConfigKey);
                            break;
                    }
                    _baseline = null;
                }

                chrome.optionsPopup.Opened += popupOpenHandler;
                chrome.optionsPopup.Closed += popupClosedHandler;
                chrome.aboutPopup.Opened += popupOpenHandler;
                chrome.aboutPopup.Closed += popupClosedHandler;

                Unloaded += (s, e) => {
                    if (config is null) { return; }
                    Persistence.Write(config, Visualizer.ConfigKey);
                };
            };
        }
    }
}
