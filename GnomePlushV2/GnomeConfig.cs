using BepInEx.Configuration;
using CSync.Extensions;
using CSync.Lib;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace GnomePlushV2
{
    [DataContract]
    internal class GnomeConfig : SyncedConfig2<GnomeConfig>
    {
        internal ConfigEntry<int> GNOME_NOISE_BASE_VOLUME;

        [SyncedEntryField]
        internal SyncedEntry<int> GNOME_SCRAP_RARITY;
        [SyncedEntryField]
        internal SyncedEntry<bool> IS_GNOME_NOISE_ENABLED;
        [SyncedEntryField]
        internal SyncedEntry<bool> CAN_GNOME_ANGER_DOGS;
        [SyncedEntryField]
        internal SyncedEntry<int>  GNOME_REVERB_CHANCE, GNOME_NOISES_FREQUENCY, GNOME_NOISE_PITCH_CHANGE_AMOUNT;
        [SyncedEntryField]
        internal SyncedEntry<int> TINY_GNOME_SIZE_CHANCE, DEFAULT_GNOME_SIZE_CHANCE, BIG_GNOME_SIZE_CHANCE;
        [SyncedEntryField]
        internal SyncedEntry<float> EXPERIMENTAL_SIZE_MULTIPLIER;

        private const int gnomeRarity_DV = 37;
        private const int gnomeSoundsFrequency_DV = 45;           
        private const int gnomeReverbSoundChance_DV = 10;       
        private const int gnomeNoiseBaseVolume_DV = 60;         
        private const int gnomeNoisePitchChangeAmount_DV = 14;   
        private const int tinyGnomeSizeChance_DV = 15;          
        private const int defaultGnomeSizeChance_DV = 70;  
        private const int bigGnomeSizeChance_DV = 15;
        private const float experimentalSizeMultiplier_DV = 1f;

        private const bool isGnomeNoiseEnabled_DV = true;
        private const bool canGnomeAngerDogs_DV = true;

        public GnomeConfig(ConfigFile cfg) : base(MyPluginInfo.PLUGIN_GUID)
        {
            GNOME_NOISE_BASE_VOLUME = cfg.Bind("GnomeNoises", "gnomeNoiseBaseVolume", gnomeNoiseBaseVolume_DV,
                new ConfigDescription("The base volume of gnome-noises.", new AcceptableValueRange<int>(0, 100)));

            GNOME_SCRAP_RARITY = cfg.BindSyncedEntry("Gnome", "gnomeRarity", gnomeRarity_DV, 
                new ConfigDescription("Sets the rarity of the gnome plush.", new AcceptableValueRange<int>(1, 800)));
            IS_GNOME_NOISE_ENABLED = cfg.BindSyncedEntry("GnomeBehaviour", "isGnomeNoiseEnabled", isGnomeNoiseEnabled_DV, "Should the gnome make gnome-noises?");
            CAN_GNOME_ANGER_DOGS = cfg.BindSyncedEntry("GnomeBehaviour", "canGnomeAngerDogs", canGnomeAngerDogs_DV, "Should the gnome-noises be able to trigger eyeless dogs or other monsters?");
            GNOME_REVERB_CHANCE = cfg.BindSyncedEntry("GnomeNoises", "gnomeReverbNoiseChance", gnomeReverbSoundChance_DV, 
                new ConfigDescription("Sets the chance of a reverb gnome noise happening (in percent).", new AcceptableValueRange<int>(0, 100)));
            GNOME_NOISES_FREQUENCY = cfg.BindSyncedEntry("GnomeNoises", "gnomeNoisesFrequency", gnomeSoundsFrequency_DV, 
                new ConfigDescription("The higher the value, the more frequent the gnome noises.", new AcceptableValueRange<int>(1, 100)));
            GNOME_NOISE_PITCH_CHANGE_AMOUNT = cfg.BindSyncedEntry("GnomeNoises", "RandomPitchChange_Amount", gnomeNoisePitchChangeAmount_DV, 
                new ConfigDescription("Higher values will make the gnome noises vary more in pitch (in percent). \n (each gnome-noise's pitch is randomized).", new AcceptableValueRange<int>(0, 100)));
            TINY_GNOME_SIZE_CHANCE = cfg.BindSyncedEntry("GnomeSize", "tinyGnomeSizeChance", tinyGnomeSizeChance_DV,
                new ConfigDescription("Chance of tiny Gnomes to appear (relative to the values of the other size-chances).", new AcceptableValueRange<int>(0, 100)));
            DEFAULT_GNOME_SIZE_CHANCE = cfg.BindSyncedEntry("GnomeSize", "defaultGnomeSizeChance", defaultGnomeSizeChance_DV,
                new ConfigDescription("Chance of default-sized Gnomes to appear (relative to the values of the other size-chances).", new AcceptableValueRange<int>(0, 100)));
            BIG_GNOME_SIZE_CHANCE = cfg.BindSyncedEntry("GnomeSize", "bigGnomeSizeChance", bigGnomeSizeChance_DV,
                new ConfigDescription("Chance of big Gnomes to appear (relative to the values of the other size-chances).", new AcceptableValueRange<int>(0, 100)));
            EXPERIMENTAL_SIZE_MULTIPLIER = cfg.BindSyncedEntry("z_EXPERIMENTAL", "experimentalSizeMultiplier", experimentalSizeMultiplier_DV,
                new ConfigDescription("Increases the size of ALL gnomes (by a lot)! High values might lead to unforseen bugs/glitches!!!", new AcceptableValueRange<float>(1f, 100f)));
            ConfigManager.Register(this);
        }
    }
}
