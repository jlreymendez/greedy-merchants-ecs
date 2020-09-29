using UTime = UnityEngine.Time;

namespace GreedyMerchants.ECS.Unity
{
    public class Time : ITime
    {
        public float TimeScale { get => UTime.timeScale; set => UTime.timeScale = value; }
        public float DeltaTime { get => UTime.deltaTime; }
    }

    public interface ITime
    {
        float TimeScale { get; set; }
        float DeltaTime { get; }
    }
}