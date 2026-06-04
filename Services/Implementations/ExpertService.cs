using AgriConsult.API.Data;
using AgriConsult.API.Models;
using AgriConsult.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgriConsult.API.Services.Implementations
{
    public class ExpertService : IExpertService
    {
        private readonly AppDbContext _db;

        public ExpertService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Expert>> GetAllAsync()
            => await _db.Experts.ToListAsync();

        public async Task<Expert?> GetByIdAsync(int id)
            => await _db.Experts.FindAsync(id);

        public async Task<Expert> CreateAsync(Expert expert)
        {
            _db.Experts.Add(expert);
            await _db.SaveChangesAsync();
            return expert;
        }

        public async Task DeleteAsync(Expert expert)
        {
            _db.Experts.Remove(expert);
            await _db.SaveChangesAsync();
        }
    }
}