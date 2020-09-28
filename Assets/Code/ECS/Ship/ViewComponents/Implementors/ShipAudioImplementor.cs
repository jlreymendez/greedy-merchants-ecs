using GreedyMerchants.Data.Audio;
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipAudioImplementor : MonoBehaviour, IImplementor, IShipAudioComponent
    {
        public AudioSource SinkClip;
        public AudioSource CoinPickClip;
        public AudioSource PirateCueClip;
        public AudioSource MerchantCueClip;

        public ShipAudioType PlayOneShot
        {
            set
            {
                switch (value)
                {
                    case ShipAudioType.Sink:
                        SinkClip.Play();
                        break;
                    case ShipAudioType.CoinPick:
                        CoinPickClip.Play();
                        break;
                    case ShipAudioType.PirateCue:
                        PirateCueClip.Play();
                        break;
                    case ShipAudioType.MerchantCue:
                        MerchantCueClip.Play();
                        break;
                }
            }
        }
    }
}