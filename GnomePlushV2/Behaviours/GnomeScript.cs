using System;
using Unity.Netcode;
using UnityEngine;

namespace GnomePlushV2.Behaviours
{
    public class GnomeScript : GrabbableObject
    {
        private static int gnomeCount = 0;

        public AudioClip gnomeSound;
        public AudioClip gnomeSoundReverb;
        public AudioSource gnomeAudioSource;

        private const float PITCH_MULTIPLIER = 0.85f;
        private const float GNOME_NOISE_WALKIE_VOLUME_OFFSET = 0.25f;
        private const float GNOME_NOISE_BASE_PITCH = 1f;
        private const float GNOME_NOISE_RANGE = 8f;
        public readonly float BIG_GNOME_WIDTH = 1.65f;
        public readonly float BIG_GNOME_GRABBING_OFFSET = 1f;

        private float randomNumberPitchChange;
        private float gnomeSoundInterval;
        private bool isGnomeSoundIntervalSet = false;
        internal static System.Random randomNoise = new System.Random(400);

        private float randomScale = 1f;
        private int randomIndex;
        private int[] tinySizes = [3, 3, 5, 6, 7];
        private int[] defaultSizes = [10, 10, 12, 13, 14];
        private int[] bigSizes = [18, 18, 20, 24, 37];

        public override void Start()
        {
            int randomNumber = 50;
            int totalChanceValue = GnomePlushV2.gnomeConfig.TINY_GNOME_SIZE_CHANCE.Value + GnomePlushV2.gnomeConfig.DEFAULT_GNOME_SIZE_CHANCE.Value + GnomePlushV2.gnomeConfig.BIG_GNOME_SIZE_CHANCE.Value;
            float tinyChance = (GnomePlushV2.gnomeConfig.TINY_GNOME_SIZE_CHANCE.Value / (float)totalChanceValue) * 100;
            float defaultChance = (GnomePlushV2.gnomeConfig.DEFAULT_GNOME_SIZE_CHANCE.Value / (float)totalChanceValue) * 100;

            gnomeCount++;
            randomNoise = new System.Random(400 + (gnomeCount));

            //only server sets size
            if (IsHost)
            {
                randomNumber = GnomePlushV2.randomSize.Next(1, 100);
                randomIndex = GnomePlushV2.randomSize.Next(0, 5);

                if (randomNumber <= tinyChance)
                {
                    randomScale = tinySizes[randomIndex];
                }
                else if (randomNumber <= tinyChance + defaultChance)
                {
                    randomScale = defaultSizes[randomIndex];
                }
                else
                {
                    randomScale = bigSizes[randomIndex];
                }

                GnomePlushV2.Logger.LogInfo("randomScale " + randomScale);
                GnomePlushV2.Logger.LogInfo("- - - - - - - - - - - -");

                randomScale *= GnomePlushV2.gnomeConfig.EXPERIMENTAL_SIZE_MULTIPLIER;
                randomScale /= 10;
            }

            //client rpc takes size information from server
            SetGnomeSizeClientRpc(randomScale);

            base.Start();
        }

        public override void Update()
        {
            base.Update();

            if (this.IsSpawned && NetworkManager.Singleton.IsServer)
            {
                //only server controls the timing
                if (!isGnomeSoundIntervalSet)
                {
                    int minInterval = (int)(0.5f * (104 - GnomePlushV2.gnomeConfig.GNOME_NOISES_FREQUENCY));
                    int maxInterval = (int)(1.3f * (104 - GnomePlushV2.gnomeConfig.GNOME_NOISES_FREQUENCY));
                    gnomeSoundInterval = randomNoise.Next(minInterval * 100, maxInterval * 100);
                    gnomeSoundInterval /= 100;
                    isGnomeSoundIntervalSet = true;
                }

                gnomeSoundInterval -= Time.deltaTime;

                if (gnomeSoundInterval < 0)
                {
                    PlayGnomeSound(CalcRandomGnomePitch());
                    isGnomeSoundIntervalSet = false;
                }
            }
        }

        private float CalcRandomGnomePitch()
        {

            float gnomePitch = GNOME_NOISE_BASE_PITCH;

            float randomPitchChangeAmount = GnomePlushV2.gnomeConfig.GNOME_NOISE_PITCH_CHANGE_AMOUNT.Value;
            float randPitchMin = 100 - (int)(randomPitchChangeAmount * PITCH_MULTIPLIER);               //less probability of lower noises, bc I just don't like the lower whoos that much
            float randPitchMax = 100 + randomPitchChangeAmount;                         

            if (randPitchMin < 0)
            {
                randPitchMin = 0;
            }
            randomNumberPitchChange = randomNoise.Next((int) randPitchMin, (int) randPitchMax);
            randomNumberPitchChange /= 100;

            gnomePitch /= Math.Clamp((randomScale - 1), 1f, 1.3f);
            gnomePitch *= randomNumberPitchChange;

            return gnomePitch;
        }


        private void PlayGnomeSound(float randomPitch)
        {
            int gnomeVolume = GnomePlushV2.gnomeConfig.GNOME_NOISE_BASE_VOLUME.Value / 100;
            int randomChance = randomNoise.Next(0, 100);

            if (randomChance < GnomePlushV2.gnomeConfig.GNOME_REVERB_CHANCE.Value)
            {
                PlayGnomeReverbSoundClientRpc(randomPitch);
                WalkieTalkie.TransmitOneShotAudio(gnomeAudioSource, gnomeSoundReverb, gnomeVolume - GNOME_NOISE_WALKIE_VOLUME_OFFSET);
            }
            else
            {
                PlayGnomeSoundClientRpc(randomPitch);
                WalkieTalkie.TransmitOneShotAudio(gnomeAudioSource, gnomeSound, gnomeVolume - GNOME_NOISE_WALKIE_VOLUME_OFFSET);
            }

            if (GnomePlushV2.gnomeConfig.CAN_GNOME_ANGER_DOGS)
            {
                RoundManager.Instance.PlayAudibleNoise(base.transform.position, GNOME_NOISE_RANGE, gnomeVolume, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed, 69420);
            }
        }

        [ClientRpc]
        public void PlayGnomeSoundClientRpc(float gnomePitch)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager == null || !networkManager.IsListening)
            {
                return;
            }
            if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
            {
                ClientRpcParams clientRpcParams = default(ClientRpcParams);
                FastBufferWriter bufferWriter = __beginSendClientRpc(152346789u, clientRpcParams, RpcDelivery.Reliable);
                BytePacker.WriteValuePacked(bufferWriter, gnomePitch);
                __endSendClientRpc(ref bufferWriter, 152346789u, clientRpcParams, RpcDelivery.Reliable);
            }
            if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
            {
                ChangePitch(gnomePitch);
                gnomeAudioSource.PlayOneShot(gnomeSound);
            }
        }

        [ClientRpc]
        public void PlayGnomeReverbSoundClientRpc(float gnomePitch)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager == null || !networkManager.IsListening)
            {
                return;
            }

            if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
            {
                ClientRpcParams clientRpcParams = default(ClientRpcParams);
                FastBufferWriter bufferWriter = __beginSendClientRpc(623146789u, clientRpcParams, RpcDelivery.Reliable);
                BytePacker.WriteValuePacked(bufferWriter, gnomePitch);
                __endSendClientRpc(ref bufferWriter, 623146789u, clientRpcParams, RpcDelivery.Reliable);
            }

            if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
            {
                ChangePitch(gnomePitch);
                gnomeAudioSource.PlayOneShot(gnomeSoundReverb);
            }
        }

        [ClientRpc]
        public void SetGnomeSizeClientRpc(float gnomeSize)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager == null || !networkManager.IsListening)
            {
                return;
            }

            if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
            {
                ClientRpcParams clientRpcParams = default(ClientRpcParams);
                FastBufferWriter bufferWriter = __beginSendClientRpc(178238291u, clientRpcParams, RpcDelivery.Reliable);
                BytePacker.WriteValuePacked(bufferWriter, gnomeSize);
                __endSendClientRpc(ref bufferWriter, 178238291u, clientRpcParams, RpcDelivery.Reliable);
            }

            if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
            {
                if (gnomeSize > 2.5f)
                {
                    this.transform.localScale = new Vector3(gnomeSize * BIG_GNOME_WIDTH, gnomeSize, gnomeSize * BIG_GNOME_WIDTH);
                    this.itemProperties.positionOffset = new Vector3(this.itemProperties.positionOffset.x + BIG_GNOME_GRABBING_OFFSET, this.itemProperties.positionOffset.y, this.itemProperties.positionOffset.z);
                }
                else
                {
                    this.transform.localScale = Vector3.one * gnomeSize;
                }
            }
        }

        public static void __rpc_handler_152346789(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
        {
            NetworkManager networkManager = target.NetworkManager;

            if (networkManager != null && networkManager.IsListening)
            {
                ByteUnpacker.ReadValuePacked(reader, out float pitchValue);

                ((GnomeScript)target).__rpc_exec_stage = __RpcExecStage.Client;
                ((GnomeScript)target).ChangePitch(pitchValue);
                ((GnomeScript)target).gnomeAudioSource.PlayOneShot(((GnomeScript)target).gnomeSound);
                ((GnomeScript)target).__rpc_exec_stage = __RpcExecStage.None;
            }
            else
            {
                Debug.LogError("NetworkManager is not listening or null in RPC handler");
            }
        }

        public static void __rpc_handler_623146789(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
        {
            NetworkManager networkManager = target.NetworkManager;

            if (networkManager != null && networkManager.IsListening)
            {
                ByteUnpacker.ReadValuePacked(reader, out float pitchValue);

                ((GnomeScript)target).__rpc_exec_stage = __RpcExecStage.Client;
                ((GnomeScript)target).ChangePitch(pitchValue);
                ((GnomeScript)target).gnomeAudioSource.PlayOneShot(((GnomeScript)target).gnomeSoundReverb);
                ((GnomeScript)target).__rpc_exec_stage = __RpcExecStage.None;
            }
            else
            {
                Debug.LogError("NetworkManager is not listening or null in RPC handler");
            }
        }

        public static void __rpc_handler_178238291(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
        {
            NetworkManager networkManager = target.NetworkManager;

            if (networkManager != null && networkManager.IsListening)
            {
                ByteUnpacker.ReadValuePacked(reader, out float gnomeSize);

                ((GnomeScript)target).__rpc_exec_stage = __RpcExecStage.Client;
                if (gnomeSize > 2.5f)
                {
                    ((GnomeScript)target).transform.localScale = new Vector3(gnomeSize * ((GnomeScript)target).BIG_GNOME_WIDTH, gnomeSize, gnomeSize * ((GnomeScript)target).BIG_GNOME_WIDTH);
                }
                else
                {
                    ((GnomeScript)target).transform.localScale = Vector3.one * gnomeSize;
                }
                ((GnomeScript)target).__rpc_exec_stage = __RpcExecStage.None;
            }
            else
            {
                Debug.LogError("NetworkManager is not listening or null in RPC handler");
            }
        }

        [RuntimeInitializeOnLoadMethod]
        internal static void InitializeRPCS_GnomeScript()
        {
            NetworkManager.__rpc_func_table.Add(152346789u, __rpc_handler_152346789);
            NetworkManager.__rpc_func_table.Add(623146789u, __rpc_handler_623146789);
            NetworkManager.__rpc_func_table.Add(178238291u, __rpc_handler_178238291);
        }

        private void ChangePitch(float targetPitch)
        {
            gnomeAudioSource.dopplerLevel = 1f;
            gnomeAudioSource.pitch = targetPitch;
        }

        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                Debug.Log("Client has spawned and is ready.");
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
