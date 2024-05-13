using System;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseChatBot.Api.Controllers
{
    [ApiController]
    [Route("/api/v1/chat")]
    public class hatController : ControllerBase
    {
        private readonly Plugins.OrchestratorPlugin.Orchestrator _orchestrator;

        public hatController(Plugins.OrchestratorPlugin.Orchestrator orchestrator, Services.IDataProvider dp)
        {
            _orchestrator = orchestrator;
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> ProcessMessageInputChat()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                string userInput = await reader.ReadToEndAsync();

                if (string.IsNullOrEmpty(userInput))
                    return BadRequest();

                string result = await _orchestrator.RouteRequestAsync(userInput);

                return new ContentResult()
                {
                    Content = result,
                    ContentType = "text/plain"
                };
            }

        }
    }
}

