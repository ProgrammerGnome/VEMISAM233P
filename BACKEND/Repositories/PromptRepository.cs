using Microsoft.EntityFrameworkCore;
using ProjectName.Data;
using ProjectName.Models;
using System.Threading.Tasks;

namespace ProjectName.Repositories
{
    public interface IPromptRepository
    {
        Task<Prompt?> GetPromptByNameAsync(string name);
        Task UpdatePromptAsync(Prompt prompt);
        Task DeletePromptAsync(int id);
    }

    public class PromptRepository : IPromptRepository
    {
        private readonly AppDbContext _context;
        public PromptRepository(AppDbContext context) => _context = context;

        public Task<Prompt?> GetPromptByNameAsync(string name)
        {
            return _context.PromptSablonok.FirstOrDefaultAsync(p => p.SablonNev == name);
        }

        public async Task UpdatePromptAsync(Prompt prompt)
        {
            if (prompt.Id == 0)
            {
                _context.PromptSablonok.Add(prompt);
            }
            else
            {
                _context.PromptSablonok.Update(prompt);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeletePromptAsync(int id)
        {
            var prompt = await _context.PromptSablonok.FindAsync(id);
            if (prompt != null)
            {
                _context.PromptSablonok.Remove(prompt);
                await _context.SaveChangesAsync();
            }
        }

    }
}