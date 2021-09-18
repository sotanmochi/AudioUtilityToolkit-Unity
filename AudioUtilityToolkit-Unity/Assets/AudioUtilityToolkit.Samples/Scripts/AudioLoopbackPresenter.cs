using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using AudioUtilityToolkit.SoundIOExtension;
using AudioUtilityToolkit.UnityAudioExtension;

namespace AudioUtilityToolkit.Samples
{
    public class AudioLoopbackPresenter : MonoBehaviour
    {
        [SerializeField] AudioInputControlView _audioInputControlView;
        [SerializeField] UnityAudioOut _loopbackAudioOut;
        
        private UnityMicrophone _microphone;
        private InputStream _inputStream;
        
        private void Awake()
        {
            // _microphone = new UnityMicrophone();
            // _microphone.OnProcessFrame += OnProcessFrame;
            // _audioInputControlView.UpdateSelectDropdown(Microphone.devices.ToList());

            var inputDeviceList = AudioDeviceDriver.InputDeviceList.Select(device => device.DeviceName).ToList();
            _audioInputControlView.UpdateSelectDropdown(inputDeviceList);

            _audioInputControlView.CurrentDevice
            .SkipLatestValueOnSubscribe()
            .Subscribe(device => 
            {
                // _microphone.Stop();
                // _microphone.Start(device.Name);
                // _loopbackAudioOut.StartOutput();

                if (_inputStream != null) _inputStream.OnProcessFrame -= OnProcessFrame;

                _inputStream = AudioDeviceDriver.GetInputDevice(device.Name);
                _loopbackAudioOut.StartOutput(_inputStream.ChannelCount, _inputStream.SampleRate);

                _inputStream.OnProcessFrame += OnProcessFrame;
            })
            .AddTo(this);

            _audioInputControlView.LoopbackIsActive
            .SkipLatestValueOnSubscribe()
            .Subscribe(loopback =>
            {
                if (loopback)
                {
                    _loopbackAudioOut.StartOutput(_inputStream.ChannelCount, _inputStream.SampleRate);
                }
                else
                {
                    _loopbackAudioOut.StopOutput();
                }
            })
            .AddTo(this);
        }

        private void OnDestroy()
        {
            // _microphone.OnProcessFrame -= OnProcessFrame;
            // _microphone.Dispose();
            _inputStream.OnProcessFrame -= OnProcessFrame;
        }

        private void OnProcessFrame(float[] data)
        {
            if (_audioInputControlView.LoopbackIsActive.Value)
            {
                _loopbackAudioOut.PushAudioFrame(data);
            }
        }
    }
}
