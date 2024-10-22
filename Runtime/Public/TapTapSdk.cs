using System;
using System.Threading.Tasks;
using TapSDK.Core;
using TapSDK.Core.Internal;
using UnityEngine;

namespace TapSDK.Login.Internal
{
    public static class TapTapSdk
    {
        // todo 
        public const string Version = "4.3.10";

        public static string ClientId { get; private set; }

        public static Region CurrentRegion { get; private set; }
        
        public static void SDKInitialize(string clientId, bool isCn) {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentException("[TapSDK] clientId is null or empty!");
            ClientId = clientId;
            CurrentRegion = isCn ? (Region)new RegionCN() : new RegionIO();
            TapLocalizeManager.SetCurrentRegion(isCn);
        }
    }
}