/*************************************************************

** Auth: ysd
** Date: 31/10/2015 14:10
** Desc: ≤‚ ‘
** Vers: v1.0

*************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using RandomMapGenerator;

namespace RandomMapGenerator
{

    public class test : MonoBehaviour
    {

        Dictionary<float, float> _vs = new Dictionary<float, float>();

        void Start ( )
        {
            PerlinNoise noise = new PerlinNoise(to: Mathf.PI * 2, ampl:5, count: 8, freq:10);
            for (float x = 0; x <= Mathf.PI * 2; x += 0.002f)
            {
                _vs.Add(x, noise.PerlinNoise1D(x));
            }
        }

        // Implement this OnDrawGizmosSelected if you want to draw gizmos only if the object is selected
        public void OnDrawGizmos ( )
        {
            foreach (var v in _vs)
            {
                Gizmos.DrawSphere(new Vector2(v.Key, v.Value), 0.01f);
            }
        }



    }

}
