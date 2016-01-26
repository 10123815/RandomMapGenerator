/*************************************************************

** Auth: ysd
** Date: 15.10.30
** Desc: 产生一个栅格并随机化
** Vers: v1.0

*************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using RandomMapGenerator;

namespace RandomMapGenerator
{

    public struct RasterIndex : IEqualityComparer<RasterIndex>
    {
        public uint x;
        public uint y;
        public RasterIndex (uint xx, uint yy)
        {
            x = xx;
            y = yy;
        }

        #region IEqualityComparer<RasterIndex> 成员

        bool IEqualityComparer<RasterIndex>.Equals (RasterIndex obj1, RasterIndex obj2)
        {
            return obj1.x == obj2.x && obj1.y == obj2.y;
        }

        /// <summary>
        /// 只要 x1, x2 < 100000，可以保证 y1 * 100000 + x1 != y2 * 100000 + x2；
        /// 实际上，x12，y12为栅格坐标，不会大于100000.
        /// </summary>
        int IEqualityComparer<RasterIndex>.GetHashCode (RasterIndex obj)
        {
            return (obj.x + obj.y * 100000).GetHashCode();
        }

        #endregion
    }

    public class Raster
    {

        private Vector2[,] _squareCenters;
        /// <summary>
        /// 栅格的中心
        /// </summary>
        public Vector2[,] SquareCenters
        {
            get
            {
                return _squareCenters;
            }
        }

        private Vector2[,] _squareCorners;
        /// <summary>
        /// 栅格四边形的拐角
        /// </summary>
        public Vector2[,] SquareCorners
        {
            get
            {
                return _squareCorners;
            }
        }

        private uint _m;
        /// <summary>
        /// 横向个数
        /// </summary>
        public uint m
        {
            get
            {
                return _m;
            }
        }

        private uint _n;
        /// <summary>
        /// 纵向个数
        /// </summary>
        public uint n
        {
            get
            {
                return _n;
            }
        }

        private Rect _border;

        private float _squareSize;

        /// <summary>
        /// 产生一个随机化的栅格，栅格的拐角将移动到周围的一个随机点上
        /// </summary>
        /// <param name="border">地图边界</param>
        /// <param name="widthNumber">横向栅格个数</param>
        /// <param name="rand">栅格拐角移动的距离</param>
        public Raster (Rect border, uint widthNumber, float rand = 0.3f)
        {
            _border = border;
            _m = widthNumber;
            _squareSize = border.size.x / _m;
            _n = (uint)(border.size.y / _squareSize);

            _squareCenters = new Vector2[widthNumber, _n];
            _squareCorners = new Vector2[widthNumber + 1, _n + 1];

            // 左面的点不能移动到右面的点的右面
            rand = Mathf.Min(0.45f, rand);
            rand = Mathf.Max(0, rand);

            _Gird();
            _RandomAdjust(rand * _squareSize);
        }

        /// <summary>
        /// 生成网格
        /// </summary>
        /// <param name="_border"></param>
        /// <param name="m"></param>
        /// <param name="n"></param>
        private void _Gird ( )
        {
            float width = _border.width / m;
            float height = _border.height / n;
            _squareCenters[0, 0] = _border.position + new Vector2(width / 2, -height / 2);
            Vector2 offset = Vector2.zero;
            for (uint i = 0; i < m + 1; i++)
            {
                for (uint j = 0; j < n + 1; j++)
                {
                    offset.x = width * i;
                    offset.y = height * j;
                    if (i < m && j < n)
                        _squareCenters[i, j] = _squareCenters[0, 0] + offset;
                    _squareCorners[i, j] = _border.position + offset;
                }
            }

        }

        private void _RandomAdjust (float randOffset)
        {
            for (uint i = 1; i < m; i++)
            {
                for (uint j = 1; j < n; j++)
                {
                    Vector2 dir = Random.insideUnitCircle;
                    _squareCorners[i, j] += dir * randOffset;
                }
            }

            // 调整中心的位置
            for (uint i = 0; i < m; i++)
            {
                for (uint j = 0; j < n; j++)
                {
                    _squareCenters[i, j].x = (
                        _squareCorners[i, j].x +
                        _squareCorners[i + 1, j].x +
                        _squareCorners[i, j + 1].x +
                        _squareCorners[i + 1, j + 1].x
                        ) / 4;

                    _squareCenters[i, j].y = (
                        _squareCorners[i, j].y +
                        _squareCorners[i + 1, j].y +
                        _squareCorners[i, j + 1].y +
                        _squareCorners[i + 1, j + 1].y
                        ) / 4;
                }
            }
        }

        /// <summary>
        /// 四边形的四个顶点，左上0，顺时针
        /// </summary>
        public Vector2[] GetSquareCorner (uint i, uint j)
        {
            Vector2[] vertexes = new Vector2[4];
            vertexes[0] = _squareCorners[i, j];
            vertexes[1] = _squareCorners[i + 1, j];
            vertexes[2] = _squareCorners[i + 1, j + 1];
            vertexes[3] = _squareCorners[i, j + 1];
            return vertexes;
        }

        public Vector2[] GetSquareCorner (RasterIndex index)
        {
            return GetSquareCorner(index.x, index.y);
        }

        public Vector2 GetSquareCenter (RasterIndex index)
        {
            return SquareCenters[index.x, index.y];
        }

        /// <summary>
        /// 用栅格逼近圆
        /// </summary>
        /// <param name="center">圆心</param>
        /// <param name="radius">圆半径</param>
        /// <returns>栅格的标号，只需第一象限的1/4</returns>
        public RasterIndex[] GetCircleIndices (RasterIndex center, float radius)
        {
            float minSize = Mathf.Min(_border.size.x, _border.size.y);
            radius = Mathf.Min(minSize * 0.9f, radius);
            uint radiusNumber = (uint)(radius / _squareSize);
            
            List<RasterIndex> result = new List<RasterIndex>();

            // 第一个栅格的位置
            RasterIndex index = new RasterIndex();
            index.x = center.x;
            index.y = center.y - radiusNumber;
            result.Add(index);

            while (index.x <= center.x + radiusNumber && index.y <= center.y)
            {
                // 向右下方
                RasterIndex right = new RasterIndex(index.x + 1, index.y);

                RasterIndex down = new RasterIndex(index.x, index.y + 1);

                RasterIndex rightDown = new RasterIndex(index.x + 1, index.y + 1);

                index = right;
                float dis = Vector2.Distance(GetSquareCenter(index), GetSquareCenter(center));
                float minDiff = Mathf.Abs(dis - radius);

                dis = Vector2.Distance(GetSquareCenter(down), GetSquareCenter(center));
                float diff = Mathf.Abs(dis - radius);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    index = down;
                }

                dis = Vector2.Distance(GetSquareCenter(rightDown), GetSquareCenter(center));
                diff = Mathf.Abs(dis - radius);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    index = rightDown;
                }

                result.Add(index);
            }

            return result.ToArray();
        }


    }

}