using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Periscope.Debuggee {
    [Flags]
    public enum ConfigDiffStates {
        NeedsWrite,
        NeedsTransfer
    }
}
