using GreedyMerchants.Data.Audio;

namespace GreedyMerchants.ECS.Ship
{
    public interface IShipAudioComponent
    {
        ShipAudioType PlayOneShot { set; }
    }
}