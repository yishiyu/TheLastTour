using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastTour.Manager
{
    public interface IAudioManager : IManager
    {
        public float GlobalVolume { get; set; }

        /// <summary>
        /// BGM 等
        /// </summary>
        public float MusicVolume { get; set; }

        /// <summary>
        /// 过关音效等
        /// </summary>
        public float SoundVolume { get; set; }

        public AudioSource PlaySound(AudioClip clip, float volume = 1f, bool loop = false);
        public AudioSource PlayMusic(AudioClip clip, float volume = 1f, bool loop = true);
        public void StopMusic();
    }

    public class AudioManager : IAudioManager
    {
        private AudioSource _musicSource;

        private AudioSource MusicSource
        {
            get
            {
                if (_musicSource == null && Camera.main != null)
                {
                    _musicSource = Camera.main.gameObject.AddComponent<AudioSource>();
                }

                return _musicSource;
            }
        }

        private AudioSource _soundSource;

        private AudioSource SoundSource
        {
            get
            {
                if (_soundSource == null && Camera.main != null)
                {
                    _soundSource = Camera.main.gameObject.AddComponent<AudioSource>();
                }

                return _soundSource;
            }
        }

        public void Init(IArchitecture architecture)
        {
            Debug.Log("AudioManager Init");
        }

        public float GlobalVolume
        {
            get { return AudioListener.volume; }
            set { AudioListener.volume = value; }
        }

        // 当前音乐和音乐普适音量
        private float _musicVolume = 1f;
        private float _currentMusicVolume = 0.2f;

        public float MusicVolume
        {
            get { return _musicVolume; }
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                MusicSource.volume = Mathf.Clamp01(_currentMusicVolume * _musicVolume);
            }
        }

        private float _soundVolume = 1;
        private float _currentSoundVolume = 0.2f;

        public float SoundVolume
        {
            get { return _soundVolume; }
            set
            {
                _soundVolume = Mathf.Clamp01(value);
                SoundSource.volume = Mathf.Clamp01(_currentSoundVolume * _soundVolume);
            }
        }


        public AudioSource PlaySound(AudioClip clip, float volume = 1, bool loop = false)
        {
            // 设置当前音量,并触发一次音量调整
            _currentSoundVolume = volume;
            SoundVolume = SoundVolume;

            SoundSource.clip = clip;
            SoundSource.loop = loop;
            SoundSource.Play();

            return SoundSource;
        }

        public AudioSource PlayMusic(AudioClip clip, float volume = 1, bool loop = true)
        {
            // 设置当前音量,并触发一次音量调整
            _currentMusicVolume = volume;
            MusicVolume = MusicVolume;

            MusicSource.clip = clip;
            MusicSource.loop = loop;
            MusicSource.Play();

            return MusicSource;
        }

        public void StopMusic()
        {
            MusicSource.Stop();
        }
    }
}