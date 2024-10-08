﻿using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;

namespace Mango.Web.Service
{
    public class AuthService : IAuthService
    {
        private readonly IBaseService _baseService;

        public AuthService(IBaseService baseServiceService)
        {
            _baseService = baseServiceService;
        }
        public async Task<ResponseDto?> RegisterAsync(RegistrationRequestDto registrationRequestDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticDetails.ApiType.POST,
                Data = registrationRequestDto,
                Url = StaticDetails.AuthAPIBase + "/api/auth/register"
            },withBearer:false);
        }
        public async Task<ResponseDto?> AssignRoleAsync(RegistrationRequestDto registrationRequestDto)
        {
            return await _baseService.SendAsync( new RequestDto()
            {
                ApiType = StaticDetails.ApiType.POST,
                Data = registrationRequestDto,
                Url = StaticDetails.AuthAPIBase + "/api/auth/AssignRole"
            });
        }

        public async Task<ResponseDto?> LoginAsync(LoginRequestDto loginRequestDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = StaticDetails.ApiType.POST,
                Data = loginRequestDto,
                Url = StaticDetails.AuthAPIBase + "/api/auth/login"
            },withBearer:false);
        }
    }
}
