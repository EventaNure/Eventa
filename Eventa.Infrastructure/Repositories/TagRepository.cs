using Eventa.Application.DTOs.Tags;
using Eventa.Application.Repositories;
using Eventa.Domain;
using Microsoft.EntityFrameworkCore;

namespace Eventa.Infrastructure.Repositories
{
    public class TagRepository : Repository<Tag>, ITagRepository
    {
        public TagRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<TagDto>> GetMainTagsAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(t => t.IsMain)
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<TagDto>> GetTagsAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .OrderByDescending(t => t.IsMain)
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> GetByIdsAsync(IEnumerable<int> tagIds)
        {
            return await _dbSet
                .Where(t => tagIds.Contains(t.Id))
                .ToListAsync();
        }
    }
}
