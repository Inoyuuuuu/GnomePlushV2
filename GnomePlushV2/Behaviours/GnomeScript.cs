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
        public int gnomeSoundsMaxInterval = 130; //seconds
        public int gnomeSoundsMinInterval = 50;  //seconds
        public int gnomeReverbSoundChance = 10;  //in percent
        public float gnomeVolume = 0.5f;

        //public bool randomizeVolume = true;
        //public float randomVolChangeAmount = 0.4f;

        private float gnomeSoundsInterval;
        private bool isGnomeSoundsSet;
        private int randomNumberSoundInterval;
        //private int randomNumberVolumeChange;

        public void Awake()
        {
            gnomeAudioSource.volume = gnomeVolume;
            isGnomeSoundsSet = false;
        }

        public override void Update()
        {
            base.Update();
            if (!isGnomeSoundsSet)
            {
                gnomeSoundsInterval = UnityEngine.Random.Range(gnomeSoundsMinInterval, gnomeSoundsMaxInterval);
                isGnomeSoundsSet = true;
            }

            gnomeSoundsInterval -= Time.deltaTime;

            if (gnomeSoundsInterval <= 0)
            {
                //randomize volume
                //if (randomizeVolume)
                //{
                //    randomNumberVolumeChange = UnityEngine.Random.Range(-(int)(randomVolChangeAmount * 10), (int)(randomVolChangeAmount * 10));
                //    gnomeAudioSource.volume = gnomeVolume + (float)randomNumberVolumeChange / 10;
                //    GnomePlushBase.mls.LogInfo("changed volume to: " + gnomeAudioSource.volume.ToString() + ", change amount was: "
                //        + (gnomeVolume + (float)randomNumberVolumeChange / 10).ToString());
                //}

                //play sound
                randomNumberSoundInterval = UnityEngine.Random.Range(1, 100);
                if (randomNumberSoundInterval <= gnomeReverbSoundChance)
                {
                    gnomeAudioSource.PlayOneShot(gnomeSoundReverb);
                }
                else
                {
                    gnomeAudioSource.PlayOneShot(gnomeSound);
                }
                isGnomeSoundsSet = false;

                //reset volume
                //if (randomizeVolume)
                //{
                //    gnomeAudioSource.volume = gnomeVolume;
                //}
            }
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
