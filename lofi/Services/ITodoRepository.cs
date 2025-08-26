using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lofi.Services
{
    public interface ITodoRepository
    {
        Task InitializeAsync();
        Task<List<(int Id, string Content, bool IsDone)>> GetAllAsync(); // is_deleted=0만
        Task<int> AddAsync(string content);
        Task SetDoneAsync(int id, bool isDone);
        Task SoftDeleteAsync(int id);
    }
}
