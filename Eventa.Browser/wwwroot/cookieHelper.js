window.getCookie = function (name) {
    const nameEQ = name + "=";
    const ca = document.cookie.split(';');

    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) === ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) === 0) {
            const value = c.substring(nameEQ.length, c.length);
            return decodeURIComponent(value);
        }
    }
    return "";
};

window.setCookie = function (name, value, days) {
    let expires = "";
    if (days) {
        const date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    // Use max-age as fallback and ensure cookie is set immediately
    const maxAge = days ? `; max-age=${days * 24 * 60 * 60}` : "";
    document.cookie = name + "=" + encodeURIComponent(value) + expires + maxAge + "; path=/; SameSite=Lax";

    // Force a cookie write by accessing document.cookie
    const test = document.cookie;
};

window.deleteCookie = function (name) {
    document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
};