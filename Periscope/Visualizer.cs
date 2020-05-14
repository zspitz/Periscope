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

        public static Visualizer Current;

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

        private string? version;
        public string? Version {
            get => version;
            private set => this.NotifyChanged(ref version, value, PropertyChanged);
        }
        private string? location;
        public string? Location {
            get => location;
            private set => this.NotifyChanged(ref location, value, PropertyChanged);
        }
        private string? filename;
        public string? Filename {
            get => filename;
            private set => this.NotifyChanged(ref filename, value, PropertyChanged);
        }

        public static string ConfigKey { get; set; } = "";

        public void LoadVersionLocationInfo(Type t) {
            // This requires an externally passed type, otherwise it'll return the Periscope DLL info
            var asm = t.Assembly;
            Version = asm.GetName().Version.ToString();
            Location = asm.Location;
            Filename = GetFileName(asm.Location);
        }

        public static void Show<TWindow, TConfig>(Type referenceType, IVisualizerObjectProvider objectProvider)
                where TWindow : VisualizerWindowBase<TWindow, TConfig>, new()
                where TConfig : ConfigBase<TConfig> {

            Visualizer.Current.LoadVersionLocationInfo(referenceType);
            ConfigProvider.LoadConfigFolder(referenceType);

            PresentationTraceSources.DataBindingSource.Listeners.Add(new DebugTraceListener());

            Visualizer.ConfigKey = objectProvider.GetObject() as string ?? "";

            var window = new TWindow();
            window.Initialize(objectProvider, ConfigProvider.Get<TConfig>(ConfigKey));
            window.ShowDialog();
        }
    }

    //public abstract class VisualizerBase<TWindow, TConfig> : DialogDebuggerVisualizer 
    //        where TWindow : VisualizerWindowBase<TWindow, TConfig>, new()
    //        where TConfig : ConfigBase<TConfig> {

    //    protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider) {
    //        if (windowService == null) { throw new ArgumentNullException(nameof(windowService)); }

    //        if (BindingErrorsAsException) {
    //            PresentationTraceSources.DataBindingSource.Listeners.Add(new DebugTraceListener());
    //        }

    //        Visualizer.ConfigKey = objectProvider.GetObject() as string ?? "";

    //        var window = new TWindow();
    //        window.Initialize(objectProvider, ConfigProvider.Get<TConfig>(Visualizer.ConfigKey));
    //        window.ShowDialog();
    //    }

    //    public virtual string ConfigKey(object o) => "";

    //    public bool BindingErrorsAsException { get; set; } = true;

    //    public VisualizerBase() {
    //        var t = GetType();
    //        Visualizer.Current.LoadVersionLocationInfo(t);
    //        ConfigProvider.LoadConfigFolder(t);
    //    }
    //}
}
