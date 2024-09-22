using BepInEx;
using BepInEx.Logging;
using GnomePlushV2.Behaviours;
using HarmonyLib;
using LethalLib.Modules;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GnomePlushV2
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("evaisa.lethallib", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.sigurd.csync", "5.0.1")] 
    public class GnomePlushV2 : BaseUnityPlugin
    {
        public static GnomePlushV2 Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }
        internal static GnomeConfig gnomeConfig;
        internal static System.Random random = new System.Random(0);

        private const string gnomeAssetbundleName = "gnomeassets";
        private const string gnomeItemPropertiesLocation = "Assets/Scrap/Gnome/GnomePlush.asset";

        internal static bool areGnomeAsstesValid = true;


        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;
            gnomeConfig = new GnomeConfig(Config);
            Logger.LogInfo("GnomePlush awake");

            string gnomeAssetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), gnomeAssetbundleName);
            AssetBundle gnomeBundle = AssetBundle.LoadFromFile(gnomeAssetDir);

            if (gnomeBundle == null)
            {
                Logger.LogError("Gnome assetbundle missing.");
                areGnomeAsstesValid = false;
            }
            else
            {
                Logger.LogDebug("GnomeBundle okay");
            }

            Item gnomeItem = gnomeBundle.LoadAsset<Item>(gnomeItemPropertiesLocation);

            if (gnomeItem == null)
            {
                Logger.LogError("Gnome item could't be loaded from asset bundle.");
                areGnomeAsstesValid = false;
            }
            else
            {
                Logger.LogDebug("GnomeItem okay");
            }
            if (gnomeItem.spawnPrefab == null)
            {
                Logger.LogError("Gnome prefab could't be loaded from item.");
                areGnomeAsstesValid = false;
            }
            else
            {
                Logger.LogDebug("GnomePrefab okay");
            }

            List<AudioClip> audioClips = gnomeBundle.LoadAllAssets<AudioClip>().ToList();
            AudioSource audioSource = gnomeItem.spawnPrefab.GetComponent<AudioSource>();

            GnomeScript gnomeScript = gnomeItem.spawnPrefab.AddComponent<GnomeScript>();
            gnomeScript.grabbable = true;
            gnomeScript.grabbableToEnemies = true;
            gnomeScript.itemProperties = gnomeItem;
            gnomeScript.gnomeAudioSource = audioSource;

            foreach (AudioClip clip in audioClips)
            {
                switch (clip.name)
                {
                    case "whoo":
                        gnomeScript.gnomeSound = clip;
                        Logger.LogDebug("found whoo sfx");
                        break;
                    case "whoo with reverb":
                        gnomeScript.gnomeSoundReverb = clip;
                        Logger.LogDebug("found whoo with reverb sfx");
                        break;
                    default:
                        break;
                }
            }

            if (areGnomeAsstesValid)
            {
                NetworkPrefabs.RegisterNetworkPrefab(gnomeItem.spawnPrefab);
                Utilities.FixMixerGroups(gnomeItem.spawnPrefab);
                Items.RegisterScrap(gnomeItem, GnomePlushV2.gnomeConfig.GNOME_SCRAP_RARITY, Levels.LevelTypes.All);

                Logger.LogInfo("Gnomes await you in the dungeons...");
            }
            else
            {
                Logger.LogError("Something went wrong, gnome mod broke. Now the gnomes are unhappy and won't spawn. >:c");
            }
        }
    }
}
