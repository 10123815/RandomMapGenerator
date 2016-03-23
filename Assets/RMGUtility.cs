/*************************************************************

** Auth: ysd
** Date: 12/11/2015 16:15
** Desc: 工具类
** Vers: v1.0

*************************************************************/

using UnityEngine;

namespace RandomMapGenerator
{

    static public class RMGUtility
    {

        /// <summary>
        /// 光滑插值
        /// </summary>
        /// <returns>更光滑的t</returns>
        static public float SmoothCurve (float t)
        {
            t = Mathf.Min(1.0f, t);
            t = Mathf.Max(0.0f, t);
            float ttt = t * t * t;
            return ttt * (3.0f * t * (2.0f * t - 5.0f) + 10.0f);
        }


        /// <summary>
        /// 线性插值
        /// </summary>
        /// <param name="t">距from的距离</param>
        static public float Lerp (float from, float to, float t)
        {
            return (1 - t) * from + t * to;
        }

        /// <summary>
        /// 计算a和b叉积的长度，也是ab组成的平心四边形的面积
        /// </summary>
        /// <returns>若b在a的右侧，结果大于0</returns>
        static public float CrossV2Magnitude (Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        /// <summary>
        /// 计算点到直线的距离
        /// </summary>
        /// <param name="corX">目标点的x坐标</param>
        /// <param name="corY">目标点的y坐标</param>
        /// <param name="line">直线方向</param>
        /// <param name="ox">直线上一点</param>
        /// <param name="oy">直线上一点</param>
        /// <returns></returns>
        static public float DistancePoint2Line2D (float corX, float corY, Vector2 line, float ox, float oy)
        {
            float dirX = corX - ox;
            float dirY = corY - oy;
            line = line.normalized;
            // 叉积 = 面积
            float s = dirX * line.y - dirY * line.x;
            // 面积除以底边
            return Mathf.Abs(s);
        }

        /// <summary>
        /// 点在四边形内?;
        /// A---0--->B
        /// ↑ ↘0  1↙ 1
        /// |   Pt   |
        /// 3 ↗3  ↖2 ↓
        /// D←----2--C
        /// </summary>
        static public bool PointInSquare (Vector2 A, Vector2 B, Vector2 C, Vector2 D, float xp, float yp)
        {
            // P × Q = x1 * y2 - y1 * x2
            float abx = B.x - A.x;
            float aby = B.y - A.y;
            float apx = xp - A.x;
            float apy = yp - A.y;

            float bcx = C.x - B.x;
            float bcy = C.y - B.y;
            float bpx = xp - B.x;
            float bpy = yp - B.y;

            float cdx = D.x - C.x;
            float cdy = D.y - C.y;
            float cpx = xp - C.x;
            float cpy = yp - C.y;

            float dax = A.x - D.x;
            float day = A.y - D.y;
            float dpx = xp - D.x;
            float dpy = yp - D.y;

            return
                abx * apy - aby * apx >= 0 &&
                bcx * bpy - bcy * bpx >= 0 &&
                cdx * cpy - cdy * cpx >= 0 &&
                dax * dpy - day * dpx >= 0;
        }

        /// <summary>
        /// 任意四边形插值，通过将任意四边形缩放成矩形实现，慢!!
        /// </summary>
        static public float LerpSquare (Vector2 A, float a, Vector2 B, float b, Vector2 C, float c, Vector2 D, float d, float px, float py)
        {
            // 新坐标轴
            Vector2 AB = B - A;
            Vector2 AD = D - A;

            // 新坐标
            Vector2 newP = _NewCoordinats(A, AB, AD, px, py);
            Vector2 newC = _NewCoordinats(A, AB, AD, C.x, C.y);

            // 缩放
            newP.x /= newC.x;
            newP.y /= newC.y;

            // 插值
            return Lerp(Lerp(a, b, newP.x), Lerp(d, c, newP.x), newP.y);
        }

        /// <summary>
        /// 新坐标系下的新坐标
        /// </summary>
        /// <param name="O">坐标原点</param>
        /// <param name="X">X轴</param>
        /// <param name="Y">Y轴</param>
        /// <param name="px">x坐标</param>
        /// <param name="py">y坐标</param>
        /// <returns></returns>
        static private Vector2 _NewCoordinats (Vector2 O, Vector2 X, Vector2 Y, float px, float py)
        {

            // |AN||AM|Sin（AN，AM） = AN × AM = 平行四边形ANPM面积 = 
            // P到AB距离 * |AN｜ = P到AD距离 * |AM|

            // 坐标轴夹角
            float radians = Vector2.Angle(X, Y) * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);

            // 点到X轴距离
            float dis = DistancePoint2Line2D(px, py, X, O.x, O.y);
            // 新的横坐标
            float an = dis / sin;

            // 点到Y轴距离
            dis = DistancePoint2Line2D(px, py, Y, O.x, O.y);
            // 新的纵坐标
            float am = dis / sin;

            return new Vector2(am, an);

        }

    }

}

