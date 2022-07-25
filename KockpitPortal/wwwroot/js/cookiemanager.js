function expireCookie() {
    //document.cookie = "jwt_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;domain=localhost;";
    //document.cookie = "kid=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;domain=localhost;";

    document.cookie = "jwt_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;domain=.kockpit.in;";
    document.cookie = "kid=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;domain=.kockpit.in;";
}