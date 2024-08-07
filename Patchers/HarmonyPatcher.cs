﻿using BepInEx;

namespace J0kersClient.Patchers
{
    [BepInPlugin("com.J0ker.Patch", "J0kerPatcher", "1.0.0")]
    internal class ApplyMenuPatcher : BaseUnityPlugin
    {
        private void OnEnable()
        {
            MenuPatch.ApplyHarmonyPatches();
        }

        private void OnDisable()
        {
            MenuPatch.RemoveHarmonyPatches();
        }
    }
}