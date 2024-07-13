using System;
using System.Threading;

namespace AudioUtilityToolkit
{
    public sealed class AudioBuffer
    {
        readonly int _channelCount;
        readonly int _samplingRate;
        readonly float _bufferingTimeSec;
        readonly RingBuffer<float> _ringBuffer;

        public int Channels => _channelCount;
        public int SamplingRate => _samplingRate;
        public float BufferingTimeSec => _bufferingTimeSec;

        public AudioBuffer(int channelCount = (int)ChannelCount.Mono,
            int samplingRate = (int)SamplingRateType.SamplingRate_16000, float bufferingTimeSec = 1.0f)
        {
            _channelCount = channelCount;
            _samplingRate = samplingRate;
            _bufferingTimeSec = bufferingTimeSec;
            _ringBuffer = new RingBuffer<float>((int)(Channels * SamplingRate * BufferingTimeSec));
        }

        public void Enqueue(float[] data)
        {
            lock (_ringBuffer)
            {
                _ringBuffer.Enqueue(data);
            }
        }

        public void Dequeue(Span<float> destination)
        {
            lock (_ringBuffer)
            {
                _ringBuffer.Dequeue(destination, fillWithDefaultWhenEmpty: true);
            }
        }
    }
}