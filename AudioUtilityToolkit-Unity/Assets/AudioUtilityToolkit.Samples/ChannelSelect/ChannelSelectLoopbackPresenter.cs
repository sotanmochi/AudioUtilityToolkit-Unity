using System.Linq;
using NoAlloq;
using UnityEngine;
using UniRx;
using AudioUtilityToolkit.SoundIOExtension;
using AudioUtilityToolkit.UnityAudioExtension;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
using UnityEngine.Profiling;
#endif

namespace AudioUtilityToolkit.Samples
{
    public class ChannelSelectLoopbackPresenter : MonoBehaviour
    {
        [SerializeField] AudioInputControlView _audioInputControlView;
        [SerializeField] UnityAudioOut _loopbackAudioOut;

        private InputStream _inputStream;
        private int _loopbackAudioChannelCount = (int)AudioChannelCount.Mono;
        private float[] _audioFrameBuffer = new float[1024];

        enum AudioChannelCount
        {
            Mono = 1,
            Stereo = 2,
            Quad = 4,
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private CustomSampler _processFrameSampler = CustomSampler.Create("ProcessFrame");
#endif

        private void Awake()
        {
            var inputDeviceList = AudioDeviceDriver.InputDeviceList.Select(device => device.DeviceName).ToList();
            _audioInputControlView.UpdateSelectDropdown(inputDeviceList);

            _audioInputControlView.CurrentDevice
            .SkipLatestValueOnSubscribe()
            .Subscribe(device => 
            {
                if (_inputStream != null) _inputStream.OnProcessFrame -= OnProcessFrame;

                _inputStream = AudioDeviceDriver.GetInputDevice(device.Name);

                if (_inputStream.ChannelCount is (int)AudioChannelCount.Quad)
                {
                    _loopbackAudioChannelCount = (int)AudioChannelCount.Stereo;
                }
                _loopbackAudioOut.Play(_loopbackAudioChannelCount, _inputStream.SampleRate);

                _audioInputControlView.SetChannelCount(_inputStream.ChannelCount, _loopbackAudioChannelCount);

                _inputStream.OnProcessFrame += OnProcessFrame;
            })
            .AddTo(this);

            _audioInputControlView.LoopbackIsActive
            .SkipLatestValueOnSubscribe()
            .Subscribe(loopback =>
            {
                if (loopback)
                {
                    if (_inputStream.ChannelCount is (int)AudioChannelCount.Quad)
                    {
                        _loopbackAudioChannelCount = (int)AudioChannelCount.Stereo;
                    }
                    _loopbackAudioOut.Play(_loopbackAudioChannelCount, _inputStream.SampleRate);

                    _audioInputControlView.SetChannelCount(_inputStream.ChannelCount, _loopbackAudioChannelCount);
                }
                else
                {
                    _loopbackAudioOut.Stop();
                    _audioInputControlView.SetChannelCount(0, _loopbackAudioChannelCount);
                }
            })
            .AddTo(this);
        }

        private void OnDestroy()
        {
            _inputStream.OnProcessFrame -= OnProcessFrame;
        }

        private void OnProcessFrame(float[] audioFrame)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                _processFrameSampler.Begin();
#endif

            if (!_audioInputControlView.LoopbackIsActive.Value) { return; }

            var inputChannelCount = _inputStream.ChannelCount;
            if (inputChannelCount is (int)AudioChannelCount.Quad)
            {
                var ratio = inputChannelCount / _loopbackAudioChannelCount;
                if (_audioFrameBuffer.Length != audioFrame.Length / ratio)
                {
                    _audioFrameBuffer = new float[audioFrame.Length / ratio];
                }

                // Using NoAlloq
                audioFrame
                    .Where((value, index) => (index % 4 == 0 || index % 4 == 1)) // Microphone 1 and 2
                    // .Where((value, index) => (index % 4 == 0 || index % 4 == 2)) // Microphone 1 and 3
                    // .Where((value, index) => (index % 4 == 0 || index % 4 == 3)) // Microphone 1 and 4
                    .ToSpanEnumerable().CopyInto(_audioFrameBuffer);
                _loopbackAudioOut.PushAudioFrame(_audioFrameBuffer);

                // // Using Linq
                // var tempBuffer = audioFrame
                //     .Where((value, index) => (index % 4 == 0 || index % 4 == 1)) // Microphone 1 and 2
                //     // .Where((value, index) => (index % 4 == 0 || index % 4 == 2)) // Microphone 1 and 3
                //     // .Where((value, index) => (index % 4 == 0 || index % 4 == 3)) // Microphone 1 and 4
                //     .ToArray();
                // _loopbackAudioOut.PushAudioFrame(tempBuffer);
            }
            else
            {
                if (_audioFrameBuffer.Length != audioFrame.Length / inputChannelCount)
                {
                    _audioFrameBuffer = new float[audioFrame.Length / inputChannelCount];
                }

                // Using NoAlloq
                audioFrame.Where((value, index) => index % inputChannelCount == 0).ToSpanEnumerable().CopyInto(_audioFrameBuffer);
                _loopbackAudioOut.PushAudioFrame(_audioFrameBuffer);

                // // Using Linq
                // var tempBuffer = audioFrame.Where((value, index) => index % inputChannelCount == 0).ToArray();
                // _loopbackAudioOut.PushAudioFrame(tempBuffer);
            }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                _processFrameSampler.End();
#endif
        }
    }
}
