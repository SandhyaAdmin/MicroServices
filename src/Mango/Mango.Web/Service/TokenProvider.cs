using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Newtonsoft.Json.Linq;

namespace Mango.Web.Service
{
    public class TokenProvider : ITokenProvider
    {
        /* using cookies to store the tokens as we send tokens while requesting the apis, when workig with cookies need to use
         * IHttpContextAccessor, already configured that in program.cs
         */

        private readonly IHttpContextAccessor _contextAccessor;

        public TokenProvider(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        public void SetToken(string token)
        {
            _contextAccessor.HttpContext?.Response.Cookies.Append(StaticDetails.TokenCookie, token);
        }

        public string? GetToken()
        {
            string? token = null;
            bool? hasToken = _contextAccessor.HttpContext?.Request.Cookies.TryGetValue(StaticDetails.TokenCookie, out token);

            return hasToken is true ? token : null;
        }

        public void ClearToken()
        {
            _contextAccessor.HttpContext?.Response.Cookies.Delete(StaticDetails.TokenCookie);
        }

    }
}
