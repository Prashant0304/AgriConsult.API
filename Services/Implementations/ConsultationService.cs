using AgriConsult.API.Data;
using AgriConsult.API.Models;
using AgriConsult.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgriConsult.API.Services.Implementations
{
    public class ConsultationService : IConsultationService
    {
        private readonly AppDbContext _db;
        private readonly INotificationService _notification;

        // Two dependencies injected — DIP applied
        public ConsultationService(
            AppDbContext db, INotificationService notification)
        {
            _db = db;
            _notification = notification;
        }

        public async Task<Consultation> BookAsync(
            int farmerId, int expertId, string topic)
        {
            // Validate expert exists
            var expert = await _db.Experts.FindAsync(expertId);
            if (expert is null)
                throw new Exception("Expert not found");

            var consultation = new Consultation
            {
                FarmerId = farmerId,
                ExpertId = expertId,
                Topic = topic,
                Status = "Pending"
            };

            _db.Consultations.Add(consultation);
            await _db.SaveChangesAsync();

            
            await _notification.SendAsync(
                expert.Name,
                $"New consultation request: {topic}");

            return consultation;
        }

        public async Task<List<Consultation>> GetByFarmerAsync(int farmerId)
        {
            return await _db.Consultations
                .Where(c => c.FarmerId == farmerId)
                .ToListAsync();
        }

        public async Task<Consultation?> GetByIdAsync(int id)
        {
            return await _db.Consultations.FindAsync(id);
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var consultation = await _db.Consultations.FindAsync(id);
            if (consultation is null) return false;

            consultation.Status = status;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}