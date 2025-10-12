namespace Eventa.Server.RequestModels
{
    public class GetEventsByTagsRequestModel
    {
        public InfinityScrollRequestModel InfinityScrollRequestModel { get; set; } = new InfinityScrollRequestModel();

        public IEnumerable<int> TagIds { get; set; } = [];
    }
}
