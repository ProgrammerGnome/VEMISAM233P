using Microsoft.EntityFrameworkCore;
using ProjectName.Data;
using ProjectName.Models;
using System.Threading.Tasks;

namespace ProjectName.Repositories
{
    public interface IUploadedSolutionsRepository
    {
        Task AddSolutionAsync(FeltoltottMegoldas solution);
        Task<FeltoltottMegoldas?> GetSolutionWithZhAsync(int feltoltesId);
        Task UpdateSolutionAsync(FeltoltottMegoldas solution);
    }

    public class UploadedSolutionsRepository : IUploadedSolutionsRepository
    {
        private readonly AppDbContext _context;
        public UploadedSolutionsRepository(AppDbContext context) => _context = context;

        public async Task AddSolutionAsync(FeltoltottMegoldas solution)
        {
            // JAVÍTAS:
            var hallgatoExists = await _context.Hallgatok
                .AnyAsync(h => h.NeptunKod == solution.HallgatoId);
            if (!hallgatoExists)
            {
                throw new ArgumentException("A megadott Neptun-kódhoz nem tartozik létező hallgató.", nameof(solution.HallgatoId));
            }

            await _context.FeltoltottMegoldasok.AddAsync(solution);
            await _context.SaveChangesAsync();
        }
        
        public Task<FeltoltottMegoldas?> GetSolutionWithZhAsync(int feltoltesId)
        {
            return _context.FeltoltottMegoldasok
                .Include(s => s.Zh)
                .FirstOrDefaultAsync(s => s.FeltoltesId == feltoltesId);
        }

        public async Task UpdateSolutionAsync(FeltoltottMegoldas solution)
        {
            _context.FeltoltottMegoldasok.Update(solution);
            await _context.SaveChangesAsync();
        }
    }
}