﻿using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Object;
using FishNet.Serializing;
using GameKit.Utilities;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FishNet.Managing.Server
{
    public sealed partial class ServerManager : MonoBehaviour
    {
        #region Private.
        /// <summary>
        /// Cached expected level of detail value.
        /// </summary>
        private uint _cachedLevelOfDetailInterval;
        /// <summary>
        /// Cached value of UseLod.
        /// </summary>
        private bool _cachedUseLod;
        #endregion

        /// <summary>
        /// Parses a received network LOD update.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ParseNetworkLODUpdate(PooledReader reader, NetworkConnection conn)
        {
            //PROSTART
            if (!conn.Authenticated)
                return;
            if (!_cachedUseLod)
            {
                conn.Kick(reader, KickReason.ExploitAttempt, LoggingType.Common, $"Connection [{conn.ClientId}] sent a level of detail update when the feature is not enabled.");
                return;
            }

            /* If local client then read out entries but do nothing.
             * Local client doesn't technically have to send LOD because
             * it's set on the client side but this code is kept in
             * to simulate actual bandwidth. */
            if (conn.IsLocalClient)
            {
                int w = reader.ReadInt32();
                for (int i = 0; i < w; i++)
                    ReadLod(out _, out _);
                return;
            }

            uint packetTick = conn.PacketTick.LastRemoteTick;
            //Check if conn can send LOD.
            uint lastLod = conn.LastLevelOfDetailUpdate;
            //If previously set see if client is potentially exploiting.
            if (lastLod != 0)
            {
                if ((packetTick - lastLod) < _cachedLevelOfDetailInterval)
                {
                    conn.Kick(reader, KickReason.ExploitAttempt, LoggingType.Common, $"Connection [{conn.ClientId}] sent excessive level of detail updates.");
                    return;
                }
            }
            //Set last recv lod.
            conn.LastLevelOfDetailUpdate = packetTick;

            //Get server objects to save calls.
            Dictionary<int, NetworkObject> serverObjects = Objects.Spawned;
            //Get level of details for this connection and reset them. 
            Dictionary<NetworkObject, NetworkConnection.LevelOfDetailData> currentLods = conn.LevelOfDetails;

            int written = reader.ReadInt32();
            //Only process if some are written.
            if (written > 0)
            {
                //Maximum infractions before a kick.
                const int maximumInfractions = 15;
                int currentInfractions = conn.LevelOfDetailInfractions;
                int infractionsCounted = 0;

                /* If the connection has no objects then LOD isn't capable
                 * of being calculated. It's possible the players object was destroyed after
                 * the LOD sent but we don't know for sure without adding extra checks.
                 * Rather than add recently destroyed player object checks if there are
                 * no player objects then just add an infraction. The odds of this happening regularly
                 * are pretty slim. */
                if (conn.Objects.Count == 0)
                {
                    if (AddInfraction(3))
                    {
                        conn.Kick(reader, KickReason.UnusualActivity, LoggingType.Common, $"Connection [{conn.ClientId}] has sent an excessive number of level of detail updates without having any player objects spawned.");
                        return;
                    }
                }

                /* If written is more than spawned + recently despawned then
                 * the client is likely trying to exploit. */
                if (written > (Objects.Spawned.Count + Objects.RecentlyDespawnedIds.Count))
                {
                    conn.Kick(reader, KickReason.UnusualActivity, LoggingType.Common, $"Connection [{conn.ClientId}] sent a level of detail update for {written} items which exceeds spawned and recently despawned count.");
                    return;
                }


                List<float> lodDistances = NetworkManager.ObserverManager.GetLevelOfDetailDistances();
                int lodDistancesCount = lodDistances.Count;
                for (int i = 0; i < written; i++)
                {
                    int objectId;
                    byte lod;
                    ReadLod(out objectId, out lod);

                    //Lod is not possible.
                    if (lod >= lodDistancesCount)
                    {
                        conn.Kick(reader, KickReason.ExploitAttempt, LoggingType.Common, $"Connection [{conn.ClientId}] provided a level of detail index which is out of bounds.");
                        return;
                    }

                    //Found in spawned, update lod.
                    if (serverObjects.TryGetValue(objectId, out NetworkObject nob))
                    {
                        NetworkConnection.LevelOfDetailData cachedLod;
                        //Value is unchanged.
                        if (currentLods.TryGetValue(nob, out cachedLod))
                        {
                            bool oldMatches = (cachedLod.CurrentLevelOfDetail == lod);
                            if (oldMatches && AddInfraction())
                            {
                                conn.Kick(reader, KickReason.UnusualActivity, LoggingType.Common, $"Connection [{conn.ClientId}] has excessively sent unchanged LOD information.");
                                return;
                            }
                        }
                        else
                        {
                            cachedLod = ObjectCaches<NetworkConnection.LevelOfDetailData>.Retrieve();
                            currentLods[nob] = cachedLod;
                        }

                        cachedLod.PreviousLevelOfDetail = cachedLod.CurrentLevelOfDetail;
                        cachedLod.CurrentLevelOfDetail = lod;
                    }
                    //Not found in spawn; validate that client isn't trying to exploit.
                    else
                    {
                        //Too many infractions.
                        if (AddInfraction())
                        {
                            conn.Kick(reader, KickReason.UnusualActivity, LoggingType.Common, $"Connection [{conn.ClientId}] has accumulated excessive level of detail infractions.");
                            return;
                        }
                    }
                }

                //Adds an infraction returning if maximum infractions have been exceeded.
                bool AddInfraction(int count = 1)
                {
                    /* Only increase infractions at most 3 per iteration.
                    * This is to prevent a kick if the client perhaps had
                    * a massive lag spike. */
                    if (infractionsCounted < 3)
                        infractionsCounted += count;

                    bool overLimit = ((currentInfractions + infractionsCounted) >= maximumInfractions);
                    return overLimit;
                }
            }

            //Reads a LOD.
            void ReadLod(out int lObjectId, out byte lLod)
            {
                lObjectId = reader.ReadNetworkObjectId();
                lLod = reader.ReadByte();
            }

            //Remove an infraction. This will steadily remove infractions over time.
            if (conn.LevelOfDetailInfractions > 0)
                conn.LevelOfDetailInfractions--;
            //PROEND
        }


    }


}
