using Eventa.Application.DTOs.Tags;
using Eventa.Application.Repositories;

namespace Eventa.Application.Services.Tags
{
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TagService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<TagDto>> GetMainTagsAsync()
        {
            var eventRepository = _unitOfWork.GetTagRepository();
            return await eventRepository.GetMainTagsAsync();
        }

        public async Task<IEnumerable<TagDto>> GetTagsAsync()
        {
            var eventRepository = _unitOfWork.GetTagRepository();
            return await eventRepository.GetTagsAsync();
        }
    }
}
