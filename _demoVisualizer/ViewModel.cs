using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ZSpitz.Util.Wpf;

namespace _demoVisualizer {
    public class ViewModel {
        public int WindowNumber { get; set; }
        public string FormatString => $"{{0}} + {WindowNumber}";
        public ICommand? OpenInNewWindow { get; set; }
        public ICommand? CopyWatchExpression { get; set; }
    }
}
