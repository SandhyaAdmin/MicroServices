﻿namespace Mango.Services.AuthAPI.Models.Dto
{
    // When the user gets logged in successfully with the LoginRequestDto, we return the UserDto with the JWT Token
    public class LoginResponse
    {
        public UserDto User { get; set; }

        public string Token { get; set; }   
    }
}
