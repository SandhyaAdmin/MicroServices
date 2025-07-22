using Mango.Web.Models;
using Mango.Web.Service.IService;
using Newtonsoft.Json;
using System.Text;
using static Mango.Web.Utility.StaticDetails;
using System.Net;

namespace Mango.Web.Service
{
    public class BaseService : IBaseService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ITokenProvider _tokenProvider;
        public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider)
        {
            _httpClientFactory = httpClientFactory;
            _tokenProvider = tokenProvider;
        }
        public async Task<ResponseDto?> SendAsync(RequestDto requestDto, bool withBearer = true)
       { 
            HttpClient client = _httpClientFactory.CreateClient("MangoAPI");

            HttpRequestMessage message = new();

            if (requestDto.ContentType == ContentType.MultipartFormData) 
            {
                message.Headers.Add("Accept", "*/*");
            }
            else
            {
                message.Headers.Add("Accept", "application/json");
            }

            //message.Headers.Add("Content-Type", "application/json");
            message.RequestUri = new Uri(requestDto.Url);


            //token

            if (withBearer)
            {
                var token = _tokenProvider.GetToken();
                message.Headers.Add("Authorization", $"Bearer {token}");
            }

            //append the content if, it is MultipartFormData
            if (requestDto.ContentType == ContentType.MultipartFormData)
            {
                var content = new MultipartFormDataContent();

                // we need to read the file and add that as new stream content
                foreach (var prop in requestDto.Data.GetType().GetProperties())
                { 
                    var value = prop.GetValue(requestDto.Data);
                    // Now, we only want to upload a file if that is a form file
                    if (value is FormFile) 
                    { 
                        var file = (FormFile)value;
                        // add the file to the content if its not null
                        if (file != null) 
                        {
                            content.Add(new StreamContent(file.OpenReadStream()),prop.Name,file.FileName);
                        }
                    }
                    else
                    {
                        content.Add(new StringContent(value == null ? "" : value.ToString()), prop.Name);

                    }
                }
                message.Content = content;
            }
            else
            {
                if (requestDto != null)
                {
                    message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8, "application/json");
                }
            }

            HttpResponseMessage? apiResponse = null;
            switch (requestDto.ApiType)
            {
                case ApiType.POST:
                    message.Method = HttpMethod.Post;
                    break;
                case ApiType.PUT:
                    message.Method = HttpMethod.Put;
                    break;
                case ApiType.DELETE:
                    message.Method = HttpMethod.Delete;
                    break;
                default: 
                    message.Method = HttpMethod.Get;
                    break;
            }
;
            apiResponse = await client.SendAsync(message);
            try
            {
                switch (apiResponse.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return new() { IsSuccess = false, Message = "Not Found" };
                    case HttpStatusCode.Forbidden:
                        return new() { IsSuccess = false, Message = "Access Denied" };
                    case HttpStatusCode.Unauthorized:
                        return new() { IsSuccess = false, Message = "Unauthorized" };
                    case HttpStatusCode.InternalServerError:
                        return new() { IsSuccess = false, Message = "Internal Server Error" };
                    default:
                        var apiContent = await apiResponse.Content.ReadAsStringAsync();
                        var apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
                        return apiResponseDto;

                }
            }
            catch (Exception ex) 
            { 
                var dto = new ResponseDto
                {
                    Message = ex.Message.ToString(),
                    IsSuccess = false

                };
                return dto; 
            }
       }
    }
}
