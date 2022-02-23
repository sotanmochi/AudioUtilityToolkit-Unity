using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace AudioUtilityToolkit.Samples
{
    public class AudioInputControlView : MonoBehaviour
    {
        [SerializeField] private Dropdown _selectDropdown;
        [SerializeField] private Toggle _loopbackToggle;
        [SerializeField] private Text _inputChannelCount;
        [SerializeField] private Text _loopbackChannelCount;
        
        public IReadOnlyReactiveProperty<(int Index, string Name)> CurrentDevice => _currentItem;
        private ReactiveProperty<(int Index, string Name)> _currentItem = new ReactiveProperty<(int Index, string Name)>();
        
        public IReadOnlyReactiveProperty<bool> LoopbackIsActive => _loopbackIsActive;
        private ReactiveProperty<bool> _loopbackIsActive = new ReactiveProperty<bool>(true);
        
        private string[] _dropdownItems;
        private string _dropdownMessage = "Select AudioInput";
        
        private void Awake()
        {            
            _selectDropdown.OnValueChangedAsObservable()
            .Subscribe(selectedIndex => 
            {
                if (selectedIndex < 1)
                {
                    return;
                }
                int deviceIndex = selectedIndex - 1;
                _currentItem.Value = (deviceIndex, _dropdownItems[deviceIndex]);
            })
            .AddTo(this);
            
            _loopbackToggle.OnValueChangedAsObservable()
            .Subscribe(value => _loopbackIsActive.Value = value)
            .AddTo(this);
        }
        
        public void UpdateSelectDropdown(List<string> itemList)
        {
            _dropdownItems = itemList.ToArray();

            _selectDropdown.ClearOptions();
            _selectDropdown.RefreshShownValue();
            _selectDropdown.options.Add(new Dropdown.OptionData { text = _dropdownMessage });
            
            foreach(var item in itemList)
            {
                _selectDropdown.options.Add(new Dropdown.OptionData { text = item });
            }
            
            _selectDropdown.RefreshShownValue();
        }

        public void SetChannelCount(int inputChannelCount, int loopbackChannelCount)
        {
            _inputChannelCount.text = $"Input channel count: {inputChannelCount}";
            _loopbackChannelCount.text = $"Loopback channel count: {loopbackChannelCount}";
        }
    }
}