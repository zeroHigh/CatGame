using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public static class AudioClipPool
    {
        private static Dictionary<string, Queue<AudioClip>> _dictAudioClipPool;
        private static int Pool_Len = 5;

        public static void InitPool()
        {
            _dictAudioClipPool = new Dictionary<string, Queue<AudioClip>>();
        }

        public static void LoadAudioClip(string file, Action<AudioClip> action)
        {
            if (string.IsNullOrEmpty(file))
            {
                action?.Invoke(null);
                return;
            }

            if (_dictAudioClipPool.TryGetValue(file, out var queue) && queue.Count > 0)
            {
                action?.Invoke(queue.Dequeue());
            }
            else
            {
                ResourceLoader.Instance.LoadAudioClip(file, action);
            }
        }

        public static void CollectAudioClip(AudioClip clip, string file)
        {
            if (_dictAudioClipPool.TryGetValue(file, out var queue))
            {
                if (queue.Count > Pool_Len)
                {
                    GameObject.Destroy(clip);
                    return;
                }
                queue.Enqueue(clip);
            }
            else
            {
                queue = new Queue<AudioClip>();
                queue.Enqueue(clip);
                _dictAudioClipPool.Add(file, queue);
            }
        }

        public static void ReleasePool()
        {
            if (_dictAudioClipPool == null)
                return;
            foreach (var kv in _dictAudioClipPool)
            {
                while (kv.Value.Count > 0)
                {
                    GameObject.Destroy(kv.Value.Dequeue());
                }
            }
            _dictAudioClipPool.Clear();
        }

        public static void Dispose()
        {
            ReleasePool();
            _dictAudioClipPool = null;
        }
    }
}