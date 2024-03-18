using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GnomePlushV2.Behaviours
{
    internal class GnomeScript : GrabbableObject
    {
        public AudioClip gnomeSound;
        public AudioClip gnomeSoundReverb;
        public AudioSource gnomeAudioSource;

        private const float PITCH_MULTIPLIER = 0.85f;
        private const float GNOME_NOISE_WALKIE_VOLUME_OFFSET = 0.3f;
        private const float GNOME_NOISE_BASE_PITCH = 1f;
        private const float GNOME_NOISE_RANGE = 8f;

        public bool randomizeVolume = true;
        public float gnomeVolume;
        private float randomNumberVolumeChange;
        public float gnomePitch = 1f;
        private float randomNumberPitchChange;
        private float gnomeSoundInterval;
        private bool isGnomeSoundIntervalSet = false;
        private int gamblingForReverb;

        public override void Update()
        {
            base.Update();

            if (GnomeConfig.Instance.IS_GNOME_NOISE_ENABLED)
            {
                if (!isGnomeSoundIntervalSet)
                {
                    int minInterval = GnomeConfig.Instance.GNOME_NOISES_MIN_INTERVAL;
                    int maxInterval = GnomeConfig.Instance.GNOME_NOISES_MAX_INTERVAL;
                    gnomeSoundInterval = UnityEngine.Random.Range(minInterval, maxInterval);
                    isGnomeSoundIntervalSet = true;
                }

                gnomeSoundInterval -= Time.deltaTime;

                if (gnomeSoundInterval <= 0)
                {
                    CalcRandomNumbersForGnomeSound();
                    PlayGnomeSound(gnomeAudioSource);

                    isGnomeSoundIntervalSet = false;
                }
            }
        }

        //Calculates the random volume and random pitch
        private void CalcRandomNumbersForGnomeSound()
        {
            ResetRandomNumbersForGnomeSound();

            gnomeVolume = GnomeConfig.Instance.GNOME_NOISE_BASE_VOLUME / 100f;
            gnomePitch = GNOME_NOISE_BASE_PITCH;

            int randomVolChangeAmount = GnomeConfig.Instance.GNOME_NOISE_VOLUME_CHANGE_AMOUNT.Value;
            int randVolMin = 100 - randomVolChangeAmount;
            int randVolMax = 100 + randomVolChangeAmount;

            randomNumberVolumeChange = UnityEngine.Random.Range(randVolMin, randVolMax);
            randomNumberVolumeChange /= 100;
            gnomeVolume *= randomNumberVolumeChange;

            GnomePlushV2.Logger.LogDebug($"changed volume to: {gnomeVolume}, " +
                $"volume change amount was: {randomNumberVolumeChange * 100}%");

            int randomPitchChangeAmount = GnomeConfig.Instance.GNOME_NOISE_PITCH_CHANGE_AMOUNT.Value;
            int randPitchMin = 100 - (int)(randomPitchChangeAmount * PITCH_MULTIPLIER);               //less probability of lower noises, bc I just don't like the lower whoos that much
            int randPitchMax = 100 + randomPitchChangeAmount;                         

            if (randPitchMin < 0)
            {
                randPitchMin = 0;
            }

            randomNumberPitchChange = UnityEngine.Random.Range(randPitchMin, randPitchMax);
            randomNumberPitchChange /= 100;
            gnomePitch *= randomNumberPitchChange;

            GnomePlushV2.Logger.LogDebug($"changed pitch to: {gnomePitch}, " +
                $"pitch change amount was: {randomNumberPitchChange * 100}%");
        }


        //Plays a gnome sound
        private void PlayGnomeSound(AudioSource gnomeAudioSource)
        {
            gnomeAudioSource.volume = gnomeVolume;
            gnomeAudioSource.dopplerLevel = 1f;                 //for some reason this NEEDS to be on in order for the pitch to work
                                                                //my guess is that LethalLibs FixMixerGroup resets this to 0 or smth

            gamblingForReverb = UnityEngine.Random.Range(1, 100);
            if (gamblingForReverb <= GnomeConfig.Instance.GNOME_REVERB_CHANCE)
            {
                gnomeAudioSource.pitch = gnomePitch;
                gnomeAudioSource.PlayOneShot(gnomeSoundReverb);
                WalkieTalkie.TransmitOneShotAudio(gnomeAudioSource, gnomeSoundReverb, gnomeVolume - GNOME_NOISE_WALKIE_VOLUME_OFFSET);
            }
            else
            {
                gnomeAudioSource.pitch = gnomePitch;
                gnomeAudioSource.PlayOneShot(gnomeSound);
                WalkieTalkie.TransmitOneShotAudio(gnomeAudioSource, gnomeSound, gnomeVolume - GNOME_NOISE_WALKIE_VOLUME_OFFSET);
            }

            if (GnomeConfig.Instance.CAN_GNOME_ANGER_DOGS)
            {
                RoundManager.Instance.PlayAudibleNoise(base.transform.position, GNOME_NOISE_RANGE, gnomeVolume, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed, 69420);
            }
        }

        //Resets some random values to their base values, bc they are multiplied in CalcRandomNumbersForGnomeSound()
        private void ResetRandomNumbersForGnomeSound()
        {
            gnomeVolume = GnomeConfig.Instance.GNOME_NOISE_BASE_VOLUME / 100f;
            gnomePitch = GNOME_NOISE_BASE_PITCH;
            gnomeAudioSource.pitch = GNOME_NOISE_BASE_PITCH;
        }

        public override void EquipItem()
        {
            base.EquipItem();
        }

        public override void __initializeVariables()
        {
            base.__initializeVariables();
        }

        public override string __getTypeName()
        {
            return "GnomeItem";
        }
    }
}
