using Periscope.Debuggee;
using System;

namespace _demoVisualizer {
    [Serializable]
    public class Config : ConfigBase<Config> {
        public int WindowNumber { get; set; }
        public override Config Clone() => new Config { WindowNumber = WindowNumber };
        public override bool NeedsTransferData(Config original) => WindowNumber != original.WindowNumber;
    }
}
