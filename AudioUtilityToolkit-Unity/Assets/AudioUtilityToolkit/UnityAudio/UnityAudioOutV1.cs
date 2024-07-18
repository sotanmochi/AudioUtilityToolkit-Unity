using System;
using System.Threading;
using UnityEngine;

namespace AudioUtilityToolkit.UnityAudioExtension
{
    [RequireComponent(typeof(AudioSource))]
    public class UnityAudioOutV1 : MonoBehaviour
    {
        [SerializeField] int _channels = (int)ChannelCount.Mono;
        [SerializeField] int _samplingRate = (int)SamplingRateType.SamplingRate_48000;

        float _bufferingTimeSec = 1.0f;
        int _audioFrameDurationMs = 40;
        int _delayFrames = 2;

        AudioSource _audioSource;
        AudioBuffer _audioBuffer;
        int _audioClipSamples;
        int _audioFrameDurationSamples;

        void Awake()
        {
            CreateNewAudioClip(_channels, _samplingRate, _bufferingTimeSec, _audioFrameDurationMs);
        }

        void OnPcmDataRead(float[] output)
        {
            _audioBuffer.Dequeue(output);
        }

        public void PushAudioFrame(float[] pcmData)
        {
            _audioBuffer.Enqueue(pcmData);

            if (!_audioSource.isPlaying && (_audioBuffer.Count > _audioFrameDurationSamples * _delayFrames))
            {
                _audioSource.Play();
            }
        }

        public void Play(int channels = 1, int samplingRate = 48000)
        {
            if (channels != _audioBuffer.Channels || samplingRate != _audioBuffer.SamplingRate)
            {
                CreateNewAudioClip(channels, samplingRate, _bufferingTimeSec, _audioFrameDurationMs);
            }
            _audioSource.Play();
        }

        public void Stop()
        {
            _audioSource.Stop();
        }

        void CreateNewAudioClip(int channels, int samplingRate, float bufferingTimeSec, int audioFrameDurationMs)
        {
            _channels = channels;
            _samplingRate = samplingRate;
            _bufferingTimeSec = bufferingTimeSec;
            _audioFrameDurationMs = audioFrameDurationMs;

            _audioClipSamples = (int)(_samplingRate * _bufferingTimeSec);
            _audioBuffer = new AudioBuffer(_channels, _samplingRate, _bufferingTimeSec);

            _audioFrameDurationSamples = (int)(_samplingRate * _audioFrameDurationMs / 1000f);

            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }
            _audioSource.clip = AudioClip.Create("UnityAudioOutput", _audioClipSamples, _channels, _samplingRate, true, OnPcmDataRead);
            _audioSource.loop = true;
        }
    }
}