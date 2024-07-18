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
        [SerializeField] UnityAudioOutV1 _loopbackAudioOut;
        [SerializeField] UnityAudioOutV2 _loopbackAudioOutV2;
        
        private UnityMicrophone _microphone;
        private InputStream _inputStream;

        private AudioLoopbackMode _loopbackMode;
        private AudioSource _audioSourceV1;
        private AudioSource _audioSourceV2;
        
        private void Awake()
        {
            _audioSourceV1 = _loopbackAudioOut.GetComponent<AudioSource>();
            _audioSourceV2 = _loopbackAudioOutV2.GetComponent<AudioSource>();

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
                // _loopbackAudioOut.Play();

                if (_inputStream != null) _inputStream.OnProcessFrame -= OnProcessFrame;

                _inputStream = AudioDeviceDriver.GetInputDevice(device.Name);
                _loopbackAudioOut.Play(_inputStream.ChannelCount, _inputStream.SampleRate);
                _loopbackAudioOutV2.Play(_inputStream.ChannelCount, _inputStream.SampleRate);

                _inputStream.OnProcessFrame += OnProcessFrame;
            })
            .AddTo(this);

            _audioInputControlView.LoopbackIsActive
            .SkipLatestValueOnSubscribe()
            .Subscribe(loopback =>
            {
                // if (loopback)
                // {
                //     _loopbackAudioOut.Play(_inputStream.ChannelCount, _inputStream.SampleRate);
                // }
                // else
                // {
                //     _loopbackAudioOut.Stop();
                // }
            })
            .AddTo(this);

            _audioInputControlView.LoopbackMode
            .Subscribe(mode =>
            {
                if (mode == AudioLoopbackMode.V1Left_V2Right)
                {
                    _audioSourceV1.volume = 1;
                    _audioSourceV1.panStereo = -1;
                    _audioSourceV2.volume = 1;
                    _audioSourceV2.panStereo = 1;
                }
                else if (mode == AudioLoopbackMode.V2Only)
                {
                    _audioSourceV1.volume = 0;
                    _audioSourceV1.panStereo = 0;
                    _audioSourceV2.volume = 1;
                    _audioSourceV2.panStereo = 0;
                }
                else if (mode == AudioLoopbackMode.V1Only)
                {
                    _audioSourceV1.volume = 1;
                    _audioSourceV1.panStereo = 0;
                    _audioSourceV2.volume = 0;
                    _audioSourceV2.panStereo = 0;
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
                _loopbackAudioOutV2.PushAudioFrame(data);
            }
        }
    }
}
