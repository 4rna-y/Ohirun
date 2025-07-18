using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Ohirun.Commands
{
    public class SlashCommandRegistry : ISlashCommandRegistry
    {
        private readonly IServiceProvider serviceProvider;

        public SlashCommandRegistry(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IEnumerable<ISlashCommand> GetCommands()
        {
            yield return serviceProvider.GetRequiredService<OhiruCommand>();
            yield return serviceProvider.GetRequiredService<AddCommand>();
            yield return serviceProvider.GetRequiredService<LinkCommand>();
            yield return serviceProvider.GetRequiredService<ListCommand>();
        }
    }
}