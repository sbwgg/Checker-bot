﻿// <copyright file="ComponentHandler.cs" company="GambleDev">
// Copyright (c) GambleDev. All rights reserved.
// </copyright>

namespace Checkers.Services.Generic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Checkers.Common;
    using Checkers.Components.Voting;
    using Checkers.Data;
    using Checkers.Data.Models;
    using Discord;
    using Discord.Addons.Hosting;
    using Discord.Commands;
    using Discord.WebSocket;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class ComponentHandler : CheckersService
    {
        private readonly IServiceProvider provider;
        private readonly CommandService service;
        private readonly IConfiguration configuration;

        private MatchManager matchManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentHandler"/> class.
        /// </summary>
        /// <param name="provider"> The <see cref="IServiceProvider"/> that should be injected. </param>
        /// <param name="client"> The <see cref="DiscordSocketClient"/> that should be injected. </param>
        /// <param name="logger"> The <see cref="ILogger"/> that should be injected. </param>
        /// <param name="service"> The <see cref="CommandService"/> that should be injected. </param>
        /// <param name="configuration"> The <see cref="IConfiguration"/> that should be injected. </param>
        /// <param name="dataAccessLayer"> The <see cref="DataAccessLayer"/> that should be injected. </param>
        public ComponentHandler(IServiceProvider provider, DiscordSocketClient client, ILogger<DiscordClientService> logger, CommandService service, IConfiguration configuration, DataAccessLayer dataAccessLayer, MatchManager mm)
            : base(client, logger, configuration, dataAccessLayer)
        {
            this.provider = provider;
            this.service = service;
            this.configuration = configuration;
            this.matchManager = mm;
        }

        public async Task ButtonHandler(SocketMessageComponent component)
        {
            var player = this.DataAccessLayer.HasPlayer(component.User.Id);

            if (player != null)
            {
                var match = this.matchManager.GetMatchOfPLayer(player);

                if (match != null)
                {
                    switch (component.Data.CustomId)
                    {
                        case "match_vote_yes":
                            {
                                Vote vote = match.GetVote(component.Channel.Id);

                                if (await this.AddVote(vote, component, player, true))
                                {

                                    EndMatchVote? matchvote = vote as EndMatchVote;

                                    if (matchvote != null)
                                    {
                                        await this.matchManager.ProcessMatch(matchvote, component.Channel);
                                    }
                                }

                                break;
                            }

                        case "match_vote_no":
                            {
                                Vote vote = match.GetVote(component.Channel.Id);

                                await this.AddVote(vote, component, player, false);

                                break;
                            }

                        case "match_forfeit_yes":
                            {
                                Vote vote = match.GetVote(component.Channel.Id);

                                if (await this.AddVote(vote, component, player, true))
                                {
                                    EndMatchVote? matchvote = vote as EndMatchVote;

                                    if (matchvote != null)
                                    {
                                        await this.matchManager.ProcessMatch(matchvote, component.Channel);
                                    }
                                }

                                break;
                            }

                        case "match_forfeit_no":
                            {
                                Vote vote = match.GetVote(component.Channel.Id);

                                await this.AddVote(vote, component, player, false);

                                break;
                            }

                        case "match_disconnect_yes":
                            {
                                Vote vote = match.GetVote(component.Channel.Id);

                                if (await this.AddVote(vote, component, player, true))
                                {
                                    EndMatchVote? matchvote = vote as EndMatchVote;

                                    if (matchvote != null)
                                    {
                                        await this.matchManager.ProcessMatch(matchvote, component.Channel);
                                    }
                                }

                                break;
                            }

                        case "match_disconnect_no":
                            {
                                Vote vote = match.GetVote(component.Channel.Id);

                                await this.AddVote(vote, component, player, false);

                                break;
                            }

                        case "match_vote_a":
                            {
                                Vote vote = match.GetVote(0);

                                if (vote is MapVote mapVote)
                                {
                                    await this.AddMapVote(mapVote, component, player);
                                }

                                break;
                            }

                        case "match_vote_b":
                            {
                                Vote vote = match.GetVote(1);

                                if (vote is MapVote mapVote)
                                {
                                    await this.AddMapVote(mapVote, component, player);
                                }

                                break;
                            }

                        case "match_vote_c":
                            {
                                Vote vote = match.GetVote(2);

                                if (vote is MapVote mapVote)
                                {
                                    await this.AddMapVote(mapVote, component, player);
                                }

                                break;
                            }

                        default:
                            break;
                    }
                }
            }
        }

        private async Task<bool> AddVote(Vote vote, SocketMessageComponent component, Player player, bool votefor)
        {
            var result = await CheckersMessageFactory.ModifyVote(vote, component, player, votefor);
            if (result)
            {
                var guild = (component.Channel as IGuildChannel)?.Guild;

                if (guild != null)
                {
                    vote.Match.RemoveVote((SocketGuild)guild, component.Channel.Id, vote);
                    await vote.Match.Channels.ChangeTextPerms((SocketGuild)guild, component.Channel.Id, true);
                }
            }

            return result;
        }

        private async Task AddMapVote(MapVote mapVote, SocketMessageComponent component, Player player)
        {
            await CheckersMessageFactory.ModifyMapVoteOnVote(mapVote, component, player);
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
           this.Client.ButtonExecuted += this.ButtonHandler;
           await this.service.AddModulesAsync(Assembly.GetEntryAssembly(), this.provider);
        }
    }
}
