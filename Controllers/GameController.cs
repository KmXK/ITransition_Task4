using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Task4.Hubs;
using Task4.Models;
using Task4.Services;

namespace Task4.Controllers
{
    [Authorize]
    public class GameController : Controller
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IFileAccessService _fileAccessService;

        public GameController(IHubContext<ChatHub> hubContext, IWebHostEnvironment webHostEnvironment,
            IFileAccessService fileAccessService)
        {
            _hubContext = hubContext;
            _webHostEnvironment = webHostEnvironment;
            _fileAccessService = fileAccessService;
        }

        public IActionResult Index()
        {
            var path = _webHostEnvironment.WebRootPath + "/App_Data/Data.txt";
            string lastMessage = "",
                broCount = "",
                sisCount = "";

            string[] lines = _fileAccessService.ReadLines(path);
            if (lines.Length >= 3)
            {
                lastMessage = lines[0];
                broCount = lines[1];
                sisCount = lines[2];
            }

            return View(new GameViewModel()
            {
                Username = User.Identity.Name,
                LastMessageName = lastMessage,
                BroCount = broCount,
                SisCount = sisCount
            });
        }

        [HttpPost]
        public async Task<IActionResult> Send(string message)
        {
            var lastMessage = $"Sent by {User.Identity.Name}: {message}.";
            var path = _webHostEnvironment.WebRootPath + "/App_Data/Data.txt";
            int broCount = 0, sisCount = 0;

            string[] lines = _fileAccessService.ReadLines(path);
            if (lines.Length >= 3)
            {
                broCount = Convert.ToInt32(lines[1]);
                sisCount = Convert.ToInt32(lines[2]);
            }

            if (message == "Bro")
                broCount++;
            else if (message == "Sis")
                sisCount++;

            lines = new string[3]
            {
                lastMessage,
                broCount.ToString(),
                sisCount.ToString()
            };

            _fileAccessService.WriteLines(path, lines);

            await _hubContext.Clients.All.SendAsync("Receive", lastMessage, broCount.ToString(), sisCount.ToString());
            return RedirectToAction(nameof(Index));
        }
    }
}
