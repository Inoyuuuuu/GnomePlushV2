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
        internal SyncedEntry<bool> IS_GNOME_NOISE_ENABLED;
        [DataMember]
        internal SyncedEntry<bool> CAN_GNOME_ANGER_DOGS;
        [DataMember]
        internal SyncedEntry<int> GNOME_NOISE_BASE_VOLUME;
        [DataMember]
        internal SyncedEntry<int> GNOME_REVERB_CHANCE;
        [DataMember]
        internal SyncedEntry<int> GNOME_NOISES_MAX_INTERVAL;
        [DataMember]
        internal SyncedEntry<int> GNOME_NOISES_MIN_INTERVAL;
        [DataMember]
        internal SyncedEntry<int> GNOME_NOISE_VOLUME_CHANGE_AMOUNT;
        [DataMember]
        internal SyncedEntry<int> GNOME_NOISE_PITCH_CHANGE_AMOUNT;

        private const int gnomeRarity_DV = 50;
        private const int gnomeSoundsMaxInterval_DV = 130;      //seconds
        private const int gnomeSoundsMinInterval_DV = 50;       //seconds
        private const int gnomeReverbSoundChance_DV = 10;       //in percent
        private const int gnomeNoiseBaseVolume_DV = 60;         //in percent
        private const int gnomeNoiseVolumeChangeAmount_DV = 40; //in percent 
        private const int gnomeNoisePitchChangeAmount_DV = 15;  //in percent 

        private const bool isGnomeNoiseEnabled_DV = true;
        private const bool canGnomeAngerDogs_DV = true;

        public GnomeConfig(ConfigFile cfg) : base(MyPluginInfo.PLUGIN_GUID)
        {
            ConfigManager.Register(this);

            GNOME_SCRAP_RARITY = cfg.BindSyncedEntry("Gnome", "gnomeRarity", gnomeRarity_DV, "Sets the rarity of the gnome plush.");

            IS_GNOME_NOISE_ENABLED = cfg.BindSyncedEntry("GnomeBehaviour", "isGnomeNoiseEnabled", isGnomeNoiseEnabled_DV, "Should the gnome make gnome-noises?");
            CAN_GNOME_ANGER_DOGS = cfg.BindSyncedEntry("GnomeBehaviour", "canGnomeAngerDogs", canGnomeAngerDogs_DV, "Should the gnome-noises be able to trigger eyeless dogs or other monsters?");

            GNOME_NOISE_BASE_VOLUME = cfg.BindSyncedEntry("GnomeNoises", "gnomeNoiseBaseVolume", gnomeNoiseBaseVolume_DV, "The base volume of gnome-noises. Max value: 100");

            GNOME_REVERB_CHANCE = cfg.BindSyncedEntry("GnomeNoises", "gnomeReverbNoiseChance", gnomeReverbSoundChance_DV, "Sets the chance of a reverb gnome noise happening (in percent).");
            GNOME_NOISES_MAX_INTERVAL = cfg.BindSyncedEntry("GnomeNoises", "gnomeNoisesMaxInterval", gnomeSoundsMaxInterval_DV, "Sets the max time between two gnome noises (in seconds).");
            GNOME_NOISES_MIN_INTERVAL = cfg.BindSyncedEntry("GnomeNoises", "gnomeNoisesMinInterval", gnomeSoundsMinInterval_DV, "Sets the min time between two gnome noises (in seconds).");

            GNOME_NOISES_MAX_INTERVAL = cfg.BindSyncedEntry("GnomeNoises", "gnomeNoisesMaxInterval", gnomeSoundsMaxInterval_DV, "Sets the max time between two gnome noises (in seconds).");
            GNOME_NOISES_MIN_INTERVAL = cfg.BindSyncedEntry("GnomeNoises", "gnomeNoisesMinInterval", gnomeSoundsMinInterval_DV, "Sets the min time between two gnome noises (in seconds).");

            GNOME_NOISE_VOLUME_CHANGE_AMOUNT = cfg.BindSyncedEntry("GnomeNoises", "RandomVolumeChange_Amount", gnomeNoiseVolumeChangeAmount_DV, "Higher values will make the gnome noise vary more in volume (in percent). \n (each gnome-noise volume is randomized).");
            GNOME_NOISE_PITCH_CHANGE_AMOUNT = cfg.BindSyncedEntry("GnomeNoises", "RandomPitchChange_Amount", gnomeNoisePitchChangeAmount_DV, "Higher values will make the gnome noises vary more in pitch (in percent). \n (each gnome-noise's pitch is randomized).");

            
        }

        private void validateConfigs()
        {

            if (GNOME_NOISE_BASE_VOLUME.Value >= 100)
            {
                GNOME_NOISE_BASE_VOLUME.Value = 100;
            } 
            else if (GNOME_NOISE_BASE_VOLUME.Value <= 0)
            {
                GNOME_NOISE_BASE_VOLUME.Value = 0;
            }

            if (GNOME_NOISE_VOLUME_CHANGE_AMOUNT.Value >= 100)
            {
                GNOME_NOISE_VOLUME_CHANGE_AMOUNT.Value = 100;
            }
            else if (GNOME_NOISE_VOLUME_CHANGE_AMOUNT.Value <= 0)
            {
                GNOME_NOISE_VOLUME_CHANGE_AMOUNT.Value = 0;
            }

            if (GNOME_NOISE_PITCH_CHANGE_AMOUNT.Value >= 100)
            {
                GNOME_NOISE_PITCH_CHANGE_AMOUNT.Value = 100;
            }
            else if (GNOME_NOISE_PITCH_CHANGE_AMOUNT.Value <= 0)
            {
                GNOME_NOISE_PITCH_CHANGE_AMOUNT.Value = 0;
            }

            if (GNOME_NOISES_MAX_INTERVAL.Value <= 0)
            {
                GNOME_NOISES_MAX_INTERVAL.Value = 1;
            }

            if (GNOME_NOISES_MIN_INTERVAL.Value <= 0)
            {
                GNOME_NOISES_MIN_INTERVAL.Value = 1;
            }

            if (GNOME_SCRAP_RARITY.Value <= 0)
            {
                GNOME_SCRAP_RARITY.Value = 0;
                GnomePlushV2.Logger.LogWarning("ERROR: Rarity is 0 or below, no gnomes will spawn now if you're the host.");
            }

            if (GNOME_REVERB_CHANCE.Value > 100)
            {
                GnomePlushV2.Logger.LogWarning("ERROR: Max chance for reverb is capped at 100%.");
                GNOME_REVERB_CHANCE.Value = 100;
            }
            else if (GNOME_NOISES_MIN_INTERVAL.Value > GNOME_NOISES_MAX_INTERVAL.Value)
            {
                GNOME_NOISES_MAX_INTERVAL.Value = (GNOME_NOISES_MIN_INTERVAL.Value + 1);
                GnomePlushV2.Logger.LogWarning("ERROR: The max interval of the Gnome sounds can't be lower than the min Interval! \n"
                    + "In order to prevent the mod from not working at all, the max interval between gnome noises was set to the gnome min interval + 1: "
                    + (GNOME_NOISES_MIN_INTERVAL + 1));
            }
        }
    }
}
