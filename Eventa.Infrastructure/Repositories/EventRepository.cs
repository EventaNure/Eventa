using Eventa.Application.DTOs.Comments;
using Eventa.Application.DTOs.Events;
using Eventa.Application.DTOs.Tags;
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

        public async Task<List<EventListItemDto>> GetEventsAsync(int pageNumber, int pageSize, IEnumerable<int> tagIds, DateOnly? startDate, DateOnly? endDate, string? subName)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(e => e.EventTags
                    .Count(et => tagIds.Contains(et.TagId)) == tagIds.Count() &&
                    (startDate == null || e.EventDateTimes.Any(edt => DateOnly.FromDateTime(edt.StartDateTime) >= startDate)) &&
                    (endDate == null || e.EventDateTimes.Any(edt => DateOnly.FromDateTime(edt.StartDateTime) <= endDate)) &&
                    (subName == null || e.Title.Contains(subName)) &&
                    e.EventStatus == EventStatus.Approved
                    )
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EventListItemDto
                {
                    Id = e.Id,
                    Price = e.Price,
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
                .Where(e => e.ApplicationUserId == organizerId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EventListItemDto
                {
                    Id = e.Id,
                    Price = e.Price,
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
                    TicketsSold = e.EventDateTimes.SelectMany(edt => edt.Orders.Where(o => o.IsPurcharsed).SelectMany(o => o.Tickets)).Count(),
                    EventStatus = e.EventStatus.ToString()
                })
                .ToListAsync();
        }

        public async Task<List<PendingEventListItem>> GetPendingEventsAsync(int pageNumber, int pageSize)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(e => e.EventStatus == EventStatus.Pending)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new PendingEventListItem
                {
                    Id = e.Id,
                    Title = e.Title,
                    OrganizerName = _dbContext.Users
                        .Where(u => u.Id == e.ApplicationUserId)
                        .Select(u => u.Name)
                        .First(),
                    OrganizationName = _dbContext.Users
                        .Where(u => u.Id == e.ApplicationUserId)
                        .Select(u => u.Organization)
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

        public async Task<EventDto?> GetEventAsync(int id)
        {
            return await _dbSet
                .Where(e => e.Id == id)
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    MinPrice = e.Price * e.Place.RowTypes.Min(rt => rt.Rows.Min(r => r.Seats.Min(s => s.PriceMultiplier))),
                    MaxPrice = e.Price * e.Place.RowTypes.Max(rt => rt.Rows.Max(r => r.Seats.Max(s => s.PriceMultiplier))),
                    Title = e.Title,
                    Description = e.Description,
                    Duration = e.Duration,
                    PlaceName = e.Place.Name,
                    PlaceAddress = e.Place.Address,
                    OrganizerName = _dbContext.Users
                        .Where(u => u.Id == e.ApplicationUserId)
                        .Select(u => u.Name)
                        .First(),
                    DateTimes = e.EventDateTimes.Select(e => new EventDateTimeDto
                    {
                        Id = e.Id,
                        DateTime = e.StartDateTime,
                    }),
                    Tags = e.EventTags.Select(et => new TagDto
                    {
                        Id = et.TagId,
                        Name = et.Tag.Name
                    }),
                    AverageRating = _dbContext.EventDateTimes.Where(edt => edt.Event.ApplicationUserId == e.ApplicationUserId)
                        .SelectMany(edt => edt.Orders
                            .Where(o => o.IsPurcharsed && o.Comment != null)
                            .Select(o => (double?)o.Comment!.Rating)
                        )
                        .Average() ?? 0,
                    Comments = e.EventDateTimes
                        .SelectMany(edt =>
                            edt.Orders
                            .Where(o => o.IsPurcharsed && o.Comment != null)
                            .Select(o => new CommentDto
                            {
                                Id = o.Comment!.Id,
                                UserName = _dbContext.Users
                                    .Where(u => u.Id == o.UserId)
                                    .Select(u => u.Name)
                                    .First(),
                                Rating = o.Comment.Rating,
                                Content = o.Comment.Content,
                                CreationDateTime = o.Comment.CreatedAt
                            })
                         )
                         .ToList()
                })
                .FirstOrDefaultAsync();
        }
    }
}
