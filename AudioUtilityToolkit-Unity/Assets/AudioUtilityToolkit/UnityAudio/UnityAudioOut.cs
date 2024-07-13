using System;
using System.Threading;
using UnityEngine;

namespace AudioUtilityToolkit.UnityAudioExtension
{
    [RequireComponent(typeof(AudioSource))]
    public class UnityAudioOut : MonoBehaviour
    {
        [SerializeField] private int _channels = (int)ChannelCount.Mono;
        [SerializeField] private int _samplingRate = (int)SamplingRateType.SamplingRate_48000;

        private AudioSource _audioSource;
        private int _audioClipSamples;
        private float _bufferingTimeSec = 1.0f;

        private AudioBuffer _audioBuffer;

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();

            _audioClipSamples = (int)(_samplingRate * _bufferingTimeSec);
            _audioBuffer = new AudioBuffer(_channels, _samplingRate, _bufferingTimeSec);

            _audioSource.clip = AudioClip.Create("UnityAudioOutput", _audioClipSamples,
                                    _audioBuffer.Channels, _audioBuffer.SamplingRate, true, OnPcmDataRead);
            _audioSource.loop = true;
        }

        void OnPcmDataRead(float[] output)
        {
            _audioBuffer.Dequeue(output);
        }

        public void PushAudioFrame(float[] pcmData)
        {
            _audioBuffer.Enqueue(pcmData);
        }

        public void Play(int channels = 1, int samplingRate = 48000)
        {
            if (channels != _audioBuffer.Channels || samplingRate != _audioBuffer.SamplingRate)
            {
                _channels = channels;
                _samplingRate = samplingRate;
                _audioClipSamples = (int)(_samplingRate * _bufferingTimeSec);

                _audioBuffer = new AudioBuffer(_channels, _samplingRate, _bufferingTimeSec);

                _audioSource.clip = AudioClip.Create("UnityAudioOutput", _audioClipSamples,
                                        _audioBuffer.Channels, _audioBuffer.SamplingRate, true, OnPcmDataRead);
                _audioSource.loop = true;
            }
            _audioSource.Play();
        }

        public void Stop()
        {
            _audioSource.Stop();
        }
    }
}