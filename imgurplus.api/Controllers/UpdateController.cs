using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using imgurplusbot.bll.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace imgurplus.api.Controllers
{
    [Route("api/[controller]")]
    public class UpdateController : Controller
    {
        private readonly IUpdateService _updateService;
        public UpdateController(IUpdateService updateService)
        {
            _updateService = updateService;
        }

        // POST api/update
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Update update)
        {
            await _updateService.ProcessRequest(update);
            return Ok();
        }

        // GET api/update
        [HttpGet]
        public Task<string> Get() 
        {
            return Task<string>.Factory.StartNew(() => 
            {
                Thread.Sleep(5000);
                return "ping";
            });
        }

    }
}