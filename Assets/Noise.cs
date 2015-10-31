/*************************************************************

** Auth: ysd
** Date: 31/10/2015 13:37
** Desc: 生成Perlin噪声
** Vers: v1.0

*************************************************************/

using UnityEngine;
using System.Collections;

using RandomMapGenerator;

namespace RandomMapGenerator
{

    public class CoherentNoise
    {

        /// <summary>
        /// 整数点的随机值
        /// </summary>
        private float[] _permutation;

        private ushort _sampleCount;

        private float _from;

        private float _to;

        /// <summary>
        /// 从from~to缩放到0~sam-1
        /// </summary>
        private float _scale;

        public CoherentNoise (float from = 0, float to = 2 * Mathf.PI, float ampl = 1, ushort sampleCount = 100)
        {
            _from = Mathf.Min(from, to);
            _to = Mathf.Max(from, to);
            _sampleCount = sampleCount;
            _scale = (float)(sampleCount - 1) / (to - from);
            _permutation = new float[sampleCount];
            for (ushort i = 0; i < sampleCount; i++)
            {
                _permutation[i] = Random.value * 2.0f * ampl - ampl;
            }
        }

        /// <summary>
        /// 非线性插值
        /// </summary>
        private float _Lerp (float left, float right, float t)
        {
            t = Mathf.Min(1.0f, t);
            t = Mathf.Max(0.0f, t);
            float ttt = t * t * t;
            t = ttt * (3.0f * t * (2.0f * t - 5.0f) + 10.0f);
            return (1.0f - t) * left + t * right;
        }

        /// <summary>
        /// 1D coherent noise
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public float CoherentNoise1D (float x)
        {
            x = Mathf.Min(_to, x);
            x = Mathf.Max(_from, x);
            x -= _from;
            x *= _scale;

            // 整数
            if ((ushort)x == x)
                return _permutation[(ushort)x];

            int left = Mathf.FloorToInt(x);
            int right = Mathf.CeilToInt(x);
            return _Lerp(_permutation[left], _permutation[right], x - left);
        }

    }

    /// <summary>
    /// 多个Coherent Noise叠加，这些Coherent Noise的频率递增，幅度递减
    /// </summary>
    public class PerlinNoise
    {

        /// <summary>
        /// 多个Coherent Noise
        /// </summary>
        private CoherentNoise[] _octaves;
        
        public PerlinNoise (float from = 0, float to = Mathf.PI * 2, float ampl = 1, ushort freq = 100, float pers = 0.5f, float lacu = 2f, byte count = 4)
        {
            _octaves = new CoherentNoise[count];
            for (byte i = 0; i < count; i++)
            {
                _octaves[i] = new CoherentNoise(from, to, ampl, freq);
                ampl *= pers;
                freq = (ushort)(freq * lacu);
            }
        }

        /// <summary>
        /// 获得一维柏林噪声
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public float PerlinNoise1D (float x)
        {
            float res = 0;
            for (int i = 0; i < _octaves.Length; i++)
            {
                res += _octaves[i].CoherentNoise1D(x);
            }
            return res / _octaves.Length;
        }

    }

}
