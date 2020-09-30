using UTime = UnityEngine.Time;

namespace GreedyMerchants.ECS.Unity
{
    public class Time : ITime
    {
        public float CurrentTime => UTime.time;
        public float TimeScale { get => UTime.timeScale; set => UTime.timeScale = value; }
        public float DeltaTime { get => UTime.deltaTime; }
    }

    public interface ITime
    {
        float CurrentTime { get; }
        float TimeScale { get; set; }
        float DeltaTime { get; }
    }
}