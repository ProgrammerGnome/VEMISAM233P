using Microsoft.EntityFrameworkCore;
using ProjectName.Data;
using ProjectName.Models;
using System.Threading.Tasks;

namespace ProjectName.Repositories
{
    public interface IZhRepository
    {
        Task AddZhAsync(ZH zh);
        Task<ZH?> GetZhByIdAsync(int zhId);
    }

    public class ZhRepository : IZhRepository
    {
        private readonly AppDbContext _context;
        public ZhRepository(AppDbContext context) => _context = context;

        public async Task AddZhAsync(ZH zh)
        {
            await _context.ZH.AddAsync(zh);
            await _context.SaveChangesAsync();
        }
        
        public Task<ZH?> GetZhByIdAsync(int zhId)
        {
            return _context.ZH.FirstOrDefaultAsync(z => z.ZhId == zhId);
        }
    }
}