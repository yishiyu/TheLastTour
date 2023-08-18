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

        private float _musicVolume = 1;

        public float MusicVolume
        {
            get { return _musicVolume; }
            set { _musicVolume = Mathf.Clamp01(value); }
        }

        private float _soundVolume = 1;

        public float SoundVolume
        {
            get { return _soundVolume; }
            set { _soundVolume = Mathf.Clamp01(value); }
        }


        public AudioSource PlaySound(AudioClip clip, float volume = 1, bool loop = false)
        {
            SoundSource.clip = clip;
            SoundSource.volume = Mathf.Clamp01(volume * SoundVolume);
            SoundSource.loop = loop;
            SoundSource.Play();

            return SoundSource;
        }

        public AudioSource PlayMusic(AudioClip clip, float volume = 1, bool loop = true)
        {
            MusicSource.clip = clip;
            MusicSource.volume = Mathf.Clamp01(volume * MusicVolume);
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