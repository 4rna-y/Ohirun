using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Ohirun.Commands
{
    public interface ISlashCommand
    {
        string Name { get; }
        SlashCommandBuilder GetCommandBuilder();
        Task HandleAsync(SocketSlashCommand command);
    }
}