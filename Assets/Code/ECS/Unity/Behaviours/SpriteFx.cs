using System.Collections;
using Svelto.Tasks.Enumerators;
using UnityEngine;

namespace GreedyMerchants.ECS.Unity
{
    public class SpriteFx : MonoBehaviour
    {
        public SpriteRenderer Renderer;
        public float SecondsPerFrame;
        public Sprite[] Sprites;

        Coroutine _animation;
        WaitForSecondsEnumerator _frameWait;

        public void Play()
        {
            Stop();
            _animation = StartCoroutine(Animate());
        }

        public void Stop()
        {
            if (_animation != null)
            {
                StopCoroutine(_animation);
            }

            Renderer.enabled = false;
        }

        void Awake()
        {
            _frameWait = new WaitForSecondsEnumerator(SecondsPerFrame);
        }

        IEnumerator Animate()
        {
            Renderer.enabled = true;
            var frame = 0;
            while (frame < Sprites.Length)
            {
                Renderer.sprite = Sprites[frame];
                yield return _frameWait;
                frame++;
            }
            Renderer.enabled = false;
        }
    }
}