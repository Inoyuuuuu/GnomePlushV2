using System;
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

        private System.Random noiseRandom = new System.Random(333);

        private float randomScale;
        private float bigGnomeWidth = 1.70f;

        public override void Start()
        {
            base.Start();
            int randomNumber = GnomePlushV2.random.Next(1, 100);
            GnomePlushV2.Logger.LogInfo(randomNumber);

            float totalChanceValue = GnomePlushV2.gnomeConfig.TINY_GNOME_SIZE_CHANCE.Value + GnomePlushV2.gnomeConfig.DEFAULT_GNOME_SIZE_CHANCE.Value + GnomePlushV2.gnomeConfig.BIG_GNOME_SIZE_CHANCE.Value;
            float defaultChance = (GnomePlushV2.gnomeConfig.DEFAULT_GNOME_SIZE_CHANCE.Value / totalChanceValue) * 100;
            float bigChance = (GnomePlushV2.gnomeConfig.BIG_GNOME_SIZE_CHANCE.Value / totalChanceValue) * 100;

            GnomePlushV2.Logger.LogInfo("defCh:" + defaultChance);
            GnomePlushV2.Logger.LogInfo("bigCh:" + bigChance);

            if (randomNumber <= defaultChance)
            {
                randomScale = GnomePlushV2.random.Next(9, 15);
            } else if (randomNumber > defaultChance && randomNumber <= defaultChance + bigChance)
            {
                randomScale = GnomePlushV2.random.Next(16, 35);
            } else
            {
                randomScale = (GnomePlushV2.random.Next(3, 8));
            }

            randomScale /= 10;

            GnomePlushV2.Logger.LogInfo("rs: " + randomScale);
        }

        public override void Update()
        {
            base.Update();

            if (randomScale > 2.5f)
            {
                this.transform.localScale = new Vector3(randomScale * bigGnomeWidth, randomScale, randomScale * bigGnomeWidth);
            }
            else
            {
                this.transform.localScale = Vector3.one * randomScale;
            }

            if (GnomePlushV2.gnomeConfig.IS_GNOME_NOISE_ENABLED)
            {
                if (!isGnomeSoundIntervalSet)
                {
                    int minInterval = (int) (0.5 * 101 - GnomePlushV2.gnomeConfig.GNOME_NOISES_FREQUENCY);
                    int maxInterval = (int)(1.3 * 101 - GnomePlushV2.gnomeConfig.GNOME_NOISES_FREQUENCY);
                    gnomeSoundInterval = noiseRandom.Next(minInterval, maxInterval);
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

            gnomeVolume = GnomePlushV2.gnomeConfig.GNOME_NOISE_BASE_VOLUME / 100f;
            gnomePitch = GNOME_NOISE_BASE_PITCH;

            int randomVolChangeAmount = GnomePlushV2.gnomeConfig.GNOME_NOISE_VOLUME_CHANGE_AMOUNT.Value;
            int randVolMin = 100 - randomVolChangeAmount;
            int randVolMax = 100 + randomVolChangeAmount;

            randomNumberVolumeChange = noiseRandom.Next(randVolMin, randVolMax);
            randomNumberVolumeChange /= 100;
            gnomeVolume *= randomNumberVolumeChange;

            int randomPitchChangeAmount = GnomePlushV2.gnomeConfig.GNOME_NOISE_PITCH_CHANGE_AMOUNT.Value;
            int randPitchMin = 100 - (int)(randomPitchChangeAmount * PITCH_MULTIPLIER);               //less probability of lower noises, bc I just don't like the lower whoos that much
            int randPitchMax = 100 + randomPitchChangeAmount;                         

            if (randPitchMin < 0)
            {
                randPitchMin = 0;
            }

            randomNumberPitchChange = noiseRandom.Next(randPitchMin, randPitchMax);
            randomNumberPitchChange /= 100;

            gnomePitch /= Math.Clamp((randomScale - 1), 1f, 1.3f);

            gnomePitch *= randomNumberPitchChange;
        }


        //Plays a gnome sound
        private void PlayGnomeSound(AudioSource gnomeAudioSource)
        {
            gnomeAudioSource.volume = gnomeVolume;
            gnomeAudioSource.dopplerLevel = 1f;                 //for some reason this NEEDS to be on in order for the pitch to work
                                                                //my guess is that LethalLibs FixMixerGroup resets this to 0 or smth

            gamblingForReverb = noiseRandom.Next(1, 100);
            if (gamblingForReverb <= GnomePlushV2.gnomeConfig.GNOME_REVERB_CHANCE)
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

            if (GnomePlushV2.gnomeConfig.CAN_GNOME_ANGER_DOGS)
            {
                RoundManager.Instance.PlayAudibleNoise(base.transform.position, GNOME_NOISE_RANGE, gnomeVolume, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed, 69420);
            }
        }

        //Resets some random values to their base values, bc they are multiplied in CalcRandomNumbersForGnomeSound()
        private void ResetRandomNumbersForGnomeSound()
        {
            gnomeVolume = GnomePlushV2.gnomeConfig.GNOME_NOISE_BASE_VOLUME / 100f;
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
