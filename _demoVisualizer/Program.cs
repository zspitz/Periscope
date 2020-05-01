using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _demoVisualizer {
    class Program {
        [STAThread]
        static void Main(string[] args) {
            int i = 0;
            var visualizerHost = new VisualizerDevelopmentHost(i, typeof(Visualizer), typeof(DebuggeeObjectSource));
            visualizerHost.ShowVisualizer();

        }
    }
}
