using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics.Eventing.Reader;

namespace Mango.Services.AuthAPI.Service
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;

        // using Helper classes/mehods available in Identity to regiter and save data to ASPNetRoles
        private readonly  UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManger;

        public AuthService(AppDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManger)
        {
            _db = db;
            _userManager = userManager;
            _roleManger = roleManger;
        }

        public async Task<string> Register(RegistrationRequestDto registrationRequestDto)
        {
            ApplicationUser applicationUser = new ApplicationUser()
            {
                UserName = registrationRequestDto.Email,
                Email = registrationRequestDto.Email,
                NormalizedEmail = registrationRequestDto.Email.ToUpper(),
                Name = registrationRequestDto.Name,
                PhoneNumber = registrationRequestDto.PhoneNumber
            };

            try
            {
                // using helper method in user manager to create application user
                //Note : while creating the use everything like hashing the password and all is done by dotnet identity
                var result =  await _userManager.CreateAsync(applicationUser,registrationRequestDto.Password);
                if (result.Succeeded)
                {
                    var userToReturn = _db.ApplicationUsers.First(x => x.UserName == registrationRequestDto.Email);

                    UserDto user = new UserDto()
                    {
                        ID = userToReturn.Id,
                        Name = userToReturn.Name,
                        PhoneNumber = userToReturn.PhoneNumber,
                        Email = userToReturn.Email
                    };

                    // If user got registered successfully returning emty string
                    return "";
                }
                else
                {
                    // returning Identity error
                    return result.Errors.FirstOrDefault().Description;
                }



            }
            catch (Exception ex) { }

            // If it catches exception we will return below

            return "Error encountered";


        }
        public Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            throw new NotImplementedException();
        }

    }
}
