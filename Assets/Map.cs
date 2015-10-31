/*************************************************************

** Auth: ysd
** Date: 15.10.30
** Desc: 生成地图
** Vers: v1.0

*************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using RandomMapGenerator;

namespace RandomMapGenerator
{

    public class Map : MonoBehaviour
    {

        public Terrain terrain;

        /// <summary>
        /// 地图尺寸
        /// </summary>
        public Vector2 mapSize;

        public float radius;

        /// <summary>
        /// 栅格个数
        /// </summary>
        public uint m;
        [System.NonSerialized]
        public uint n;

        private Raster _raster;
        private Vector2[,] _vertexes;
        private Vector2[,] _centers;

        private RasterIndex[] _circleIndecies;

        private PerlinNoise _noise;

        /// <summary>
        /// 判断四边形是否是陆地
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool IsInsideIsland (uint x, uint y)
        {
            float minSize = Mathf.Min(mapSize.x, mapSize.y);
            radius = Mathf.Min(minSize * 0.9f, radius);

            // 默认整个栅格的中心
            RasterIndex centerIndex = new RasterIndex(m / 2, n / 2);
            Vector2 centerPoint = _raster.GetSquareCenter(centerIndex);

            int cor_x = (int)x - (int)centerIndex.x;
            int cor_y = (int)y - (int)centerIndex.y;

            // 弧度
            float angle = Mathf.Atan2(cor_y, cor_x);
            if (angle < 0)
                angle = 2 * Mathf.PI + angle;

            // 该点到圆周的最大距离
            float noiseValue = 0;
            if (!_noiseValues.ContainsKey(angle))
            {
                noiseValue = _noise.PerlinNoise1D(angle);
                _noiseValues.Add(angle, noiseValue);
            }
            else
            {
                noiseValue = _noiseValues[angle];
            }

            return Vector2.Distance(_raster.SquareCenters[x, y], centerPoint) < noiseValue + radius;
        }

        #region implement MonoBehaviour

        // Awake is called when the script instance is being loaded
        public void Awake ( )
        {

        }

        // Update is called every frame, if the MonoBehaviour is enabled
        public void Update ( )
        {

        }

        bool[,] island;
        private Dictionary<float, float> _noiseValues = new Dictionary<float, float>();

        // Start is called just before any of the Update methods is called the first time
        public void Start ( )
        {

            Rect border = new Rect(Vector2.zero, mapSize);
            float squareSize = border.size.x / m;
            n = (uint)(border.size.y / squareSize);
            _raster = new Raster(border, m);
            _vertexes = _raster.SquareCorners;
            _centers = _raster.SquareCenters;
            _circleIndecies = _raster.GetCircleIndices(new RasterIndex(m / 2, n / 2), Mathf.Min(mapSize.x, mapSize.y) * 0.25f);

            _noise = new PerlinNoise(ampl: 100, count: 8, freq: 10);

            island = new bool[m, n];
            float[,] heights = new float[m, n];
            for (uint i = 0; i < m; i++)
            {
                for (uint j = 0; j < n; j++)
                {
                    island[i, j] = IsInsideIsland(i, j);
                    if (island[i, j])
                        heights[i, j] = 10;
                    else
                        heights[i, j] = 0;
                }
            }

            terrain.terrainData.SetHeightsDelayLOD(0, 0, heights);
        }

        #endregion


    }

}