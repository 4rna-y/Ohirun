using System.Collections.Generic;
using Ohirun.Commands;

namespace Ohirun.Commands
{
    public interface ISlashCommandRegistry
    {
        IEnumerable<ISlashCommand> GetCommands();
    }
}