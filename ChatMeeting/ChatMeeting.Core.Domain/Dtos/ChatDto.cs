using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatMeeting.Core.Domain.Dtos
{
    public class ChatDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }  
        public HashSet<MessageDto>? Messages { get; set; }

    }
}
