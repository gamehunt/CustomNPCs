using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

namespace NPCS.Utils
{
        public class SerializableVector2
        {

            public SerializableVector2()
            {

            }

            public SerializableVector2(Vector2 vec)
            {
                x = vec.x;
                y = vec.y;
            }

            public float x { get; set; }
            public float y { get; set; }

            public Vector2 ToVector2()
            {
                return new Vector2(x, y);
            }
        }

        public class SerializableVector3
        {

            public SerializableVector3()
            {

            }

            public SerializableVector3(Vector3 vec)
            {
                x = vec.x;
                y = vec.y;
                z = vec.z;
            }

            public float x { get; set; }
            public float y { get; set; }
            public float z { get; set; }

            public Vector3 ToVector3()
            {
                return new Vector3(x, y, z);
            }
        }

}
