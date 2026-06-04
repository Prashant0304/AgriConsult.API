using AgriConsult.API.Models;

namespace AgriConsult.API.Services.Interfaces
{
    public interface IExpertreader
    {
        Task<List<Expert>> GetAllAsync();
        Task<Expert?> GetByIdAsync(int id);
    }

    public interface IExpertWriter
    {
        Task<Expert> CreateAsync(Expert expert);
        Task DeleteAsync(Expert expert);
    }

    public interface IExpertService : IExpertreader,IExpertWriter
    {
        
    }

}
