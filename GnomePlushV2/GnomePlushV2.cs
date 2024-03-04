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
    public class GnomePlushV2 : BaseUnityPlugin
    {
        public static GnomePlushV2 Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }
        internal static GnomeConfig gnomeConfig;

        private const string gnomeAssetbundleName = "gnomeassets";
        private const string gnomeItemPropertiesLocation = "Assets/Scrap/Gnome/HGnome.asset";
        private const int gnomeRarity = 50;

        private static bool areGnomeAsstesValid = true;


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
            gnomeScript.scrapValue = 60;

            foreach (AudioClip clip in audioClips)
            {
                if (clip.name.Equals("whoo"))
                {
                    gnomeScript.gnomeSound = clip;
                    Logger.LogDebug("found sound 1");
                }
                if (clip.name.Equals("whoo with reverb"))
                {
                    gnomeScript.gnomeSoundReverb = clip;
                    Logger.LogDebug("found sound 2");
                }
            }

            if (areGnomeAsstesValid)
            {

                NetworkPrefabs.RegisterNetworkPrefab(gnomeItem.spawnPrefab);
                Utilities.FixMixerGroups(gnomeItem.spawnPrefab);
                Items.RegisterScrap(gnomeItem, GnomeConfig.Instance.GNOME_SCRAP_RARITY, Levels.LevelTypes.All);

                Logger.LogInfo("Gnome awaits you in the dungeons...");
            }
            else
            {
                Logger.LogError("Something went wrong, game won't spawn gnomes.");
            }
        }
    }
}
