using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Ohirun.Commands;

namespace Ohirun.Services
{
    public class SlashCommandService
    {
        private readonly DiscordSocketClient client;
        private readonly ILogger<SlashCommandService> logger;
        private readonly ISlashCommandRegistry commandRegistry;

        public SlashCommandService(DiscordSocketClient client, ILogger<SlashCommandService> logger, ISlashCommandRegistry commandRegistry)
        {
            this.client = client;
            this.logger = logger;
            this.commandRegistry = commandRegistry;
        }

        public async Task RegisterCommandsAsync()
        {
            try
            {
                logger.LogInformation("Starting guild command registration...");
                logger.LogInformation("Bot ID: {BotId}", client.CurrentUser?.Id ?? 0);
                
                foreach (SocketGuild guild in client.Guilds)
                {
                    try
                    {
                        logger.LogInformation("Registering commands for guild: {GuildName} ({GuildId})", guild.Name, guild.Id);

                        IReadOnlyCollection<SocketApplicationCommand> existingGuildCommands = await guild.GetApplicationCommandsAsync();
                        
                        foreach (ISlashCommand slashCommand in commandRegistry.GetCommands())
                        {
                            bool commandExists = existingGuildCommands.Any(cmd => cmd.Name == slashCommand.Name);
                            
                            if (commandExists)
                            {
                                logger.LogInformation("/{CommandName} command already exists in guild {GuildName}, skipping", slashCommand.Name, guild.Name);
                                continue;
                            }

                            SlashCommandBuilder commandBuilder = slashCommand.GetCommandBuilder();
                            SocketApplicationCommand createdCommand = await guild.CreateApplicationCommandAsync(commandBuilder.Build());
                            logger.LogInformation("Successfully registered /{CommandName} command in guild {GuildName} with ID: {CommandId}", slashCommand.Name, guild.Name, createdCommand.Id);
                        }
                    }
                    catch (Exception guildEx)
                    {
                        logger.LogError(guildEx, "Failed to register commands in guild {GuildName} ({GuildId})", guild.Name, guild.Id);
                    }
                }
                
                logger.LogInformation("Guild commands are available immediately after registration");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to register commands");
            }
        }

        public async Task HandleSlashCommandAsync(SocketSlashCommand command)
        {
            ISlashCommand? slashCommand = commandRegistry.GetCommands().FirstOrDefault(cmd => cmd.Name == command.Data.Name);
            
            if (slashCommand == null)
            {
                await command.RespondAsync("Unknown command", ephemeral: true);
                logger.LogWarning("Unknown command: {CommandName}", command.Data.Name);
                return;
            }

            await slashCommand.HandleAsync(command);
        }
    }
}