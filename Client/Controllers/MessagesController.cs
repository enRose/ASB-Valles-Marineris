using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client.Models;
using Client.Workers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Client.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly IBotConnector botConnector;

        public MessagesController(IBotConnector botConnector)
        {
            this.botConnector = botConnector;
        }

        [HttpPost("[action]")]
        public async Task<Chat> Talk(Chat chat)
        {
            return await botConnector.TalkToBot(chat.ChatMessage);
        }
    }
}
