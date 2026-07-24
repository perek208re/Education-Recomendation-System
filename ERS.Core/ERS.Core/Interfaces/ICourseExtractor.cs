using ERS.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERS.Core.Interfaces
{
    public interface ICourseExtractor
    {
        string ProviderName { get; }
        IAsyncEnumerable<Course> ExtractAsync(CancellationToken cancel_token = default);
    }
}
