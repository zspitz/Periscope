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
using System.Diagnostics.CodeAnalysis;

namespace Periscope {
    [ContentProperty("MainContent")]
    public abstract class VisualizerWindowBase<TWindow, TConfig> : Window 
            where TWindow : VisualizerWindowBase<TWindow, TConfig>, new()
            where TConfig : ConfigBase<TConfig> {

        private readonly VisualizerWindowChrome chrome = new();

        private IVisualizerObjectProvider? objectProvider;
        private object? response;

        public void Initialize(TConfig config) => initialize(objectProvider, config, NoAction);
        public void Initialize(IVisualizerObjectProvider objectProvider, TConfig config) => initialize(objectProvider, config, NoAction);
        private void initialize(IVisualizerObjectProvider? objectProvider, TConfig config, ConfigDiffStates configDiffState) {
            if (this.objectProvider != objectProvider) {
                this.objectProvider = objectProvider;
                configDiffState = NeedsTransfer;
            }
            if (Config != config) {
                Config = config;
                configDiffState = NeedsTransfer;
            }
            if (configDiffState == NoAction || config is null || objectProvider is null) { return; }

            if (configDiffState == NeedsTransfer) {
                response = objectProvider.TransferObject(config);
                if (response is null) {
                    throw new InvalidDataException("Null received from debuggee-side.");
                } else if (response is ExceptionData exceptionData) {
                    MessageBox.Show(exceptionData.ToString(), "Debuggee-side exception.");
                    Close();
                    return;
                }
            } else if (response is null) {
                // TODO not sure how we'd get here
                return;
            }

            //configDiffState at this point could be either NeedsTransfer or NeedsWrite

            object windowContext;
            object optionsContext;
            (windowContext, optionsContext, Config) = GetViewState(response, openInNewWindow);

            Persistence.Write(Config, Visualizer.ConfigKey);

            chrome.optionsPopup.DataContext = optionsContext;
            DataContext = windowContext;
        }

        protected abstract (object windowContext, object optionsContext, TConfig config) GetViewState(object response, ICommand? OpenInNewWindow);
        protected abstract void TransformConfig(TConfig config, object parameter);

        [SuppressMessage("Style", "IDE1006:Naming Styles")]
        private class _OpenInNewWindow : ICommand {
            private readonly TWindow window;
            public _OpenInNewWindow(TWindow window) => this.window = window;
            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) {
                if (window.objectProvider is null) { throw new ArgumentNullException("Missing object provider"); }
                if (window.Config is null) { throw new ArgumentNullException("Missing config."); }

                var newConfig = window.Config.Clone();
                window.TransformConfig(newConfig, parameter);
                var newWindow = new TWindow();
                newWindow.Initialize(window.objectProvider, newConfig);
                if (newWindow.IsOpen) {
                    newWindow.ShowDialog();
                }
            }

#pragma warning disable CS0067
            public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067
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

            Closed += (s,e) => IsOpen = false;

            Loaded += (s, e) => {
                TConfig? _baseline = null;

                void popupOpenHandler(object s, EventArgs e) {
                    if (Config is null) { throw new ArgumentNullException(nameof(Config)); }
                    _baseline = Config.Clone();
                }

                void popupClosedHandler(object s, EventArgs e) {
                    if (objectProvider is null) { throw new ArgumentNullException(nameof(objectProvider)); }
                    if (Config is null) { throw new ArgumentNullException(nameof(Config)); }
                    if (_baseline is null) { throw new ArgumentNullException(nameof(_baseline)); }

                    var configState = Config.Diff(_baseline);
                    initialize(objectProvider, Config, configState);
                    _baseline = null;
                }

                chrome.optionsPopup.Opened += popupOpenHandler;
                chrome.optionsPopup.Closed += popupClosedHandler;
                chrome.aboutPopup.Opened += popupOpenHandler;
                chrome.aboutPopup.Closed += popupClosedHandler;

                Unloaded += (s, e) => {
                    if (Config is null) { return; }
                    Persistence.Write(Config, Visualizer.ConfigKey);
                };
            };
        }

        public bool IsOpen { get; private set; } = true;
        protected TConfig? Config { get; private set; }
    }
}
