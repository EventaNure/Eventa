using AsyncImageLoader.Loaders;

namespace Eventa.Services;

public class AsyncImageLoader : DiskCachedWebImageLoader
{
    public static AsyncImageLoader Instance { get; } = new AsyncImageLoader();
}