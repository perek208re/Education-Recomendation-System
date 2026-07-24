using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERS.Core.Interfaces
{
    public interface IInteractionRepository
    {
        Task AddOrUpdateInteractionAsync(string userId, string courseId, double rating);
        Task<Dictionary<string, Dictionary<string, double>>> GetAllInteractionsAsync();
        Task<Dictionary<string, double>> GetUserInteractionsAsync(string userId);
    }
}
