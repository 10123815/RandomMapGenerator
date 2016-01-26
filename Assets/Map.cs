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

    /// <summary>
    /// 多边形拐角类型
    /// </summary>
    public enum CornerType : byte
    {
        kNone = 0,      // 没有分配属性
        kOcean = 1,     // 海洋
        kCoast = 2,     // 海岸线
        kLand = 3,      // 陆地
        kRiver = 4
    }

    /// <summary>
    /// 四边形中心属性
    /// </summary>
    public enum CenterType : byte
    {
        kNone = 0,
        kOcean = 1,
        kLand = 2
    }

    public enum IslandType : byte
    {
        kCircle = 0,
        kPerlin = 1
    }

    public enum ElevationType : byte
    {
        kRise = 0,
        kFlat = 1
    }

    public class Map : MonoBehaviour
    {

        public Terrain terrain;

        public IslandType islandType;
        public ElevationType elevationType;

        /// <summary>
        /// 地图尺寸
        /// </summary>
        public Vector2 mapSize;

        /// <summary>
        /// 栅格中心
        /// </summary>
        private RasterIndex _centerIndex;
        private Vector2 _centerPoint;

        private float _squareSize;

        /// <summary>
        /// 四边形属性
        /// </summary>
        private CenterType[,] _centerTypes;
        private CornerType[,] _cornerTypes;

        /// <summary>
        /// 属性为海岸线的拐角
        /// </summary>
        private List<RasterIndex> _coastCornerList;
        private RasterIndex[] _coastLineArr;

        private Dictionary<float, float> _noiseValues = new Dictionary<float, float>();

        [Range(5, 15)]
        public byte elevationScale = 20;
        /// <summary>
        /// 海拔，海洋为0，海岸线为1
        /// </summary>
        private float[,] _cornerElevations;

        /// <summary>
        /// 设置决定四边形属性的方式
        /// </summary>
        public delegate bool SetIsIslandDelegate (uint x, uint y);
        private Dictionary<byte, SetIsIslandDelegate> _setIsIslandMethods;

        /// <summary>
        /// 设置海拔的方式
        /// </summary>
        public delegate float SetElevationDelegate ( );
        private Dictionary<byte, SetElevationDelegate> _setElevationMethods;

        /// <summary>
        /// 岛半径
        /// </summary>
        public float radius;
        [System.NonSerialized]
        public float normalizedRadius;

        /// <summary>
        /// 栅格个数
        /// </summary>
        public uint m;
        [System.NonSerialized]
        public uint n;

        private Raster _raster;

        /// <summary>
        /// 噪声
        /// </summary>
        private PerlinNoise1D _noise1D;
        private PerlinNoise2D _noise2D;

        /// <summary>
        /// 设置海拔，距离海岸线越远，海拔越高
        /// </summary>
        /// <returns>返回最高点</returns>
        private float _LandRiseElevation ( )
        {

            // 待设置的拐角
            Queue<RasterIndex> cornerQueue = new Queue<RasterIndex>(_coastCornerList);

            // 某一圈的数量
            int count = cornerQueue.Count;

            // 离海洋的距离，从1开始
            float dis = 1;

            // 广度优先搜索，每次加入一个内圈的
            while (cornerQueue.Count > 0)
            {
                // 上一圈已经全部搞定
                if (count == 0)
                {
                    count = cornerQueue.Count;
                    dis += 0.1f;
                }

                RasterIndex corner = cornerQueue.Dequeue();
                count--;

                uint x = corner.x;
                uint y = corner.y;
                _cornerElevations[x, y] = dis;

                // 周围4个点是否是land？是否已加入下一圈？已加入设为255
                if (_cornerTypes[x + 1, y] == CornerType.kLand && _cornerElevations[x + 1, y] == 0)
                {
                    _cornerElevations[x + 1, y] = byte.MaxValue;
                    cornerQueue.Enqueue(new RasterIndex(x + 1, y));
                }
                if (_cornerTypes[x, y + 1] == CornerType.kLand && _cornerElevations[x, y + 1] == 0)
                {
                    _cornerElevations[x, y + 1] = byte.MaxValue;
                    cornerQueue.Enqueue(new RasterIndex(x, y + 1));
                }
                if (_cornerTypes[x - 1, y] == CornerType.kLand && _cornerElevations[x - 1, y] == 0)
                {
                    _cornerElevations[x - 1, y] = byte.MaxValue;
                    cornerQueue.Enqueue(new RasterIndex(x - 1, y));
                }
                if (_cornerTypes[x, y - 1] == CornerType.kLand && _cornerElevations[x, y - 1] == 0)
                {
                    _cornerElevations[x, y - 1] = byte.MaxValue;
                    cornerQueue.Enqueue(new RasterIndex(x, y - 1));
                }

            }

            return dis;

        }

        /// <summary>
        /// 平坦地形，中间几个山
        /// </summary>
        private float _FlatElevation ( )
        {
            float maxOffset = Mathf.Min(m, n) * radius / Mathf.Min(mapSize.x, mapSize.y);
            byte count = (byte)(Random.value * 5);
            for (byte i = 0; i < count; i++)
            {

            }

            return 1;
        }

        /// <summary>
        /// 设置高度图
        /// </summary>
        /// <param name="terrainData">地形数据</param>
        private void _SetHeight (TerrainData terrainData, float maxHeight)
        {
            int hmr = terrainData.heightmapResolution;
            float[,] heights = new float[hmr, hmr];

            // 每栅格采样点数
            byte sample = (byte)((hmr - 1) / m);
            float pixelSize = _squareSize / sample;

            for (uint i = 0; i < hmr; i++)
            {
                for (uint j = 0; j < hmr; j++)
                {

                    // 边界
                    if (i < sample || j < sample || i >= hmr - 1 - sample || j >= hmr - 1 - sample)
                    {
                        heights[i, j] = 0;
                        continue;
                    }

                    // 落到中心及周围8个栅格中的某一个，x、y是栅格的序号
                    uint x = j / sample;
                    uint y = i / sample;

                    // 坐标 = 序号 * 像素大小
                    float xPoint = (float)j * pixelSize;
                    float yPoint = (float)i * pixelSize;

                    // 落到哪里就用哪个的四个拐角插值
                    // 先判断是否在中间的栅格
                    if (RMGUtility.PointInSquare(
                        _raster.SquareCorners[x, y],
                        _raster.SquareCorners[x + 1, y],
                        _raster.SquareCorners[x + 1, y + 1],
                        _raster.SquareCorners[x, y + 1],
                        xPoint, yPoint))
                    {
                        if (_centerTypes[x, y] == CenterType.kLand)
                            heights[i, j] = _Lerp4Height(x, y, xPoint, yPoint) / maxHeight;
                    }
                    else
                    {
                        // 不在中间
                        // 退到左上角
                        x--;
                        y--;
                        for (byte p = 0; p < 3; p++)
                        {
                            for (byte q = 0; q < 3; q++)
                            {
                                // 中间的不用测试
                                if (p == 1 && q == 1)
                                    continue;

                                uint xx = x + p;
                                uint yy = y + q;

                                // 判断是否在四边形内
                                if (RMGUtility.PointInSquare(
                                    _raster.SquareCorners[xx, yy],
                                    _raster.SquareCorners[xx + 1, yy],
                                    _raster.SquareCorners[xx + 1, yy + 1],
                                    _raster.SquareCorners[xx, yy + 1],
                                    xPoint, yPoint))
                                {
                                    if (_centerTypes[xx, yy] == CenterType.kOcean)
                                        heights[i, j] = 0;
                                    else
                                        heights[i, j] = _Lerp4Height(xx, yy, xPoint, yPoint) / maxHeight;

                                    continue;
                                }
                            }
                        }
                    }
                }
            }

            terrain.terrainData.SetHeightsDelayLOD(0, 0, heights);
        }

        private float _Lerp4Height (uint x, uint y, float px, float py)
        {
            return RMGUtility.LerpSquare(
                _raster.SquareCorners[x, y], _cornerElevations[x, y],
                _raster.SquareCorners[x + 1, y], _cornerElevations[x + 1, y],
                _raster.SquareCorners[x + 1, y + 1], _cornerElevations[x + 1, y + 1],
                _raster.SquareCorners[x, y + 1], _cornerElevations[x, y + 1],
                px, py) - 1;
        }

        /// <summary>
        /// 近似的任意四边形插值，顺时针，权值为距顶点的距离
        /// </summary>
        /// <param name="x">左上角x栅格号</param>
        /// <param name="y">左上角y栅格号</param>
        /// <returns></returns>
        private float _LerpInSquareSimple (uint x, uint y, float corX, float corY)
        {
            Vector2 p = new Vector2(corX, corY);
            float ap = Vector2.Distance(_raster.SquareCorners[x, y], p);
            float bp = Vector2.Distance(_raster.SquareCorners[x + 1, y], p);
            float cp = Vector2.Distance(_raster.SquareCorners[x + 1, y + 1], p);
            float dp = Vector2.Distance(_raster.SquareCorners[x, y + 1], p);
            float s = ap + bp + cp + dp;
            return
                (1 - ap / s) * _cornerElevations[x, y] +
                (1 - bp / s) * _cornerElevations[x + 1, y] +
                (1 - cp / s) * _cornerElevations[x + 1, y + 1] +
                (1 - dp / s) * _cornerElevations[x, y + 1];

        }

        /// <summary>
        /// 扫描，以获取海岸线上的点，设置每个四边形中心、拐角属性
        /// </summary>
        private void _SetCoastPoint ( )
        {
            _centerTypes = new CenterType[m, n];
            _cornerTypes = new CornerType[m + 1, n + 1];
            _coastCornerList = new List<RasterIndex>();
            _cornerElevations = new float[m + 1, n + 1];
            for (uint i = 0; i < m; i++)
            {
                for (uint j = 0; j < n; j++)
                {
                    if (i == 0 || j == 0)
                    {
                        _centerTypes[i, j] = CenterType.kOcean;

                        // 边界
                        _cornerTypes[i, j] = CornerType.kOcean;
                    }
                    else
                    {
                        bool island = _setIsIslandMethods[(byte)islandType](i, j);
                        if (island)
                            _centerTypes[i, j] = CenterType.kLand;
                        else
                            _centerTypes[i, j] = CenterType.kOcean;

                        // 每个Corner的属性需要周围4个四边形确定
                        if (i > 1)
                        {
                            bool topLeft = IsLand(i - 2, j - 1);
                            bool topRight = IsLand(i - 1, j - 1);
                            bool bottomLeft = IsLand(i - 1, j - 1);
                            bool bottomRight = IsLand(i - 1, j);
                            // 全是陆地
                            if (topLeft && topRight && bottomLeft && bottomRight)
                            {
                                _cornerTypes[i - 1, j] = CornerType.kLand;
                                if (elevationType == ElevationType.kFlat)
                                    _cornerElevations[i - 1, j] = 2;
                            }
                            // 全是海洋
                            else if (!(topLeft || topRight || bottomLeft || bottomRight))
                                _cornerTypes[i - 1, j] = CornerType.kOcean;
                            else
                            {
                                _cornerTypes[i - 1, j] = CornerType.kCoast;
                                _cornerElevations[i - 1, j] = 1;
                                // 用于计算海拔
                                _coastCornerList.Add(new RasterIndex(i - 1, j));
                            }
                        }
                    }

                }
            }

            // 还剩倒数第二行未处理
            for (uint j = 1; j < n; j++)
            {
                bool topLeft = IsLand(m - 2, j - 1);
                bool topRight = IsLand(m - 1, j - 1);
                bool bottomLeft = IsLand(m - 1, j - 1);
                bool bottomRight = IsLand(m - 1, j);
                if (topLeft && topRight && bottomLeft && bottomRight)
                {
                    _cornerTypes[m - 1, j] = CornerType.kLand;
                    if (elevationType == ElevationType.kFlat)
                        _cornerElevations[m - 1, j] = 2;
                }
                else if (!(topLeft || topRight || bottomLeft || bottomRight))
                    _cornerTypes[m - 1, j] = CornerType.kOcean;
                else
                {
                    _cornerTypes[m - 1, j] = CornerType.kCoast;
                    _cornerElevations[m - 1, j] = 1;

                    // 用于计算海拔
                    _coastCornerList.Add(new RasterIndex(m - 1, j));
                }
            }

            // 边界
            for (uint i = 0; i < m + 1; i++)
            {
                _cornerTypes[i, n] = CornerType.kOcean;
            }
            for (uint j = 0; j < n + 1; j++)
            {
                _cornerTypes[m, j] = CornerType.kOcean;
            }

            // 重排
        }

        /// <summary>
        /// 某条边是否是海岸线
        /// </summary>
        private bool _AvailableCoastPoint (uint curX, uint curY, uint nextX, uint nextY)
        {
            // 上下
            if (nextX == curX)
            {
                uint y = nextY < curY ? nextY : curY;
                // 左，右四边形
                CenterType left = _centerTypes[curX - 1, y];
                CenterType right = _centerTypes[curX, y];
                return
                    left == CenterType.kLand && right == CenterType.kOcean ||
                    left == CenterType.kOcean && right == CenterType.kLand;
            }
            // 左右
            else if (nextY == curY)
            {
                uint x = nextX < curX ? nextX : curX;
                // 上，下四边形
                CenterType up = _centerTypes[x, curY - 1];
                CenterType down = _centerTypes[x, curY];
                return
                    up == CenterType.kLand && down == CenterType.kOcean ||
                    up == CenterType.kOcean && down == CenterType.kLand;
            }
            else
                return false;

        }

        /// <summary>
        /// 随机化海岸线
        /// </summary>
        private void _NoiseEdge ( )
        {

        }

        public bool IsLand (uint x, uint y)
        {
            Debug.Assert(_centerTypes[x, y] != CenterType.kNone);
            return _centerTypes[x, y] != CenterType.kOcean;
        }

        /// <summary>
        /// 判断四边形是否是陆地
        /// </summary>
        private bool _IsInsideCircleIsland (uint x, uint y)
        {

            int cor_x = (int)x - (int)_centerIndex.x;
            int cor_y = (int)y - (int)_centerIndex.y;

            // 弧度
            float angle = Mathf.Atan2(cor_y, cor_x);
            if (angle < 0)
                angle = 2 * Mathf.PI + angle;

            // 该点到圆周的最大距离
            float noiseValue = 0;
            if (!_noiseValues.ContainsKey(angle))
            {
                noiseValue = _noise1D.GetPixel(angle);
                _noiseValues.Add(angle, noiseValue);
            }
            else
            {
                noiseValue = _noiseValues[angle];
            }

            return Vector2.Distance(_raster.SquareCenters[x, y], _centerPoint) < noiseValue + radius;
        }

        /// <summary>
        /// 判断四边形是否为陆地
        /// </summary>
        private bool _IsInsidePerlinIsland (uint x, uint y)
        {

            // 归一化距栅格中心的距离
            float xc = (float)x * 2.0f / (float)m - 1;
            float yc = (float)y * 2.0f / (float)n - 1;
            float dis = xc * xc + yc * yc;

            // 我也不知道这是为什么，Ivan.Z差不多这么干的
            // Mathf.PerlinNoise( )，unity自带的就是辣鸡
            return _noise2D.GetPixel(x, y) > dis;
        }

        #region implement MonoBehaviour

        // Awake is called when the script instance is being loaded
        public void Awake ( )
        {

        }

        // Start is called just before any of the Update methods is called the first time
        public void Start ( )
        {

            // 初始化栅格
            Rect border = new Rect(Vector2.zero, mapSize);
            _squareSize = border.size.x / m;
            n = (uint)(border.size.y / _squareSize);
            _raster = new Raster(border, m, 0.5f);

            float minSize = Mathf.Min(mapSize.x, mapSize.y) * 0.48f;
            radius = Mathf.Min(minSize, radius);
            normalizedRadius = radius / minSize;

            // 默认整个栅格的中心
            _centerIndex = new RasterIndex(m / 2, n / 2);
            _centerPoint = _raster.GetSquareCenter(_centerIndex);

            _setElevationMethods = new Dictionary<byte, SetElevationDelegate>();
            _setElevationMethods.Add((byte)ElevationType.kFlat, _FlatElevation);
            _setElevationMethods.Add((byte)ElevationType.kRise, _LandRiseElevation);

            _setIsIslandMethods = new Dictionary<byte, SetIsIslandDelegate>();
            _setIsIslandMethods.Add((byte)IslandType.kCircle, _IsInsideCircleIsland);
            _setIsIslandMethods.Add((byte)IslandType.kPerlin, _IsInsidePerlinIsland);

            float maxHeight = 0;

            if (islandType == IslandType.kCircle)
            {
                _noise1D = new PerlinNoise1D(ampl: 100, count: 8, freq: 10);
                _SetCoastPoint();
                maxHeight = _setElevationMethods[(byte)elevationType]();
                _SetHeight(terrain.terrainData, maxHeight);
            }
            else if (islandType == IslandType.kPerlin)
            {
                _noise2D = new PerlinNoise2D((uint)mapSize.x, (uint)mapSize.y);
                _SetCoastPoint();
                maxHeight = _setElevationMethods[(byte)elevationType]();
                _SetHeight(terrain.terrainData, maxHeight);
            }

            //Test();
        }

        #endregion

        private void Test ( )
        {
            Vector2 A = new Vector2(4, 3);
            Vector2 B = new Vector2(22, 3);
            Vector2 C = new Vector2(25, 30);
            Vector2 D = new Vector2(5, 28);
            Vector2 E = new Vector2(55, 2);
            Vector2 F = new Vector2(50, 32);
            float a = Random.value;
            float b = Random.value;
            float c = Random.value;
            float d = Random.value;
            float e = Random.value;
            float f = Random.value;
            print(a + " " + b + " " + c + " " + d);
            print(RMGUtility.LerpSquare(A, a, B, b, C, c, D, d, B.x, B.y));
            print(b + " " + e + " " + f + " " + c);
            float[,] heights = new float[65, 65];

            for (int i = 0; i < 65; i++)
            {
                for (int j = 0; j < 65; j++)
                {
                    if (RMGUtility.PointInSquare(A, B, C, D, j, i))
                    {
                        heights[i, j] = RMGUtility.LerpSquare(A, a, B, b, C, c, D, d, j, i);
                    }
                    if (RMGUtility.PointInSquare(B, E, F, C, j, i))
                    {
                        heights[i, j] = RMGUtility.LerpSquare(B, b, E, e, F, f, C, c, j, i);
                    }
                }
            }
            TerrainData trdata = new TerrainData();
            trdata.heightmapResolution = 65;
            trdata.size = new Vector3(100, 20, 100);
            trdata.SetHeights(0, 0, heights);
            GameObject tr = Terrain.CreateTerrainGameObject(trdata);
        }


    }

}