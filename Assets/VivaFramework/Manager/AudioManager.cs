using System;
using System.Collections.Generic;
using UnityEngine;

namespace VivaFramework
{
    public class AudioManager : MonoBehaviour
    {
        private List<AudioSource> sources;  //AudioSource容器
        private Dictionary<string, AudioClip> clipDictionary; //缓存加载的AudioClip
        private AudioListener audioListener; //AudioListener组件
        private GameObject _audioMgr; //放置音效相关组件的GameObject
        [HideInInspector]
        public AudioSource effectAudioSource; //默认音效播放器
        [HideInInspector]
        public AudioSource bgmAudioSource; //默认bgm播放器
        private void Start()
        {
            if (_audioMgr is null)
            {
                _audioMgr = new GameObject("AudioRoot");
                DontDestroyOnLoad(_audioMgr); 
            }

            audioListener = _audioMgr.AddComponent<AudioListener>();
            sources = new List<AudioSource>();
            clipDictionary = new Dictionary<string, AudioClip>();
            
            //先添加两个默认AudioSource
            effectAudioSource = CreateAudioSource();
            bgmAudioSource = CreateAudioSource();
        }

        //添加一个新的AudioSource
        private AudioSource CreateAudioSource()
        {
            AudioSource audioSource = _audioMgr.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            sources.Add(audioSource);
            return audioSource;
        }
        
        //获取一个闲置的AudioSource
        public AudioSource GetAudioSource()
        {
            AudioSource audioSource = null;
            for (int i = 0; i < sources.Count; i++)
            {
                AudioSource source = sources[i];
                if (source.isPlaying == false && source != effectAudioSource && source != bgmAudioSource)
                {
                    audioSource = sources[i];
                    break;
                }
            }

            if (audioSource == null)
            {
                audioSource = CreateAudioSource();
            }

            return audioSource;
        }

        //获取一个播放过指定clip的AudioSource
        public AudioSource GetAudioSource(string clipName)
        {
            AudioSource audioSource = null;
            for (int i = 0; i < sources.Count; i++)
            {
                AudioSource source = sources[i];
                if (source != effectAudioSource && source != bgmAudioSource && source.clip != null && source.clip.name == clipName)
                {
                    audioSource = sources[i];
                    break;
                }
            }
            
            if (audioSource == null)
            {
                audioSource = CreateAudioSource();
            }
            
            return audioSource;
        }

        //清理闲置的音源
        public void CleanIdleAudioSource()
        {
            for (int i = sources.Count - 1; i >= 0; i--)
            {
                AudioSource source = sources[i];
                if (source != effectAudioSource && source != bgmAudioSource && source.isPlaying == false)
                {
                    Destroy(source);
                    sources.RemoveAt(i);
                }
            }
        }

        //根据路径获取AudioClip
        public AudioClip GetAudioClip(string clipPath)
        {
            AudioClip audioClip;
            if (clipDictionary.TryGetValue(clipPath, out audioClip) == false)
            {
                audioClip = (AudioClip)Main.resManager.LoadAssetAtPath(clipPath);
                clipDictionary.Add(clipPath, audioClip);
            }

            return audioClip;
        }

        //暂停所有的AudioSource
        public void PauseAllAudio()
        {
            for (int i = 0; i < sources.Count; i++)
            {
                sources[i].Pause();
            }
        }
        
        //播放所有的AudioSource
        public void PlayAllAudio()
        {
            for (int i = 0; i < sources.Count; i++)
            {
                sources[i].Play();
            }
        }

        //停止所有的AudioSource
        public void StopAllAudio()
        {
            for (int i = 0; i < sources.Count; i++)
            {
                sources[i].Stop();
            }
        }
        
        //播放BGM
        public void PlayBGM(string clipPath, float volume = 1f, bool isLoop = true)
        {
            AudioClip audioClip = GetAudioClip(clipPath);
            if (audioClip == null)
            {
                Debug.LogError("PlayBGM 没有找到音源 path:" + clipPath);
                return;
            }
            StopBGM();
            if (bgmAudioSource.clip == null || bgmAudioSource.clip.name != audioClip.name)
            {
                bgmAudioSource.clip = audioClip;
                bgmAudioSource.loop = isLoop;
                bgmAudioSource.volume = volume;
            }
            bgmAudioSource.loop = isLoop;
            bgmAudioSource.Play();
        }

        //停止BGM
        public void StopBGM()
        {
            if (bgmAudioSource.isPlaying)
            {
                bgmAudioSource.Stop();
            }
        }

        //播放音效
        public void PlayEffectAudio(string clipPath,float volumeScale = 1f)
        {
            AudioClip audioClip = GetAudioClip(clipPath);
            if (audioClip == null)
            {
                Debug.LogError("PlayEffectAudio 没有找到音源 path:" + clipPath);
                return;
            }
            effectAudioSource.PlayOneShot(audioClip, volumeScale);
        }
        

        void OnDestroy()
        {
            DestroyImmediate(_audioMgr);
            _audioMgr = null;
            sources.Clear();
            sources = null;
            clipDictionary.Clear();
            clipDictionary = null;
            audioListener = null;
        }
    }
}