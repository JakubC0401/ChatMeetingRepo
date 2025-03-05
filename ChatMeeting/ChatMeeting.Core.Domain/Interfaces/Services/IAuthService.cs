using ChatMeeting.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatMeeting.Core.Domain.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthDto> GetToken(LoginDto loginModel);
        Task RegisterUser(RegisterUserDto registerUser);
    }
}
