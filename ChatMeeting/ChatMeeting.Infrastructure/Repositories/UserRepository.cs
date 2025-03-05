using ChatMeeting.Core.Domain;
using ChatMeeting.Core.Domain.Interfaces.Repositories;
using ChatMeeting.Core.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatMeeting.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ChatDbContext _context;
        private readonly ILogger<UserRepository> _logger;
        public UserRepository(ChatDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddUser(User user)
        {
            try
            {
                await _context.Users.AddAsync(user);

                await _context.SaveChangesAsync();

            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, $"Error occured when adding user: {user.Username}");
                throw;
            }
        }

        public async Task<User?> GetUserById(Guid id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    _logger.LogWarning($"User with id: {id} not found");
                }

                return user;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error occured GetUserById with id: {id}");
                throw;
            }
        }

        public async Task<User?> GetUserByLogin(string login)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == login);

                if (user == null)
                {
                    _logger.LogWarning($"User with login: {login} not found");
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occured GetUserByLogin with login: {login}");
                throw;
            }
        }
    }
}
