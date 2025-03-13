using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game
{
    public class AudioPlayer //: IPool
    {
        public uint Id { get; private set; }
        public string Tag;

        public AudioSource Source { get; private set; }
        public Action<AudioPlayStatus> PlayStatusAction => playStatusAction;
        public Action NoFileAction;

        private Action<AudioPlayStatus> playStatusAction;
        private Action<AudioPlayer> audioEnd;
        private bool blAudioPlayEnd;
        private bool _isAudioPause;
        private float _fixVolume;
        private string _fileName;
        private bool _isNeedCache;
        private AudioClipType _audioClipType = AudioClipType.Old;

        public AudioPlayer(uint id, AudioSource audioSource, Action<AudioPlayer> audioEnd)
        {
            Id = id;
            Source = audioSource;
            this.audioEnd = audioEnd;
            _fixVolume = -1;
        }

        public void SetAudioId(uint id, string fileName, bool cache)
        {
            Id = id;
            Source.name = "Audio_" + id;
            _fileName = fileName;
            _isNeedCache = cache;
        }

        public void PlayAudio(AudioClip clip, bool loop, float vol = 1.0f)
        {
            if (!Source.enabled)
                Source.enabled = true;
            Source.clip = clip;
            Source.loop = loop;

            if (_fixVolume != vol && _fixVolume > 0)
            {
                Source.volume = _fixVolume;
            }
            else
            {
                Source.volume = vol;
            }
            Source.Play();
            blAudioPlayEnd = false;
        }

        public void RevisePlayingAudioVolume(float volume)
        {
            if (Source != null)
            {
                Source.volume = volume;
                _fixVolume = volume;
            }
        }

        private void OnAudioPlayEnd()
        {
            blAudioPlayEnd = true;
            playStatusAction?.Invoke(AudioPlayStatus.PlayFinished);
            //声音播放完成
            audioEnd?.Invoke(this);
        }

        public void Pause(bool pause, bool needNotify = false)
        {
            if (pause)
            {
                if (Source.isPlaying)
                {
                    Source.Pause();
                    _isAudioPause = true;
                }
            }
            else
            {
                if (!Source.isPlaying)
                {
                    Source.Play();
                    _isAudioPause = false;
                }
            }
            if(needNotify)
                playStatusAction?.Invoke(pause ? AudioPlayStatus.Pause : AudioPlayStatus.Play);
        }

        public void StopAudio(bool needNotify = false)
        {
            Source.Stop();
            if (needNotify && !blAudioPlayEnd)
                playStatusAction?.Invoke(AudioPlayStatus.Interrupt);
        }

        public void SetAction(Action<AudioPlayStatus> action)
        {
            playStatusAction = action;
        }

        public AudioPlayer SetLoop(bool loop)
        {
            if (Source != null)
                Source.loop = loop;
            return this;
        }

        public AudioPlayer SetVolume(float volume)
        {
            if (Source != null)
                Source.volume = volume;
            return this;
        }

        public AudioPlayer SetCache(bool cache)
        {
            _isNeedCache = cache;

            return this;
        }

        public AudioPlayer SetAudioClipType(AudioClipType clipType)
        {
            _audioClipType = clipType;

            return this;
        }

        public AudioPlayer OnPlayStatusAction(Action<AudioPlayStatus> statusAction)
        {
            playStatusAction = statusAction;

            return this;
        }

        public AudioPlayer OnNoFileAction(Action noFileAction)
        {
            NoFileAction = noFileAction;

            return this;
        }

        public AudioPlayer SetTag(string tag)
        {
            Tag = tag;

            return this;
        }

        public void Update()
        {
            if (Source == null || Source.clip == null || Source.loop)
                return;
            if (!Source.isPlaying && !blAudioPlayEnd && !_isAudioPause)
            {
                OnAudioPlayEnd();
            }
        }

        public void Dispose()
        {
            playStatusAction = null;
            _isAudioPause = false;
            audioEnd = null;

            if (Source == null) return;

            if (Source.clip != null)
            {
                if (_isNeedCache)
                {
                    AudioClipPool.CollectAudioClip(Source.clip, _fileName);
                }
                else
                {
                    if (_audioClipType == AudioClipType.WWWAudio)
                    {
                        Object.Destroy(Source.clip);
                    }
                }
            }

            Source.clip = null;
            UnityObjectHelper.DestroyGameObjectSafe(Source.gameObject);
            Source = null;
        }
    }

    public enum AudioClipType
    {
        Old = 0,
        AbAudio = 1,
        WWWAudio = 2
    }
}