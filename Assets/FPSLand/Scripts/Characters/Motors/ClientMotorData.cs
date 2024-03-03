using System.Collections.Generic;
using UnityEngine;

namespace FirstGearGames.FPSLand.Characters.Motors
{


    /// <summary>
    /// Data for authoritive movement on client.
    /// </summary>
    public class ClientMotorData
    {
        /// <summary>
        /// Inputs which have been sent to the server and not yet returned, or inputs in queue to be sent to the server.
        /// </summary>
        public List<ReplicateData> CachedInputs = new List<ReplicateData>();
        /// <summary>
        /// Unprocessed input results from the server.
        /// </summary>
        public ReconcileData? InputResult = null;
    }


}