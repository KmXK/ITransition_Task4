using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Task4.Hubs
{
    public class ChatHub: Hub
    {
        public async Task Send(string lastMessage, string broCount, string sisCount)
        {
            await Clients.All.SendAsync("Receive", lastMessage, broCount, sisCount);
        }
    }
}