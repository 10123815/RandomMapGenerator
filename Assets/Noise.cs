/*************************************************************

** Auth: ysd
** Date: 31/10/2015 13:37
** Desc: ����Perlin����
** Vers: v1.0

*************************************************************/

using UnityEngine;
using System.Collections;

using RandomMapGenerator;

namespace RandomMapGenerator
{

    /// <summary>
    /// һά
    /// </summary>
    public class CoherentNoise1D
    {

        /// <summary>
        /// ����������ֵ
        /// </summary>
        private float[] _permutation;

        private ushort _sampleCount;

        private float _from;

        private float _to;

        /// <summary>
        /// ��from~to���ŵ�0~sam-1
        /// </summary>
        private float _scale;

        public CoherentNoise1D (float from = 0, float to = 2 * Mathf.PI, float ampl = 1, ushort sampleCount = 100)
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
        /// �����Բ�ֵ
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
        public float GetPixel (float x)
        {
            x = Mathf.Min(_to, x);
            x = Mathf.Max(_from, x);
            x -= _from;
            x *= _scale;

            // ����
            if ((ushort)x == x)
                return _permutation[(ushort)x];

            int left = Mathf.FloorToInt(x);
            int right = Mathf.CeilToInt(x);
            return _Lerp(_permutation[left], _permutation[right], x - left);
        }

    }

    /// <summary>
    /// ���Coherent Noise���ӣ���ЩCoherent Noise��Ƶ�ʵ��������ȵݼ�
    /// </summary>
    public class PerlinNoise1D
    {

        /// <summary>
        /// ���Coherent Noise
        /// </summary>
        private CoherentNoise1D[] _octaves;

        public PerlinNoise1D (float from = 0, float to = Mathf.PI * 2, float ampl = 1, ushort freq = 100, float pers = 0.5f, float lacu = 2f, byte count = 4)
        {
            _octaves = new CoherentNoise1D[count];
            for (byte i = 0; i < count; i++)
            {
                _octaves[i] = new CoherentNoise1D(from, to, ampl, freq);
                ampl *= pers;
                freq = (ushort)(freq * lacu);
            }
        }

        /// <summary>
        /// ���һά��������
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public float GetPixel (float x)
        {
            float res = 0;
            for (int i = 0; i < _octaves.Length; i++)
            {
                res += _octaves[i].GetPixel(x);
            }
            return res / _octaves.Length;
        }

    }

    /// <summary>
    /// 0~1���������
    /// </summary>
    public class CoherentNoise2D
    {

        private float[,] _permutaion;

        private uint _m;
        private uint _n;

        public CoherentNoise2D (uint m, uint n)
        {
            _m = m;
            _n = n;
            _permutaion = new float[m, n];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    _permutaion[i, j] = Random.value;
                }
            }
        }

        /// <summary>
        /// ���Բ�ֵ
        /// </summary>
        /// <param name="t">��from�ľ���</param>
        /// <returns></returns>
        private float _Lerp (float from, float to, float t)
        {
            return (1 - t) * from + t * to;
        }

        /// <summary>
        /// �⻬��ֵ
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private float _SmoothCurve (float t)
        {
            t = Mathf.Min(1.0f, t);
            t = Mathf.Max(0.0f, t);
            float ttt = t * t * t;
            return ttt * (3.0f * t * (2.0f * t - 5.0f) + 10.0f);
        }

        /// <summary>
        /// ˫���Բ�ֵ
        /// @---->*<--@
        ///       ��
        ///       #
        ///       ��
        /// @---->*<--@
        /// </summary>
        /// <returns></returns>
        private float _Lerp2D (uint x1, uint y1, uint x2, uint y2, float tx, float ty)
        {
            tx = _SmoothCurve(tx);
            ty = _SmoothCurve(ty);

            return _Lerp(
                _Lerp(_permutaion[x1, y1], _permutaion[x2, y1], tx),
                _Lerp(_permutaion[x1, y2], _permutaion[x2, y2], tx),
                ty);
        }

        /// <summary>
        /// �õ�����ֵ
        /// </summary>
        /// <param name="x">������</param>
        /// <param name="y">������</param>
        /// <returns></returns>
        public float GetPixel (float x, float y)
        {
            x = Mathf.Min(x, _m - 1);
            x = Mathf.Max(x, 0);

            y = Mathf.Min(y, _n - 1);
            y = Mathf.Max(y, 0);

            uint curX = (uint)Mathf.FloorToInt(x);
            uint curY = (uint)Mathf.FloorToInt(y);
            uint nextX = (uint)Mathf.CeilToInt(x);
            uint nextY = (uint)Mathf.CeilToInt(y);

            return _Lerp2D(curX, curY, nextX, nextY, x - (float)curX, y - (float)curY);

        }


    }

    /// <summary>
    /// ��ά��������
    /// </summary>
    public class PerlinNoise2D
    {

        /// <summary>
        /// ��0����1��Ƶ...
        /// </summary>
        private CoherentNoise2D[] _octaves;

        /// <summary>
        /// 1��Ƶ�ķ��ȣ�������
        /// </summary>
        private float _amplitude;

        public float Amplitude
        {
            get
            {
                return _amplitude;
            }
        }

        private float _scaleX;
        private float _scaleY;

        /// <summary>
        /// �������Ϊ2^n+1
        /// <param name="minFreq">x������С��Ƶ</param>
        /// </summary>
        public PerlinNoise2D (uint m, uint n, uint minFreq, float maxAmpl = 1)
        {
            // ���ŵ���׼2^n
            minFreq = _DownTo2Based(minFreq);
            uint newM = _DownTo2Based(m);
            uint newN = _DownTo2Based(n);
            _scaleX = (float)newM / (float)m;
            _scaleY = (float)newN / (float)n;

            _octaves = new CoherentNoise2D[newM / minFreq];
            uint freqX = minFreq;
            uint freqY = minFreq * newN / newM;
            // ��ͬƵ�ʵ��������
            for (uint i = 0; i < newM / minFreq; i++)
            {
                m = freqX * (i + 1);
                n = freqY * (i + 1);
                _octaves[i] = new CoherentNoise2D(m, n);
            }

            _amplitude = maxAmpl;

        }

        public PerlinNoise2D (uint m, uint n, byte count = 4, float maxAmpl = 1)
        {
            Debug.Assert(m >= count);

            uint newM = _DownTo2Based(m);
            uint newN = _DownTo2Based(n);
            _scaleX = (float)newM / (float)m;
            _scaleY = (float)newN / (float)n;

            _octaves = new CoherentNoise2D[count];
            uint freqX = newM / count;
            uint freqY = newN / count;
            for (uint i = 0; i < count; i++)
            {
                m = freqX * (i + 1);
                n = freqY * (i + 1);
                _octaves[i] = new CoherentNoise2D(m, n);
            }

            _amplitude = maxAmpl;

        }
        
        /// <summary>
        /// ������x�����2��ָ��
        /// </summary>
        private uint _DownTo2Based (uint x)
        {
            while (x > 0)
            {
                if ((x & (x - 1)) == 0)
                {
                    return x;
                }
                // ȥ�����ұߵ�1
                x = x & (x - 1);
            }
            return 1;
        }

        public float GetPixel (float x, float y)
        {
            float result = 0;
            x *= _scaleX;
            y *= _scaleY;
            for (byte i = 0; i < _octaves.Length; i++)
            {
                // ���ŵ���Ƶ�ķ�Χ��
                float multi = 1 << (_octaves.Length - (i + 1));
                result += _octaves[i].GetPixel(x / multi, y / multi) * _amplitude / (i + 1);
            }
            return result / _octaves.Length;
        }

    }

}
