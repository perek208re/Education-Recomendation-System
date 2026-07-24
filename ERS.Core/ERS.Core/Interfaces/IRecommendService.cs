using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERS.Core.Interfaces
{
    public interface IRecommendService
    {
        Task<List<string>?> GetContentBasedRecAsync(string userId, int count = 5);
        Task<List<string>?> GetCollaborativeRecAsync(string userId, int k = 5);
        Task WarmUpAsync();
    }
}
