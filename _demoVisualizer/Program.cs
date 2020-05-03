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
            int aNumber = 29;
            var visualizerHost = new VisualizerDevelopmentHost(aNumber, typeof(Visualizer), typeof(DebuggeeObjectSource));
            visualizerHost.ShowVisualizer();

        }
    }
}
