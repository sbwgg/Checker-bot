﻿// <copyright file="Team.cs" company="GambleDev">
// Copyright (c) GambleDev. All rights reserved.
// </copyright>

namespace Checkers.Data.Models.Game
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Checkers.Data.Models.Ranked;

    /// <summary>
    /// Team class defining a standard Checkers team.
    /// </summary>
    public class Team
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Team"/> class.
        /// </summary>
        /// <param name="players"> The players for this Team. </param>
        /// <param name="vcID"> The Voice Channel ID for Team VC. </param>
        public Team(List<Player> players, ulong vcID)
        {
            this.Players = players;
            this.VoiceID = vcID;

            this.AverageRating = this.GetAverageRating();
        }

        /// <summary>
        /// Gets the Players on the team.
        /// </summary>
        public List<Player> Players { get; }

        /// <summary>
        /// Gets the UId of this Teams voice channel.
        /// </summary>
        public ulong VoiceID { get; }

        /// <summary>
        /// Gets the Average <see cref="RatingUtils"/> of this Team.
        /// </summary>
        public int AverageRating { get; }

        private int GetAverageRating()
        {
            int skillRating = 0;

            foreach (Player player in this.Players)
            {
                skillRating += player.GetCurrentRanting();
            }

            int skillAverage = skillRating / this.Players.Count;

            return skillAverage;
        }
    }
}