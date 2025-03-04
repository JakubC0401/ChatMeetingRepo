using ChatMeeting.Core.Domain.Dtos;
using ChatMeeting.Core.Domain.Interfaces.Repositories;
using ChatMeeting.Core.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatMeeting.Core.Application.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthService> _logger;
        public AuthService(IUserRepository userRepository, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task RegisterUser(RegisterUserDto registerUser)
        {
            try
            {
                var existingUser = _userRepository.GetUserByLogin(registerUser.UserName);

                if(existingUser != null)
                {
                    _logger.LogWarning($"User with login {registerUser.UserName} already exists.");
                    throw new InvalidOperationException("User with this login already exist");
                }

                var user = new User(registerUser.UserName, registerUser.Password);

                await _userRepository.AddUser(user);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, $"Error occured while registering user: {registerUser.UserName}");
                throw new InvalidProgramException();
            }
        }
    }
}
