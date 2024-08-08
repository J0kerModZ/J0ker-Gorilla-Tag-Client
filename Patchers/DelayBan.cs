using HarmonyLib;
using UnityEngine;

namespace J0kersClient.Patchers
{
    [HarmonyPatch(typeof(GorillaGameManager), "ForceStopGame_DisconnectAndDestroy")]
    internal class DelayBan : MonoBehaviour
    {
        public static bool Prefix()
        {
            return false;
        }
    }
}