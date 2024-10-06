using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GnomePlushV2.Patches
{
    [HarmonyPatch()]
    public class RoundManagerRandomSeedPatch
    {
        [HarmonyPatch(typeof(RoundManager), "GenerateNewLevelClientRpc")]
        [HarmonyPostfix]
        private static void OnNewRandomSeed(int randomSeed)
        {
            InitRandoms(randomSeed);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
        public static void InitializeClientRandoms()
        {
            InitRandoms(StartOfRound.Instance.randomMapSeed);
        }

        private static void InitRandoms(int randomSeed)
        {
            GnomePlushV2.randomSize = new Random(randomSeed);
            GnomePlushV2.randomSeed = randomSeed;
        }
    }
}
