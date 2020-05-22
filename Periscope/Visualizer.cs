using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.ComponentModel;
using ZSpitz.Util;
using ZSpitz.Util.Wpf;
using System.Windows;
using System.Diagnostics;
using Periscope.Debuggee;
using static System.IO.Path;
using System.Linq;
using System.Runtime.CompilerServices;
using Octokit;

namespace Periscope {
    public class Visualizer : INotifyPropertyChanged {
        public static RelayCommand CopyWatchExpression = new RelayCommand(parameter => {
            if (!(parameter is string formatString)) { throw new ArgumentException("'parameter' is not a string."); }

            var rootExpression = Current?.GetRootExpression();
            if (rootExpression.IsNullOrWhitespace()) { return; }

            Clipboard.SetText(string.Format(formatString, rootExpression));
        });

        public static void Show<TWindow, TConfig>(Type referenceType, IVisualizerObjectProvider objectProvider, IProjectInfo? projectInfo = default)
                where TWindow : VisualizerWindowBase<TWindow, TConfig>, new()
                where TConfig : ConfigBase<TConfig> {

            Current.Initialize(referenceType, projectInfo);
            ConfigProvider.LoadConfigFolder(referenceType);

            PresentationTraceSources.DataBindingSource.Listeners.Add(new DebugTraceListener());

            ConfigKey = objectProvider.GetObject() as string ?? "";

            var window = new TWindow();
            window.Initialize(objectProvider, ConfigProvider.Get<TConfig>(ConfigKey));
            window.ShowDialog();
        }

        public static Visualizer Current;

        static Visualizer() => Current = new Visualizer();

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyChanged<T>(ref T current, T newValue, [CallerMemberName] string? name = null) => 
            this.NotifyChanged(ref current, newValue, PropertyChanged, name);
        private void NotifyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private string? rootExpression;
        public string? RootExpression {
            get => rootExpression;
            set => NotifyChanged(ref rootExpression, value);
        }

        public string? GetRootExpression() {
            if (rootExpression.IsNullOrWhitespace()) {
                new ExpressionRootPrompt().ShowDialog();
            }
            return rootExpression;
        }

        private Version? version;
        public Version? Version {
            get => version;
            private set {
                NotifyChanged(ref version, value);
                NotifyChanged(nameof(IsLatest));
                NotifyChanged(nameof(VersionString));
            }
        }

        public string VersionString => $"Version: {Version}" + (IsLatest == true ? " (latest)" : "");

        private Version? latestVersion;
        public Version? LatestVersion {
            get => latestVersion;
            set {
                NotifyChanged(ref latestVersion, value);
                NotifyChanged(nameof(IsLatest));
                NotifyChanged(nameof(VersionString));
            }
        }

        private string? location;
        public string? Location {
            get => location;
            private set => NotifyChanged(ref location, value);
        }
        private string? filename;
        public string? Filename {
            get => filename;
            private set => NotifyChanged(ref filename, value);
        }

        private string? feedbackUrl;
        public string? FeedbackUrl {
            get => feedbackUrl;
            set {
                if (value.IsNullOrWhitespace()) { value = null; }
                NotifyChanged(ref feedbackUrl, value);
            }
        }

        private string? releaseUrl;
        public string? ReleaseUrl {
            get => releaseUrl;
            set {
                if (value.IsNullOrWhitespace()) { value = null; }
                NotifyChanged(ref releaseUrl, value);
            }
        }

        public RelayCommand? LatestVersionCheck { get; private set; }

        public bool? IsLatest => 
            Version is null || LatestVersion is null ?
                (bool?)null :
                Version == LatestVersion;

        public (string url, string args) UrlArgs => ("explorer.exe", $"/n /e,/select,\"{location}\"");
        public static string ConfigKey { get; set; } = "";

        public void Initialize(Type t, IProjectInfo? projectInfo) {
            // This requires an externally passed type, otherwise it'll return the Periscope DLL info
            var asm = t.Assembly;
            Version = asm.GetName().Version;
            Location = asm.Location;
            Filename = GetFileName(asm.Location);

            if (projectInfo is { }) {
                FeedbackUrl = projectInfo.FeedbackUrl;
                ReleaseUrl = projectInfo.ReleaseUrl;
                LatestVersionCheck = new RelayCommand(async o =>
                    LatestVersion = await projectInfo.GetLatestVersionAsync()
                );
                NotifyChanged(nameof(LatestVersionCheck));
            }
        }
    }
}
