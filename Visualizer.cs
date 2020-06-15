using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.ComponentModel;
using ZSpitz.Util;
using ZSpitz.Util.Wpf;
using System.Windows;
using System.Diagnostics;
using static System.IO.Path;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace Periscope {
    public abstract class Visualizer : DialogDebuggerVisualizer, INotifyPropertyChanged {
        public static RelayCommand CopyWatchExpression = new RelayCommand(parameter => {
            if (!(parameter is string formatString)) { throw new ArgumentException("'parameter' is not a string."); }

            var rootExpression = Current?.GetRootExpression();
            if (rootExpression.IsNullOrWhitespace()) { return; }

            Clipboard.SetText(string.Format(formatString, rootExpression));
        });

        public static Visualizer? Current { get; private set; }

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

        public Version Version { get; }
        public string Location { get; }
        public string Filename { get; }
        public string? Description { get; }


        private bool autoVersionCheck;
        public bool AutoVersionCheck {
            get => autoVersionCheck;
            set => NotifyChanged(ref autoVersionCheck, value);
        }

        private DateTime? versionCheckedOn;
        public DateTime? VersionCheckedOn {
            get => versionCheckedOn;
            private set => NotifyChanged(ref versionCheckedOn, value);
        }

        private Version? latestVersion;
        public Version? LatestVersion {
            get => latestVersion;
            private set => NotifyChanged(ref latestVersion, value);
        }

        private string? latestVersionString = null;
        [AllowNull]
        public string LatestVersionString {
            get => latestVersionString ?? latestVersion?.ToString() ?? "Unknown";
            private set => NotifyChanged(ref latestVersionString, value);
        }

        public bool HasProjectUrl => new[] { ProjectUrl, FeedbackUrl, ReleaseUrl }.Any(x => !x.IsNullOrWhitespace());
        public string? ProjectUrl { get; }
        public string? FeedbackUrl { get; }
        public string? ReleaseUrl { get; }

        public RelayCommand? LatestVersionCheck { get; }

        public (string url, string args) UrlArgs => ("explorer.exe", $"/n /e,/select,\"{Location}\"");
        public static string ConfigKey { get; set; } = "";

        public Visualizer(IProjectInfo? projectInfo, string? description = null) {
            Current = this;

            var t = GetType();
            var asm = t.Assembly;
            Version = asm.GetName().Version;
            Location = asm.Location;
            Filename = GetFileName(asm.Location);

            Description =
                description ??
                asm.GetAttributes<DebuggerVisualizerAttribute>(false)
                    .Select(x => x.Description)
                    .Distinct()
                    .Single();

            Persistence.SetFolder(Description);

            if (projectInfo is { }) {
                string? fixUrl(string? value) => value.IsNullOrWhitespace() ? null : value;

                ProjectUrl = fixUrl(projectInfo.ProjectUrl);
                FeedbackUrl = fixUrl(projectInfo.FeedbackUrl);
                ReleaseUrl = fixUrl(projectInfo.ReleaseUrl);

                if (Persistence.GetVersionCheckInfo() is VersionCheckInfo versionCheckInfo) {
                    (AutoVersionCheck, VersionCheckedOn, LatestVersion) = versionCheckInfo;
                }

                LatestVersionCheck = new RelayCommand(async o => {
                    LatestVersionString = "Checking...";
                    LatestVersion = await projectInfo.GetLatestVersionAsync();
                    LatestVersionString = null;
                    VersionCheckedOn = DateTime.UtcNow;
                    NotifyNewVersion();
                }, o =>
                    VersionCheckedOn is null ||
                    DateTime.UtcNow - VersionCheckedOn >= TimeSpan.FromHours(1)
                );
                NotifyChanged(nameof(LatestVersionCheck));

                PropertyChanged += (s, e) => {
                    if (e.PropertyName.In(
                        nameof(AutoVersionCheck),
                        nameof(VersionCheckedOn),
                        nameof(LatestVersion)
                    )) {
                        Persistence.Write(new VersionCheckInfo {
                            AutoCheck = autoVersionCheck,
                            LastChecked = versionCheckedOn,
                            LastVersion = latestVersion
                        });
                    }
                };

                if (AutoVersionCheck) {
                    if (LatestVersionCheck.CanExecute("")) {
                        LatestVersionCheck.Execute("");
                    } else {
                        NotifyNewVersion();
                    }
                }
            }
        }

        private void NotifyNewVersion() {
            if (LatestVersion <= Version) { return; }
            var msg = $"There is a newer version available:\nCurrent: {Version}\nNewer: {LatestVersion}";
            if (ReleaseUrl.IsNullOrWhitespace()) {
                MessageBox.Show(msg);
                return;
            }
            msg += "\nDo you want to open the releases page?";
            var result = MessageBox.Show(msg, "", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) { return; }
            Commands.LaunchUrlOrFileCommand.Execute(ReleaseUrl);
        }
    }
}
