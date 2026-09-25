using System;
using System.Collections.Generic;
using UnityEngine;

namespace ButterflyStep
{
    [Serializable]
    public class SpriteClip
    {
        [Tooltip("Nome usado pelo código e pelos estágios. Ex: Idle, Run, Walk, Fly, Attack.")]
        public string name = "Idle";
        public Sprite[] frames = new Sprite[0];
        [Min(0.1f)] public float fps = 10f;
        public bool loop = true;
    }

    public class SpriteAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private List<SpriteClip> clips = new List<SpriteClip>();
        [SerializeField] private string defaultClip = "Idle";
        [Tooltip("Para de animar enquanto a pausa do tempo estiver ativa (inimigos).")]
        [SerializeField] private bool affectedByStasis;

        private SpriteClip current;
        private string returnTo;
        private float time;

        public string CurrentName => current != null ? current.name : "";
        public bool IsPlayingOnce => returnTo != null;

        public void SetAffectedByStasis(bool value) => affectedByStasis = value;

        public void Setup(SpriteRenderer renderer, string startClip)
        {
            target = renderer;
            defaultClip = startClip;
            clips = new List<SpriteClip>();
        }

        public void AddClip(string clipName, Sprite[] frames, float fps, bool loop)
        {
            clips.Add(new SpriteClip { name = clipName, frames = frames, fps = fps, loop = loop });
        }

        private void Awake()
        {
            if (target == null) target = GetComponent<SpriteRenderer>();
            Play(defaultClip, true);
        }

        public bool Has(string clipName) => Find(clipName) != null;

        private SpriteClip Find(string clipName)
        {
            if (string.IsNullOrEmpty(clipName)) return null;
            foreach (var c in clips)
            {
                if (c.name == clipName) return c;
            }
            return null;
        }

        public void Play(string clipName, bool restart = false)
        {
            var clip = Find(clipName);
            if (clip == null || clip.frames.Length == 0) return;
            returnTo = null;
            if (clip == current && !restart) return;
            current = clip;
            time = 0f;
            ApplyFrame();
        }

        public void PlayOnce(string clipName, string thenPlay)
        {
            var clip = Find(clipName);
            if (clip == null || clip.frames.Length == 0) return;
            current = clip;
            time = 0f;
            returnTo = string.IsNullOrEmpty(thenPlay) ? defaultClip : thenPlay;
            ApplyFrame();
        }

        private void Update()
        {
            if (current == null || current.frames.Length == 0) return;
            if (affectedByStasis && TimeStasis.Active) return;
            time += UnityEngine.Time.deltaTime;
            float length = current.frames.Length / current.fps;
            if (time >= length && !current.loop)
            {
                if (returnTo != null)
                {
                    string next = returnTo;
                    returnTo = null;
                    Play(next, true);
                    return;
                }
                time = length - 0.0001f;
            }
            ApplyFrame();
        }

        private void ApplyFrame()
        {
            if (target == null || current == null || current.frames.Length == 0) return;
            int index = Mathf.FloorToInt(time * current.fps);
            index = current.loop ? index % current.frames.Length : Mathf.Min(index, current.frames.Length - 1);
            target.sprite = current.frames[index];
        }
    }
}
