using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.ComponentModel;
using ZSpitz.Util;
using ZSpitz.Util.Wpf;
using System.Windows;
using System.Diagnostics;
using Periscope.Debuggee;
using static System.IO.Path;

namespace Periscope {
    public class Visualizer : INotifyPropertyChanged {
        public static RelayCommand CopyWatchExpression = new RelayCommand(parameter => {
            if (!(parameter is string formatString)) { throw new ArgumentException("'parameter' is not a string."); }

            var rootExpression = Current?.GetRootExpression();
            if (rootExpression.IsNullOrWhitespace()) { return; }

            Clipboard.SetText(string.Format(formatString, rootExpression));
        });

        public static Visualizer? Current;

        static Visualizer() => Current = new Visualizer();

        private string? rootExpression;

        public string? RootExpression {
            get => rootExpression;
            set => this.NotifyChanged(ref rootExpression, value, PropertyChanged);
        }

        public string? GetRootExpression() {
            if (rootExpression.IsNullOrWhitespace()) {
                new ExpressionRootPrompt().ShowDialog();
            }
            return rootExpression;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Version { get; } 
        public string Location { get; }
        public string Filename { get; }

        public Visualizer() {
            var asm = GetType().Assembly;
            Version = asm.GetName().Version.ToString();
            Location = asm.Location;
            Filename = GetFileName(Location);
        }
    }

    public abstract class VisualizerBase<TWindow, TConfig> : DialogDebuggerVisualizer 
            where TWindow : VisualizerWindowBase<TWindow, TConfig>, new()
            where TConfig : ConfigBase<TConfig>, new() {

        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider) {
            if (windowService == null) { throw new ArgumentNullException(nameof(windowService)); }

            if (BindingErrorsAsException) {
                PresentationTraceSources.DataBindingSource.Listeners.Add(new DebugTraceListener());
            }

            var window = new TWindow();
            window.Initialize(objectProvider, GetInitialConfig());
            window.ShowDialog();
        }

        public abstract TConfig GetInitialConfig();

        public bool BindingErrorsAsException { get; set; } = true;
    }
}
