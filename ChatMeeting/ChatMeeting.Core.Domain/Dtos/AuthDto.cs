using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatMeeting.Core.Domain.Dtos
{
    public class AuthDto
    {
        public string Token { get; set; }
        public DateTime ExpiredDate { get; set; }
    }
}
