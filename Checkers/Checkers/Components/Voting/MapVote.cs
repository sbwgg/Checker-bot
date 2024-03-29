﻿// <copyright file="MapVote.cs" company="GambleDev">
// Copyright (c) GambleDev. All rights reserved.
// </copyright>

namespace Checkers.Components.Voting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Checkers.Components;
    using Checkers.Data.Models;

    public class MapVote : Vote
    {
        public MapVote(MapVoteManager manager, int id, Match match, KeyValuePair<string, MapType> map)
            : base((ulong)id, VoteType.MapPick, match)
        {
            this.Title = map.Key;
            this.Maptype = map.Value;
            this.Manager = manager;

            switch (map.Value)
            {
                case MapType.Assault:
                    {
                        this.TypeName = "Assault";
                        break;
                    }

                case MapType.Hybrid:
                    {
                        this.TypeName = "Hybrid";
                        break;
                    }

                case MapType.Escort:
                    {
                        this.TypeName = "Escort";
                        break;
                    }

                case MapType.Control:
                    {
                        this.TypeName = "Control";
                        break;
                    }

                default:
                    break;
            }
        }

        public string TypeName { get; }

        public MapVoteManager Manager { get; }

        public MapType Maptype { get; }

        public override bool AddForVote(Player player)
        {
            this.TotalVotes++;
            this.VoterIDs.Add(player.Id);

            return true;
        }

        public void RemoveVote(Player player)
        {
            this.VoterIDs.Remove(player.Id);
            this.TotalVotes--;
        }
    }
}
