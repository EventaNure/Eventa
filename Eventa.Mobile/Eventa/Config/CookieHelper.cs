using System.Runtime.InteropServices.JavaScript;

namespace Eventa.Config;

public partial class CookieHelper
{
    [JSImport("globalThis.getCookie")]
    public static partial string GetCookie(string name);

    [JSImport("globalThis.setCookie")]
    public static partial void SetCookie(string name, string value, int days);

    [JSImport("globalThis.deleteCookie")]
    public static partial void DeleteCookie(string name);
}