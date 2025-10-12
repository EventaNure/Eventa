using Eventa.Application.DTOs.Tags;

namespace Eventa.Application.Services.Tags
{
    public interface ITagService
    {
        Task<IEnumerable<TagDto>> GetMainTagsAsync();
        Task<IEnumerable<TagDto>> GetTagsAsync();
    }
}