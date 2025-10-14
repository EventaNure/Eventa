using Eventa.Application.DTOs.Tags;
using Eventa.Domain;

namespace Eventa.Application.Repositories
{
    public interface ITagRepository
    {
        Task<IEnumerable<Tag>> GetByIdsAsync(IEnumerable<int> tagIds);
        Task<IEnumerable<TagDto>> GetMainTagsAsync();
        Task<IEnumerable<TagDto>> GetTagsAsync();
    }
}