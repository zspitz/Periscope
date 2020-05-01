using Periscope;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _demoVisualizer {
    public class Visualizer : VisualizerBase<VisualizerWindow, Config> {
        public override Config GetInitialConfig() => new Config { WindowNumber = 0 };
    }
}
