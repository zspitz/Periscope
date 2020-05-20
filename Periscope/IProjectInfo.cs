using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Periscope {
    public interface IProjectInfo {
        public string FeedbackUrl { get; }
        public Task<Version> GetLatestVersionAsync(CancellationToken cancellationToken = default);
    }
}
