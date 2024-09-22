using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace GnomePlushV2.Patches
{
    internal class GnomeRandomPatch
    {
        [HarmonyPatch(typeof(RoundManager))]
        public static class RoundManagerPatch
        {
            [HarmonyPatch("GenerateNewLevelClientRpc")]
            [HarmonyPostfix]
            private static void OnNewRandomSeed(int randomSeed)
            {
                GnomePlushV2.random = new Random(randomSeed);
            }
        }
    }
}
