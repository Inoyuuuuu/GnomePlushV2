using BepInEx.Configuration;
using CSync.Lib;
using CSync.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace GnomePlushV2
{
    [DataContract]
    internal class GnomeConfig : SyncedConfig<GnomeConfig>
    {
        [DataMember]
        internal SyncedEntry<int> GNOME_SCRAP_RARITY;
        [DataMember]
        internal SyncedEntry<int> GNOME_NOISES_MAX_INTERVAL;
        [DataMember]
        internal SyncedEntry<int> GNOME_NOISES_MIN_INTERVAL;
        [DataMember]
        internal SyncedEntry<int> GNOME_REVERB_CHANCE;

        private const int gnomeRarity_DV = 50;
        private const int gnomeSoundsMaxInterval_DV = 130; //seconds
        private const int gnomeSoundsMinInterval_DV = 50;  //seconds
        private const int gnomeReverbSoundChance_DV = 10;  //in percent

        public GnomeConfig(ConfigFile cfg) : base(MyPluginInfo.PLUGIN_NAME)
        {
            ConfigManager.Register(this);

            GNOME_SCRAP_RARITY = cfg.BindSyncedEntry("Gnome", "gnomeRarity", gnomeRarity_DV, "Sets the rarity of the gnome plush.");
            GNOME_NOISES_MAX_INTERVAL = cfg.BindSyncedEntry("GnomeNoises", "gnomeNoisesMaxInterval", gnomeSoundsMaxInterval_DV, "Sets the max time between two gnome noises (in seconds).");
            GNOME_NOISES_MIN_INTERVAL = cfg.BindSyncedEntry("GnomeNoises", "gnomeNoisesMinInterval", gnomeSoundsMinInterval_DV, "Sets the min time between two gnome noises (in seconds).");
            GNOME_REVERB_CHANCE = cfg.BindSyncedEntry("GnomeNoises", "gnomeReverbNoiseChance", gnomeReverbSoundChance_DV, "Sets the chance of a reverb gnome noise happening (in percent).");
        }
    }
}
