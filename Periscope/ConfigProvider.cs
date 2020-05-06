using Newtonsoft.Json.Linq;
using Periscope.Debuggee;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using ZSpitz.Util;
using static System.IO.Path;
using static System.Environment;

namespace Periscope {
    public static class ConfigProvider {
        public static string? ConfigFolder { get; private set; }
        public static void LoadConfigFolder(Type t) {
            var description = t.Assembly.GetAttributes<DebuggerVisualizerAttribute>(false).Select(x => x.Description).Distinct().Single();
            ConfigFolder = Combine(
                GetFolderPath(SpecialFolder.LocalApplicationData),
                description
            );
        }

        private static bool TryReadFile(string key, [NotNullWhen(true)] out JObject? data) {
            data = null;
            var path = Combine(
                ConfigFolder,
                $"{key}.json"
            );
            if (!File.Exists(path)) { return false; }

            string? fileText = null;
            try {
                fileText = File.ReadAllText(path);
            } catch { }
            if (fileText.IsNullOrWhitespace()) { return false; }

            data = JObject.Parse(fileText);
            return true;
        }

        // config data is stored in two files: global config (".json"), and keyed files
        // the global config stores globals (at the globals property), and the last used keyed config (at the lastKeyed property)
        // each visualizer can determine what should be the key
        // keyed config overrides last used keyed config

        public static TConfig Get<TConfig>(string key = "") where TConfig : ConfigBase<TConfig> {
            var ret = new JObject();

            if (Directory.Exists(ConfigFolder)) {
                if (TryReadFile("", out var globalConfig)) {
                    if (globalConfig.TryGetValue("globals", out var globals)) {
                        ret.Merge(globals);
                    }
                    if (globalConfig.TryGetValue("lastKeyed", out var lastKeyed)) {
                        ret.Merge(lastKeyed);
                    }
                }

                if (!key.IsNullOrWhitespace() && TryReadFile(key, out var keyedConfig)) {
                    ret.Merge(keyedConfig);
                }
            }

            return ret.ToObject<TConfig>()!;
        }
    }
}
