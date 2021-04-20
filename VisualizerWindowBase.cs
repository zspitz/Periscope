using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using ZSpitz.Util.Wpf;
using Periscope.Debuggee;
using static Periscope.Debuggee.ConfigDiffStates;
using System.IO;

namespace Periscope {
    [ContentProperty("MainContent")]
    public abstract class VisualizerWindowBase<TWindow, TConfig> : Window 
            where TWindow : VisualizerWindowBase<TWindow, TConfig>, new()
            where TConfig : ConfigBase<TConfig> {

        private readonly VisualizerWindowChrome chrome = new VisualizerWindowChrome();

        private IVisualizerObjectProvider? objectProvider;
        private TConfig? config;
        private object? response;

        public void Initialize(IVisualizerObjectProvider objectProvider, TConfig config) => Initialize(objectProvider, config, NoAction);

        private void Initialize(IVisualizerObjectProvider objectProvider, TConfig config, ConfigDiffStates configDiffState) {
            if (this.objectProvider != objectProvider) {
                this.objectProvider = objectProvider;
                configDiffState = NeedsTransfer;
            }
            if (this.config != config) {
                this.config = config;
                configDiffState = NeedsTransfer;
            }
            if (configDiffState == NoAction || config is null || objectProvider is null) { return; }

            if (configDiffState == NeedsTransfer) {
                response = objectProvider.TransferObject(config);
                if (response is null) {
                    throw new InvalidDataException("Null received from debuggee-side.");
                } else if (response is ExceptionData exceptionData) {
                    MessageBox.Show(exceptionData.ToString(), "Debuggee-side exception");
                    Close();
                    return;
                }
            } else if (response is null) {
                // TODO not sure how we'd get here
                return;
            }

            object windowContext;
            object optionsContext;
            (windowContext, optionsContext, this.config) = GetViewState(response, openInNewWindow);

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
                if (newWindow.IsOpen) {
                    newWindow.ShowDialog();
                }
            }

            public event EventHandler? CanExecuteChanged;
        }

        private readonly ICommand openInNewWindow;

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

            Closed += (s,e) => isOpen = false;

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
                    Initialize(objectProvider, config, configState);
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

        private bool isOpen = true;
        public bool IsOpen => isOpen;

    }
}
