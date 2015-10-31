/*************************************************************

** Auth: ysd
** Date: 15.10.30
** Desc: 栅格中的四边形
** Vers: v1.0

*************************************************************/

using UnityEngine;
using System.Collections;

using RandomMapGenerator;

namespace RandomMapGenerator
{



    public class Square
    {

        private Vector2 _center;

        private Vector2[] _corners;

        private Square[] _neighbours;

        public Square ( )
        {
            _corners = new Vector2[4];
            _neighbours = new Square[4];
        }

    }

}