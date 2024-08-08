using BepInEx;
using HarmonyLib;
using UnityEngine;
using J0kersClient.Client;
using J0kersClient.Patchers;

namespace J0kersClient.Client
{
    [BepInPlugin("com.J0kerMenu.J0kerModZ", "J0kerMenu", "1.0.0")]
    [HarmonyPatch(typeof(GorillaLocomotion.Player), "LateUpdate", MethodType.Normal)]
    public class Loader : BaseUnityPlugin
    {
        public void FixedUpdate()
        {
            if (!GameObject.Find("J0kerLoader") && GorillaLocomotion.Player.hasInstance)
            {
                GameObject Loader = new GameObject("J0kerLoader");

                // Menu & Mods
                Loader.AddComponent<Client>();
                Loader.AddComponent<Mods>();

                // I <3 event logging
                Loader.AddComponent<LogEvents>();
            }
        }
    }
}
