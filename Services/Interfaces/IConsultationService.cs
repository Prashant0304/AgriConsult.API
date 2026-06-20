using AgriConsult.API.Models;

namespace AgriConsult.API.Services.Interfaces
{
    public interface IConsultationService
    {
        Task<Consultation> BookAsync(int farmerId, int expertId, string topic);
        Task<List<Consultation>> GetByFarmerAsync(int farmerId);
        Task<Consultation?> GetByIdAsync(int id);
        Task<bool> UpdateStatusAsync(int id, string status);
    }
}
