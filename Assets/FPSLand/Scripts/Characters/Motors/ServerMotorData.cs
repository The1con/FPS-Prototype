using System.Collections.Generic;
using UnityEngine;

namespace FirstGearGames.FPSLand.Characters.Motors
{


    /// <summary>
    /// Data for authoritive movement on server.
    /// </summary>
    public class ServerMotorData
    {
        /// <summary>
        /// Network time of the last input which the server has processed.
        /// </summary>
        public uint LastInputFixedFrame = 0;
        /// <summary>
        /// Unprocessed user input from the client.
        /// </summary>
        public Queue<ReplicateData> UserInputs = new Queue<ReplicateData>();
    }


}