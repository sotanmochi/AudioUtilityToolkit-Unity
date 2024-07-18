using System;
using System.Threading;
using UnityEngine;

namespace AudioUtilityToolkit.UnityAudioExtension
{
    /// <summary>
    /// Low latency audio output using AudioSource.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class UnityAudioOutV2 : MonoBehaviour
    {
        [SerializeField] int _channels = (int)ChannelCount.Mono;
        [SerializeField] int _samplingRate = (int)SamplingRateType.SamplingRate_48000;

        float _bufferingTimeSec = 1.0f;
        int _audioFrameDurationMs = 40;
        int _delayFrames = 2;

        AudioSource _audioSource;
        float[] _zeroBuffer;
        int _audioClipSamples;
        int _audioFrameDurationSamples;
        int _lastPlaybackSamplePosition;
        int _bufferedSamplesCount;
        int _bufferTailPosition;

        void Awake()
        {
            CreateNewAudioClip(_channels, _samplingRate, _bufferingTimeSec, _audioFrameDurationMs);
        }

        void Update()
        {
            var playbackSamplePosition = _audioSource.timeSamples;

            var dequeuedSamplesCount = (playbackSamplePosition - _lastPlaybackSamplePosition + _audioClipSamples) % _audioClipSamples;
            _lastPlaybackSamplePosition = playbackSamplePosition;

            _bufferedSamplesCount -= dequeuedSamplesCount;
            if (_bufferedSamplesCount <= 0)
            {
                Stop();
            }
        }

        public void PushAudioFrame(float[] pcmData)
        {
            // NOTE:
            // If the length from the offset is longer than the clip length,
            // the write will wrap around and write the remaining samples from the start of the clip.
            // https://docs.unity3d.com/jp/2022.3/ScriptReference/AudioClip.SetData.html
            //
            _audioSource.clip.SetData(pcmData, _bufferTailPosition); // Enqueue data

            var samplesCount = pcmData.Length / _channels;
            _bufferedSamplesCount += samplesCount;
            _bufferTailPosition += samplesCount;

            if (!_audioSource.isPlaying && (_bufferTailPosition > _audioFrameDurationSamples * _delayFrames))
            {
                _audioSource.Play();
            }

            _bufferTailPosition = _bufferTailPosition % (_audioClipSamples);
        }

        public void Play(int channels = 1, int samplingRate = 48000)
        {
            if (channels != _channels || samplingRate != _samplingRate)
            {
                CreateNewAudioClip(channels, samplingRate, _bufferingTimeSec, _audioFrameDurationMs);
            }
            _audioSource.Play();
        }

        public void Stop()
        {
            _audioSource.Stop();
            _audioSource.clip.SetData(_zeroBuffer, offsetSamples: 0);
            _audioSource.timeSamples = 0;

            _lastPlaybackSamplePosition = 0;
            _bufferedSamplesCount = 0;
            _bufferTailPosition = 0;
        }

        void CreateNewAudioClip(int channels, int samplingRate, float bufferingTimeSec, int audioFrameDurationMs)
        {
            _channels = channels;
            _samplingRate = samplingRate;
            _bufferingTimeSec = bufferingTimeSec;
            _audioFrameDurationMs = audioFrameDurationMs;

            _audioClipSamples = (int)(_samplingRate * _bufferingTimeSec);
            _zeroBuffer = new float[_audioClipSamples * _channels];

            _audioFrameDurationSamples = (int)(_samplingRate * _audioFrameDurationMs / 1000f);

            _lastPlaybackSamplePosition = 0;
            _bufferedSamplesCount = 0;
            _bufferTailPosition = 0;

            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }
            _audioSource.clip = AudioClip.Create("UnityAudioOutput", _audioClipSamples, _channels, _samplingRate, false);
            _audioSource.loop = true;
        }
    }
}