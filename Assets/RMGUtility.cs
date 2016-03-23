/*************************************************************

** Auth: ysd
** Date: 12/11/2015 16:15
** Desc: ������
** Vers: v1.0

*************************************************************/

using UnityEngine;

namespace RandomMapGenerator
{

    static public class RMGUtility
    {

        /// <summary>
        /// �⻬��ֵ
        /// </summary>
        /// <returns>���⻬��t</returns>
        static public float SmoothCurve (float t)
        {
            t = Mathf.Min(1.0f, t);
            t = Mathf.Max(0.0f, t);
            float ttt = t * t * t;
            return ttt * (3.0f * t * (2.0f * t - 5.0f) + 10.0f);
        }


        /// <summary>
        /// ���Բ�ֵ
        /// </summary>
        /// <param name="t">��from�ľ���</param>
        static public float Lerp (float from, float to, float t)
        {
            return (1 - t) * from + t * to;
        }

        /// <summary>
        /// ����a��b����ĳ��ȣ�Ҳ��ab��ɵ�ƽ���ı��ε����
        /// </summary>
        /// <returns>��b��a���Ҳ࣬�������0</returns>
        static public float CrossV2Magnitude (Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        /// <summary>
        /// ����㵽ֱ�ߵľ���
        /// </summary>
        /// <param name="corX">Ŀ����x����</param>
        /// <param name="corY">Ŀ����y����</param>
        /// <param name="line">ֱ�߷���</param>
        /// <param name="ox">ֱ����һ��</param>
        /// <param name="oy">ֱ����һ��</param>
        /// <returns></returns>
        static public float DistancePoint2Line2D (float corX, float corY, Vector2 line, float ox, float oy)
        {
            float dirX = corX - ox;
            float dirY = corY - oy;
            line = line.normalized;
            // ��� = ���
            float s = dirX * line.y - dirY * line.x;
            // ������Եױ�
            return Mathf.Abs(s);
        }

        /// <summary>
        /// �����ı�����?;
        /// A---0--->B
        /// �� �K0  1�L 1
        /// |   Pt   |
        /// 3 �J3  �I2 ��
        /// D��----2--C
        /// </summary>
        static public bool PointInSquare (Vector2 A, Vector2 B, Vector2 C, Vector2 D, float xp, float yp)
        {
            // P �� Q = x1 * y2 - y1 * x2
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
        /// �����ı��β�ֵ��ͨ���������ı������ųɾ���ʵ�֣���!!
        /// </summary>
        static public float LerpSquare (Vector2 A, float a, Vector2 B, float b, Vector2 C, float c, Vector2 D, float d, float px, float py)
        {
            // ��������
            Vector2 AB = B - A;
            Vector2 AD = D - A;

            // ������
            Vector2 newP = _NewCoordinats(A, AB, AD, px, py);
            Vector2 newC = _NewCoordinats(A, AB, AD, C.x, C.y);

            // ����
            newP.x /= newC.x;
            newP.y /= newC.y;

            // ��ֵ
            return Lerp(Lerp(a, b, newP.x), Lerp(d, c, newP.x), newP.y);
        }

        /// <summary>
        /// ������ϵ�µ�������
        /// </summary>
        /// <param name="O">����ԭ��</param>
        /// <param name="X">X��</param>
        /// <param name="Y">Y��</param>
        /// <param name="px">x����</param>
        /// <param name="py">y����</param>
        /// <returns></returns>
        static private Vector2 _NewCoordinats (Vector2 O, Vector2 X, Vector2 Y, float px, float py)
        {

            // |AN||AM|Sin��AN��AM�� = AN �� AM = ƽ���ı���ANPM��� = 
            // P��AB���� * |AN�� = P��AD���� * |AM|

            // ������н�
            float radians = Vector2.Angle(X, Y) * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);

            // �㵽X�����
            float dis = DistancePoint2Line2D(px, py, X, O.x, O.y);
            // �µĺ�����
            float an = dis / sin;

            // �㵽Y�����
            dis = DistancePoint2Line2D(px, py, Y, O.x, O.y);
            // �µ�������
            float am = dis / sin;

            return new Vector2(am, an);

        }

    }

}

