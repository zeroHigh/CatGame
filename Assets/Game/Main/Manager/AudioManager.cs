using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class AudioManager : Singleton<AudioManager>
    {
        private AudioSource audioSource;

        // 音频剪辑缓存
        private Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();

        // 播放完成事件
        public UnityEvent onAudioFinished; // 可以通过Inspector绑定事件
        private Action onAudioFinishedCallback; // 动态回调

        public void Init(GameObject gameObj)
        {
            // 初始化音频源
            audioSource = gameObj.AddComponent<AudioSource>();
            // 初始化播放完成事件
            onAudioFinished = new UnityEvent();
        }

        public void Update()
        {
            // 检测音频是否播放完成
            if (audioSource.clip != null && !audioSource.isPlaying && audioSource.time >= audioSource.clip.length)
            {
                OnAudioFinished();
            }
        }

        /// <summary>
        /// 加载音频剪辑
        /// </summary>
        /// <param name="path">音频文件路径（相对于Resources文件夹）</param>
        /// <returns>加载的音频剪辑</returns>
        private AudioClip LoadAudioClip(string path)
        {
            if (audioClipCache.ContainsKey(path))
            {
                return audioClipCache[path];
            }

            AudioClip clip = Resources.Load<AudioClip>(path);
            if (clip != null)
            {
                audioClipCache[path] = clip;
                return clip;
            }
            else
            {
                Debug.LogError($"Failed to load audio clip at path: {path}");
                return null;
            }
        }

        /// <summary>
        /// 播放音频
        /// </summary>
        /// <param name="path">音频文件路径（相对于Resources文件夹）</param>
        /// <param name="loop">是否循环播放</param>
        /// <param name="onFinished">播放完成回调</param>
        public void Play(string path, bool loop = false, Action onFinished = null)
        {
            AudioClip clip = LoadAudioClip(path);
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.loop = loop;
                audioSource.Play();

                // 设置播放完成回调
                onAudioFinishedCallback = onFinished;
            }
        }

        /// <summary>
        /// 暂停播放
        /// </summary>
        public void Pause()
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }

        /// <summary>
        /// 继续播放
        /// </summary>
        public void Resume()
        {
            if (!audioSource.isPlaying)
            {
                audioSource.UnPause();
            }
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void Stop()
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
                OnAudioFinished(); // 停止时也触发完成事件
            }
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="volume">音量（0到1之间）</param>
        public void SetVolume(float volume)
        {
            audioSource.volume = Mathf.Clamp(volume, 0f, 1f);
        }

        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool IsPlaying()
        {
            return audioSource.isPlaying;
        }

        /// <summary>
        /// 音频播放完成时调用
        /// </summary>
        private void OnAudioFinished()
        {
            // 触发UnityEvent事件
            onAudioFinished.Invoke();

            // 触发动态回调
            onAudioFinishedCallback?.Invoke();

            // 清空回调
            onAudioFinishedCallback = null;
        }

        /// <summary>
        /// 释放指定路径的音频资源
        /// </summary>
        /// <param name="path">音频文件路径（相对于Resources文件夹）</param>
        public void ReleaseAudioClip(string path)
        {
            if (audioClipCache.ContainsKey(path))
            {
                AudioClip clip = audioClipCache[path];
                if (clip != null)
                {
                    Resources.UnloadAsset(clip); // 释放音频资源
                }
                audioClipCache.Remove(path); // 从缓存中移除
                Debug.Log($"Released audio clip: {path}");
            }
            else
            {
                Debug.LogWarning($"Audio clip not found in cache: {path}");
            }
        }

        /// <summary>
        /// 释放所有缓存的音频资源
        /// </summary>
        public void ReleaseAllAudioClips()
        {
            foreach (var pair in audioClipCache)
            {
                AudioClip clip = pair.Value;
                if (clip != null)
                {
                    Resources.UnloadAsset(clip); // 释放音频资源
                }
            }
            audioClipCache.Clear(); // 清空缓存
            Debug.Log("Released all audio clips.");
        }

    }
}