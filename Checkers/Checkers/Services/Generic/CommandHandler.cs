﻿// <copyright file="CommandHandler.cs" company="GambleDev">
// Copyright (c) GambleDev. All rights reserved.
// </copyright>

namespace Checkers.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Checkers.Data;
    using Discord;
    using Discord.Addons.Hosting;
    using Discord.Commands;
    using Discord.WebSocket;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Command Handler for the Client.
    /// </summary>
    public class CommandHandler : CheckersService
    {
        private readonly IServiceProvider provider;
        private readonly CommandService service;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandler"/> class.
        /// </summary>
        /// <param name="provider"> The <see cref="IServiceProvider"/> that should be injected. </param>
        /// <param name="client"> The <see cref="DiscordSocketClient"/> that should be injected. </param>
        /// <param name="logger"> The <see cref="ILogger"/> that should be injected. </param>
        /// <param name="service"> The <see cref="CommandService"/> that should be injected. </param>
        /// <param name="configuration"> The <see cref="IConfiguration"/> that should be injected. </param>
        /// <param name="dataAccessLayer"> The <see cref="DataAccessLayer"/> that should be injected. </param>
        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, ILogger<DiscordClientService> logger, CommandService service, IConfiguration configuration, DataAccessLayer dataAccessLayer)
            : base(client, logger, configuration, dataAccessLayer)
        {
           this.provider = provider;
           this.service = service;
           this.configuration = configuration;
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.Client.MessageReceived += this.OnMessageReceived;
            this.service.CommandExecuted += this.OnCommandExcecuted;
            this.Client.UserJoined += this.OnUserJoin;
            this.Client.UserLeft += this.OnUserLeave;
            this.Client.PresenceUpdated += this.OnUserUpdate;
            await this.service.AddModulesAsync(Assembly.GetEntryAssembly(), this.provider);
        }

        private async Task OnUserUpdate(SocketUser user, SocketPresence oldStatus, SocketPresence newStatus)
        {
            if (newStatus.Status == UserStatus.Offline)
            {
                var player = this.DataAccessLayer.HasPlayer(user.Id);

                if (player != null)
                {
                    player.IsActive = false;
                    await this.DataAccessLayer.UpdatePlayer(player);
                }
            }
            else if (oldStatus.Status == UserStatus.Offline)
            {
                var player = this.DataAccessLayer.HasPlayer(user.Id);

                if (player != null)
                {
                    player.IsActive = true;
                    await this.DataAccessLayer.UpdatePlayer(player);
                }
            }
        }

        private async Task OnCommandExcecuted(Optional<CommandInfo> commandInfo, ICommandContext commandContext, IResult result)
        {
            if (result.IsSuccess)
            {
                return;
            }

            // Send this to me or to a dev channel.
            await commandContext.Channel.SendMessageAsync(result.ErrorReason);
        }

        private async Task OnUserJoin(SocketUser user)
        {
            #region editname
            /*
             player.Registered = true;

                        if (nameAllowed)
                        {
                            if (player.Username != name)
                            {
                                if (args != null && args.Name != "name:")
                                {
                                    // This seemns like an awful way to do it. Problem is we need feedback instantly with new name. How?
                                    await this.DataAccessLayer.UpdatePlayerName(player, name);
                                    await user.SendMessageAsync($"Account already exists. Your name has been updated to {player.Username}!");

                                    // Database login function.
                                }
                            }

                            await this.Context.Message.ReplyAsync($"Welcome back, {player.Username}!");
                        }
                        else
                        {
                            await this.ReplyAsync($"The chosen name is inappropiate.");
                            return;
                        }
            */
            #endregion

            if (user is not SocketGuildUser)
            {
                return;
            }

            SocketGuildUser socketGuildUser = (SocketGuildUser)user;

            var player = this.DataAccessLayer.HasPlayer(socketGuildUser.Id);
            if (player != null)
            {
                var role = socketGuildUser.Guild.GetRole(CheckersConstants.RegisterRole);
                await socketGuildUser.AddRoleAsync(role);

                player.IsActive = false;
                await this.DataAccessLayer.UpdatePlayer(player);
            }
        }

        private async Task OnUserLeave(SocketGuild guild, SocketUser user)
        {
            var player = this.DataAccessLayer.HasPlayer(user.Id);

            if (player != null)
            {
                player.IsActive = false;
                await this.DataAccessLayer.UpdatePlayer(player);
            }
        }

        private async Task OnMessageReceived(SocketMessage socketMessage)
        {
            if (socketMessage is not SocketUserMessage message)
            {
                return;
            }

            if (message.Source != MessageSource.User)
            {
                return;
            }

            var argPos = 0;
            var prefix = string.Empty;

            if (message.Author is SocketGuildUser user)
            {
                prefix = this.DataAccessLayer.GetPrefix(user.Guild.Id);
            }
            else if (message.Author is SocketUser priv_user)
            {
                prefix = "!";
            }

            if (!message.HasStringPrefix(prefix, ref argPos) &&
                    !message.HasMentionPrefix(this.Client.CurrentUser, ref argPos))
            {
                return;
            }

            var context = new SocketCommandContext(this.Client, message);
            await this.service.ExecuteAsync(context, argPos, this.provider);
        }
    }
}
