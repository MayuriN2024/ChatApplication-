using Microsoft.AspNetCore.Authorization;
using PingMe.Server.Data;
using PingMe.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PingMe.Server.Controllers
{
    [ApiController] // Matches the functionality in MessageController.java
    [Route("api/messages")]
    public class MessageController : ControllerBase
    {
        private readonly ChatDbContext _context;

        public MessageController(ChatDbContext context)
        {
            _context = context;
        }

        [HttpGet("private")]
        public async Task<ActionResult<List<ChatMessage>>> GetPrivateMessages([FromQuery] string user1, [FromQuery] string user2)
        {
            var messages = await _context.ChatMessages
                .Where(m => (m.Sender == user1 && m.Recipient == user2) || (m.Sender == user2 && m.Recipient == user1))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            return Ok(messages);
        }
    }
}
