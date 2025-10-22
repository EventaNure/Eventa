namespace Eventa.Server.RequestModels
{
    public class LoadImageRequestModel
    {
        public IFormFile ImageFile { get; set; } = default!;
    }
}
