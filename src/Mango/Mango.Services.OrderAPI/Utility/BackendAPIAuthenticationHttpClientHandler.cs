using Microsoft.AspNetCore.Authentication;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Mango.Services.OrderAPI.Utility
{
    public class BackendAPIAuthenticationHttpClientHandler : DelegatingHandler
    {
        /*DeligatingHandler : Deligating handlers are kind of simillar to the dotnet core middleware
        but thr main difference is that the delegating handlers are on client side, 
        lets say if you are making an http request using http client, we can leverage the delegate handle to pass
        out bearer token to the other request and that is exactly what we want here.
        */

        // bearer token we can retrive using HttpContext accessor
        private readonly IHttpContextAccessor _accessor;
        public BackendAPIAuthenticationHttpClientHandler(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }


        // We can use protected override SendAsync(),override it to access the token and we can add it in autherization header
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Geth the token using default authentication scheme "access_token"
            var token = await _accessor.HttpContext.GetTokenAsync("access_token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await base.SendAsync(request, cancellationToken);
        }




        //Note : Register this class in Program.cs and HttpContextaccessor

        /* Here we have added backendapi authentication handlers, that we are cretes, but when we are registering 
        the services, at that point we need to add http message handlers
        that will make sure that it will add the token, pass that for both product api and coupon api
        */


        // Below are the changes that needs to be in program.cs

        /*
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<BackendAPIAuthenticationHttpClientHandler>();

        builder.Services.AddHttpClient("Product", u => u.BaseAddress =
        new Uri(builder.Configuration["ServiceUrls:ProductAPI"])).AddHttpMessageHandler<BackendAPIAuthenticationHttpClientHandler>();

        builder.Services.AddHttpClient("Coupon", u => u.BaseAddress =
        new Uri(builder.Configuration["ServiceUrls:CouponAPI"])).AddHttpMessageHandler<BackendAPIAuthenticationHttpClientHandler>();
        */
    }
}
