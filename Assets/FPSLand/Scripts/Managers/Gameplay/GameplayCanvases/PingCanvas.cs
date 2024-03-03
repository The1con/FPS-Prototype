
using FishNet;
using TMPro;
using UnityEngine;

namespace FirstGearGames.FPSLand.Managers.Gameplay.Canvases
{

    public class PingCanvas : MonoBehaviour
    {
        #region Serialized.
        /// <summary>
        /// Text to show ping.
        /// </summary>
        [Tooltip("Text to show ping.")]
        [SerializeField]
        private TextMeshProUGUI _pingText;
        #endregion

        private void FixedUpdate()
        {
            long ping = InstanceFinder.TimeManager.RoundTripTime;
            _pingText.text = ping.ToString() + "ms";

            if (ping >= 200)
                _pingText.color = Color.red;
            else if (ping >= 100)
                _pingText.color = Color.yellow;
            else
                _pingText.color = Color.white;
        }

    }


}