using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace GnomePlushV2.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    public static class RoundManagerRandomSeedPatch
    {
        [HarmonyPatch("GenerateNewLevelClientRpc")]
        [HarmonyPostfix]
        private static void OnNewRandomSeed(int randomSeed)
        {
            GnomePlushV2.randomSize = new Random(randomSeed);
            GnomePlushV2.randomNoise = new Random(randomSeed + 100);
        }
    }
}
