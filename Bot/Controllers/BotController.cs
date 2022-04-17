using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BotController : ControllerBase
    {
        private readonly ILogger<BotController> _logger;

        public BotController(ILogger<BotController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public void Get()
        {
            TelegramBot.Start();
        }
    }
}
