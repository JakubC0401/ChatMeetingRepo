using ChatMeeting.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatMeeting.Core.Domain.Interfaces.Services
{
    public interface IChatService
    {
        Task<ChatDto> GetPaginatedChat(string chatName, int pageNumber, int pageSize);
        Task SaveMessage(MessageDto messageDto);
    }
}
