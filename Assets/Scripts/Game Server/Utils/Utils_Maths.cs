using System;
using UnityEngine;

namespace GameServer
{
    public class Utils_Maths
    {
        public static bool PointIsInsideBox(Vector3 point, Box box)
        {
            return (point.x >= box.min.x && point.x <= box.max.x) &&
           (point.y >= box.min.y && point.y <= box.max.y) &&
           (point.z >= box.min.z && point.z <= box.max.z);
        }
        public static bool PointIsInsideSphere(Vector3 point, Sphere sphere)
        {
            double distance = Math.Pow((point.x - sphere.center.x), 2) + Math.Pow((point.y - sphere.center.y), 2) + Math.Pow((point.z - sphere.center.z), 2);

            return distance <= Math.Pow(sphere.radius, 2);
        }

        public static bool PointIsInsideBox2D(Vector2 point, Box2D box)
        {
            return (point.x >= box.min.x && point.x <= box.max.x) &&
                (point.y >= box.min.y && point.y <= box.max.y);
        }
        public static bool PointIsInsideCircle(Vector2 point, Circle circle)
        {
            double distance = Math.Pow(point.x - circle.center.x, 2) * Math.Pow(point.y - circle.center.y, 2);

            return distance <= Math.Pow(circle.radius, 2);
        }

        public static bool CloseEnough(Vector3 value1, Vector3 value2, double acceptableDifference)
        {
            bool ok = false;
            if (Mathf.Abs(value1.x - value2.x) <= acceptableDifference)
                ok = true;
            else if (Mathf.Abs(value1.y - value2.y) <= acceptableDifference)
                ok = true;
            else if (Mathf.Abs(value1.z - value2.z) <= acceptableDifference)
                ok = true;

            return ok;
        }
        public static bool CloseEnough(Quaternion value1, Quaternion value2, double acceptableDifference)
        {
            bool ok = false;
            if (Mathf.Abs(value1.x - value2.x) <= acceptableDifference)
                ok = true;
            else if (Mathf.Abs(value1.y - value2.y) <= acceptableDifference)
                ok = true;
            else if (Mathf.Abs(value1.z - value2.z) <= acceptableDifference)
                ok = true;
            else if (Mathf.Abs(value1.w - value2.w) <= acceptableDifference)
                ok = true;

            return ok;
        }
    }
}
