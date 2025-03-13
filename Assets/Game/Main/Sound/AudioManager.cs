using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 音频播放状态
    /// </summary>
    public enum AudioPlayStatus
    {
        Play,
        Pause,
        Interrupt,
        PlayFinished,
    }

    public class AudioManager : ILSingleton<AudioManager>
    {
        private uint soundIndex = 1;
        private Transform audioRoot;
        //当前正在播放中的音频文件
        private Dictionary<uint, AudioPlayer> dictAudioPlayers;
        private Dictionary<uint, AudioPlayer> dictBackGroundAudioPlayer;



        public void Init(Transform audioRoot)
        {
            dictAudioPlayers = new Dictionary<uint, AudioPlayer>();
            dictBackGroundAudioPlayer = new Dictionary<uint, AudioPlayer>();
            this.audioRoot = audioRoot;
            AudioClipPool.InitPool();
        }

        public uint PlayBackGround(string fileName, Action<AudioPlayStatus> playStatusAction = null, float volume = 1f, Action noFileAction=null, bool cache = false)
        {
            var player = CreateAudioPlayer(fileName, true, playStatusAction, volume, noFileAction, cache);
            dictBackGroundAudioPlayer[player.Id] = player;
            return player.Id;
        }

        public AudioPlayer CreateAudioPlayer(string fileName, bool loop = false, Action<AudioPlayStatus> playStatusAction = null, float volume = 1f,Action noFileAction=null, bool cache = false, Action<AudioPlayer> readyToPlayAction = null)
        {
            var player = CreateAudioObject();
            player.SetAudioId(soundIndex, fileName, cache);
            soundIndex++;
            AudioClipPool.LoadAudioClip(fileName, (clip) =>
            {
                if (clip == null)
                {
                    Logger.Log("[AudioManager.PlayAudio() => 音频加载失败，fileName:" + fileName + "]");
                    StopAudio(player.Id);

                    playStatusAction?.Invoke(AudioPlayStatus.PlayFinished);
                    player.PlayStatusAction?.Invoke(AudioPlayStatus.PlayFinished);

                    noFileAction?.Invoke();
                    player.NoFileAction?.Invoke();

                    return;
                }

                if (playStatusAction != null)
                {
                    player.SetAction(playStatusAction);
                }

                try
                {
                    player.PlayAudio(clip, loop, volume);
                    readyToPlayAction?.Invoke(player);
                }
                catch (NullReferenceException e)
                {
                    Debug.Log($"音频还没播放起来，就被stop：{e}");
                }
            });
            return player;
        }

        public uint PlayAudio(string fileName, bool loop = false,  Action<AudioPlayStatus> playStatusAction = null, float volume = 1f,Action noFileAction=null, bool cache = false, Action<AudioPlayer> readyToPlayAction = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                playStatusAction?.Invoke(AudioPlayStatus.PlayFinished);
                return 0;
            }

            var player = CreateAudioPlayer(fileName, loop, playStatusAction, volume, noFileAction, cache,readyToPlayAction);
            dictAudioPlayers[player.Id] = player;
            return player.Id;
        }

        public void PlayAudioEndOfFrame(MonoBehaviour mono, string fileName, bool loop = false,  Action<AudioPlayStatus> playStatusAction = null, float volume = 1f,Action noFileAction=null, bool cache = false)
        {
            mono.StartCoroutine(_PlayAudioEndOfFrame(fileName, loop, playStatusAction, volume, noFileAction, cache));
        }

        private IEnumerator _PlayAudioEndOfFrame(string fileName, bool loop = false,  Action<AudioPlayStatus> playStatusAction = null, float volume = 1f,Action noFileAction=null, bool cache = false)
        {
            yield return new WaitForEndOfFrame();
            PlayAudio(fileName, loop, playStatusAction, volume, noFileAction, cache);
        }

        public AudioPlayer PlayFile(string fileName)
        {
            var player = CreateAudioPlayer(fileName);
            dictAudioPlayers[player.Id] = player;
            return player;
        }

        public void RevisePlayingAudioVolume( uint id,float volume)
        {
            if (dictBackGroundAudioPlayer.ContainsKey(id))
            {
                dictBackGroundAudioPlayer[id].RevisePlayingAudioVolume(volume);
            }

            if (dictAudioPlayers.ContainsKey(id))
            {
                dictAudioPlayers[id].RevisePlayingAudioVolume(volume);
            }
        }

        public void Pause(uint id, bool pause, bool needNotify = false)
        {
            var player = GetAudioPlayer(id);
            player?.Pause(pause, needNotify);
        }

        public void StopAudio(uint id, bool needNotify = false)
        {
            if (!dictAudioPlayers.ContainsKey(id) && !dictBackGroundAudioPlayer.ContainsKey(id))
            {
                Logger.Log("[AudioManager.StopAudio() => 停止声音失败，找不到AudioPlayer, id:" + id + "]");
                return;
            }

            var player = GetAudioPlayer(id);
            if (player == null)
                return;
            if (dictAudioPlayers.ContainsKey(id))
                dictAudioPlayers.Remove(id);
            if (dictBackGroundAudioPlayer.ContainsKey(id))
                dictBackGroundAudioPlayer.Remove(id);
            player.StopAudio(needNotify);
            player.Dispose();
        }

        private void CollectAudioPlayer(AudioPlayer audioPlayer)
        {
            dictAudioPlayers.Remove(audioPlayer.Id);
            audioPlayer.Dispose();
        }

        public AudioPlayer GetAudioPlayer(uint id)
        {
            if (dictBackGroundAudioPlayer.ContainsKey(id))
                return dictBackGroundAudioPlayer[id];
            if (!dictAudioPlayers.ContainsKey(id))
            {
                Logger.LogRed("[AudioManager.GetAudioPlayer() => 找不到AudioPlayer, id:" + id + "]");
                return null;
            }

            return dictAudioPlayers[id];
        }

        private AudioPlayer CreateAudioObject()
        {
            var name = "AudioSource_" + soundIndex;
            var go = new GameObject(name);
            UnityObjectHelper.SetParent(go.transform, audioRoot);
            AudioSource audioSource  = go.AddComponent<AudioSource>();
            var player = new AudioPlayer(soundIndex, audioSource, CollectAudioPlayer);
            return player;
        }

        public void ControlAllAudio(bool audioState)
        {
            if (dictAudioPlayers != null)
            {
                KeyValuePair<uint, AudioPlayer> kv;
                for (var i = 0; i < dictAudioPlayers.Count; i++)
                {
                    kv = dictAudioPlayers.ElementAt(i);
                    kv.Value.Pause(audioState);
                }
            }

            if (dictBackGroundAudioPlayer != null)
            {
                for (var i = dictBackGroundAudioPlayer.Count - 1; i >= 0; i--)
                {
                    audioDictKv = dictBackGroundAudioPlayer.ElementAt(i);
                    Pause(audioDictKv.Value.Id, true);
                }
            }
        }

        public override void Dispose()
        {
            if (dictAudioPlayers != null)
            {
                KeyValuePair<uint, AudioPlayer> kv;
                for (var i = 0; i < dictAudioPlayers.Count; i++)
                {
                    kv = dictAudioPlayers.ElementAt(i);
                    kv.Value.Dispose();
                }
                dictAudioPlayers.Clear();
                dictAudioPlayers = null;
            }

            audioRoot = null;
            _instance = null;
            AudioClipPool.Dispose();
        }

        private KeyValuePair<uint, AudioPlayer> audioDictKv;
        public void StopAllAudio(bool includeBackGroundAudio = false, bool releaseClipPool = false)
        {
            for (var i = dictAudioPlayers.Count - 1; i >= 0; i--)
            {
                if (dictAudioPlayers.Count <= i)
                {
                    continue;
                }
                audioDictKv = dictAudioPlayers.ElementAt(i);
                StopAudio(audioDictKv.Value.Id, true);
            }

            if (includeBackGroundAudio)
            {
                for (var i = dictBackGroundAudioPlayer.Count - 1; i >= 0; i--)
                {
                    audioDictKv = dictBackGroundAudioPlayer.ElementAt(i);
                    StopAudio(audioDictKv.Value.Id, true);
                }
            }
            if (releaseClipPool)
            {
                AudioClipPool.ReleasePool();
            }
        }

        public void StopAllAudioByTag(string tag, bool releaseClipPool = false)
        {
            for (var i = dictAudioPlayers.Count - 1; i >= 0; i--)
            {
                audioDictKv = dictAudioPlayers.ElementAt(i);
                if (audioDictKv.Value.Tag == tag)
                {
                    StopAudio(audioDictKv.Value.Id, true);
                }
            }

            if (releaseClipPool)
            {
                AudioClipPool.ReleasePool();
            }
        }

        public void StopAllMusic()
        {
            for (var i = dictBackGroundAudioPlayer.Count - 1; i >= 0; i--)
            {
                audioDictKv = dictBackGroundAudioPlayer.ElementAt(i);
                StopAudio(audioDictKv.Value.Id, true);
            }
        }

        public void Update()
        {
            if (dictAudioPlayers != null && dictAudioPlayers.Count > 0)
            {
                for (int i = 0; i < dictAudioPlayers.Count; i++)
                {
                    audioDictKv = dictAudioPlayers.ElementAt(i);
                    audioDictKv.Value.Update();
                }
            }

        }
    }
}