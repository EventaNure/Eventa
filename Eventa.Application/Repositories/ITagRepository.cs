using Eventa.Application.DTOs.Tags;

namespace Eventa.Application.Repositories
{
    public interface ITagRepository
    {
        Task<IEnumerable<TagDto>> GetMainTagsAsync();
        Task<IEnumerable<TagDto>> GetTagsAsync();
    }
}