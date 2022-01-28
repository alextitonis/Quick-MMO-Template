using System;
using UnityEngine;

namespace GameServer
{
    #region 3D
    [Serializable]
    public class Box
    {
        public Vector3 min;
        public Vector3 max;
        public Vector3 center;
    }
    [Serializable]
    public class Sphere
    {
        public Vector3 center;
        public float radius;
    }
    #endregion
    #region 2D
    [Serializable]
    public class Box2D
    {
        public Vector2 min;
        public Vector2 max;
        public Vector2 center;
    }
    [Serializable]
    public class Circle
    {
        public Vector2 center;
        public float radius;
    }
    #endregion
}