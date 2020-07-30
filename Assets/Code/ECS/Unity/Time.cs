namespace GreedyMerchants.ECS.Unity
{
    public class Time : ITime
    {
        public float DeltaTime { get => UnityEngine.Time.deltaTime; }
    }

    public interface ITime
    {
        float DeltaTime { get; }
    }
}