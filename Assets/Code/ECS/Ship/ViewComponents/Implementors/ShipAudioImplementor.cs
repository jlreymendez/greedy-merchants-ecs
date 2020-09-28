using GreedyMerchants.Data.Audio;
using UnityEngine;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipAudioImplementor : MonoBehaviour, IShipAudioComponent
    {
        public AudioClip SinkClip;
        public AudioClip CoinPickClip;
        public AudioClip PirateCueClip;
        public AudioClip MerchantCueClip;

        AudioSource _audioSource;

        public ShipAudioType PlayOneShot
        {
            set
            {
                switch (value)
                {
                    case ShipAudioType.Sink:
                        _audioSource.clip = SinkClip;
                        _audioSource.Play();
                        break;
                    case ShipAudioType.CoinPick:
                        _audioSource.clip = CoinPickClip;
                        _audioSource.Play();
                        break;
                    case ShipAudioType.PirateCue:
                        _audioSource.clip = PirateCueClip;
                        _audioSource.Play();
                        break;
                    case ShipAudioType.MerchantCue:
                        _audioSource.clip = MerchantCueClip;
                        _audioSource.Play();
                        break;
                }
            }
        }

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }
    }
}