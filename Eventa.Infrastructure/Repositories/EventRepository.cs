using Eventa.Application.DTOs.Events;
using Eventa.Application.Repositories;
using Eventa.Domain;
using Microsoft.EntityFrameworkCore;

namespace Eventa.Infrastructure.Repositories
{
    public class EventRepository : Repository<Event>, IEventRepository
    {
        public EventRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<EventListItemDto>> GetEventsAsync(int pageNumber, int pageSize, List<int> tagIds)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(e => e.EventTags
                    .Count(et => tagIds.Contains(et.TagId)) == tagIds.Count())
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EventListItemDto
                {
                    Id = e.Id,
                    Price = 450,
                    Title = e.Title,
                    Address = e.Place.Address,
                    FirstDateTime = e.EventDateTimes
                        .OrderBy(edt => edt.StartDateTime)
                        .Select(edt => edt.StartDateTime)
                        .First(),
                    LastDateTime = e.EventDateTimes
                        .OrderByDescending(edt => edt.StartDateTime)
                        .Select(edt => edt.StartDateTime)
                        .First(),
                })
                .ToListAsync();
        }

        public async Task<List<EventListItemDto>> GetEventsByOrganizerAsync(int pageNumber, int pageSize, string organizerId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(e => e.OrganizerId == organizerId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EventListItemDto
                {
                    Id = e.Id,
                    Price = 450,
                    Title = e.Title,
                    Address = e.Place.Address,
                    FirstDateTime = e.EventDateTimes
                        .OrderBy(edt => edt.StartDateTime)
                        .Select(edt => edt.StartDateTime)
                        .First(),
                    LastDateTime = e.EventDateTimes
                        .OrderByDescending(edt => edt.StartDateTime)
                        .Select(edt => edt.StartDateTime)
                        .First(),
                })
                .ToListAsync();
        }

        public async Task<Event?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Where(e => e.Id == id)
                .Include(e => e.EventTags)
                .Include(e => e.EventDateTimes)
                .Include(e => e.Place)
                .FirstOrDefaultAsync();
        }
    }
}
