using HarmonyLib;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J0kersClient.Patchers
{
    [HarmonyPatch(typeof(PlayFabClientInstanceAPI), "ReportPlayer", 0)]
    internal class NoReport
    {
        private static bool Prefix(ReportPlayerClientRequest request, Action<ReportPlayerClientResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
        {
            return false;
        }
    }
}
