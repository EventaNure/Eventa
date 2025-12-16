using System.Runtime.InteropServices.JavaScript;

namespace Eventa.Config;

public partial class GoogleAuthHelper
{
    [JSImport("globalThis.navigateToGoogleAuth")]
    public static partial void NavigateToGoogleAuth();
}