using System.Linq;
using NoAlloq;
using UnityEngine;
using UnityEngine.Profiling;

namespace AudioUtilityToolkit.Samples
{
    public class NoAlloqTest : MonoBehaviour
    {
        int[] _data, _buffer;

        CustomSampler _noalloqSampler = CustomSampler.Create("NoAlloq");
        CustomSampler _linqSampler = CustomSampler.Create("Linq");

        void Awake()
        {
            _data = Enumerable.Range(0, 2048).ToArray();
            _buffer = new int[_data.Length / 2];
        }

        void Update()
        {
            _noalloqSampler.Begin();

            // Using NoAlloq
            _data.Where((value, index) => index % 2 == 0)
                    .ToSpanEnumerable()
                    .CopyInto(_buffer);

            _noalloqSampler.End();

            _linqSampler.Begin();

            // Using Linq only
            _data.Where((value, index) => index % 2 == 0).ToArray();

            _linqSampler.End();
        }
    }
}