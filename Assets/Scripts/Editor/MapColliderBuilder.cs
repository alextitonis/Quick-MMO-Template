using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapColliderBuilder : MonoBehaviour
{
    static List<BoxCollider> getBoxColliders()
    {
        List<BoxCollider> colliders = new List<BoxCollider>();

        object[] objs = FindObjectsOfType<BoxCollider>();

        foreach (object obj in objs)
        {
            if (obj != null)
                colliders.Add((BoxCollider)obj);
        }

        return colliders;
    }
    static List<SphereCollider> getSphereColliders()
    {
        List<SphereCollider> colliders = new List<SphereCollider>();

        object[] objs = FindObjectsOfType<SphereCollider>();

        foreach (object obj in objs)
        {
            if (obj != null)
                colliders.Add((SphereCollider)obj);
        }

        return colliders;
    }
    static List<CapsuleCollider> getCapsuleColliders()
    {
        List<CapsuleCollider> colliders = new List<CapsuleCollider>();

        object[] objs = FindObjectsOfType<CapsuleCollider>();

        foreach (object obj in objs)
        {
            if (obj != null)
                colliders.Add((CapsuleCollider)obj);
        }

        return colliders;
    }
    static List<MeshCollider> getMeshColliders()
    {
        List<MeshCollider> colliders = new List<MeshCollider>();

        object[] objs = FindObjectsOfType<MeshCollider>();

        foreach (object obj in objs)
        {
            if (obj != null)
                colliders.Add((MeshCollider)obj);
        }

        return colliders;
    }
    static List<TerrainCollider> getTerrainColliders()
    {
        List<TerrainCollider> colliders = new List<TerrainCollider>();

        object[] objs = FindObjectsOfType<TerrainCollider>();

        foreach (object obj in objs)
        {
            if (obj != null)
                colliders.Add((TerrainCollider)obj);
        }

        return colliders;
    }
    static List<WheelCollider> getWheelColliders()
    {
        List<WheelCollider> colliders = new List<WheelCollider>();

        object[] objs = FindObjectsOfType<WheelCollider>();

        foreach (object obj in objs)
        {
            if (obj != null)
                colliders.Add((WheelCollider)obj);
        }

        return colliders;
    }

    public static void DestroyColliders()
    {
        foreach (var c in getBoxColliders())
            Destroy(c);

        foreach (var c in getSphereColliders())
            Destroy(c);

        foreach (var c in getCapsuleColliders())
            Destroy(c);

        foreach (var c in getMeshColliders())
            Destroy(c);
    }

    public static void ExportData()
    {
        Debug.ClearDeveloperConsole();

        #region Box_Colliders
        string path = Directory.GetCurrentDirectory() + "/Colliders/Box Colliders/";

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        string fileName = "";
        int i = 0;

        foreach (var c in getBoxColliders())
        {
            fileName = i + ".bin";

            if (File.Exists(path + fileName))
                File.Delete(path + fileName);

            using (BinaryWriter writer = new BinaryWriter(new FileStream(path + fileName, FileMode.Create)))
            {
                writer.Write(c.transform.position.x);
                writer.Write(c.transform.position.y);
                writer.Write(c.transform.position.z);

                writer.Write(c.transform.rotation.x);
                writer.Write(c.transform.rotation.y);
                writer.Write(c.transform.rotation.z);
                writer.Write(c.transform.rotation.w);

                writer.Write(c.center.x);
                writer.Write(c.center.y);
                writer.Write(c.center.z);

                writer.Write(c.size.x);
                writer.Write(c.size.y);
                writer.Write(c.size.z);

                writer.Write(c.isTrigger);
            }

            i++;
        }

        Debug.Log("Found " + i + " Box Colliders!");
        #endregion
        #region Sphere_Colliders
        path = Directory.GetCurrentDirectory() + "/Colliders/Sphere Colliders/";

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        fileName = "";
        i = 0;

        foreach (var c in getSphereColliders())
        {
            fileName = i + ".bin";

            if (File.Exists(path + fileName))
                File.Delete(path + fileName);

            using (BinaryWriter writer = new BinaryWriter(new FileStream(path + fileName, FileMode.Create)))
            {
                writer.Write(c.transform.position.x);
                writer.Write(c.transform.position.y);
                writer.Write(c.transform.position.z);

                writer.Write(c.transform.rotation.x);
                writer.Write(c.transform.rotation.y);
                writer.Write(c.transform.rotation.z);
                writer.Write(c.transform.rotation.w);

                writer.Write(c.center.x);
                writer.Write(c.center.y);
                writer.Write(c.center.z);

                writer.Write(c.radius);

                writer.Write(c.isTrigger);
            }

            i++;
        }

        Debug.Log("Found " + i + " Sphere Colliders!");
        #endregion
        #region Capsule_Colliders
        path = Directory.GetCurrentDirectory() + "/Colliders/Capsule Colliders/";

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        fileName = "";
        i = 0;

        foreach (var c in getCapsuleColliders())
        {
            fileName = i + ".bin";

            if (File.Exists(path + fileName))
                File.Delete(path + fileName);

            using (BinaryWriter writer = new BinaryWriter(new FileStream(path + fileName, FileMode.Create)))
            {
                writer.Write(c.transform.position.x);
                writer.Write(c.transform.position.y);
                writer.Write(c.transform.position.z);

                writer.Write(c.transform.rotation.x);
                writer.Write(c.transform.rotation.y);
                writer.Write(c.transform.rotation.z);
                writer.Write(c.transform.rotation.w);

                writer.Write(c.center.x);
                writer.Write(c.center.y);
                writer.Write(c.center.z);

                writer.Write(c.radius);
                writer.Write(c.height);
                writer.Write(c.direction);

                writer.Write(c.isTrigger);
            }

            i++;
        }

        Debug.Log("Found " + i + " Capsule Colliders!");
        #endregion
        #region Mesh_Colliders
        path = Directory.GetCurrentDirectory() + "/Colliders/Mesh Colliders/";

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        fileName = "";
        i = 0;

        foreach (var c in getMeshColliders())
        {
            fileName = i + ".bin";

            if (File.Exists(path + fileName))
                File.Delete(path + fileName);

            using (BinaryWriter writer = new BinaryWriter(new FileStream(path + fileName, FileMode.Create)))
            {
                writer.Write(c.transform.position.x);
                writer.Write(c.transform.position.y);
                writer.Write(c.transform.position.z);

                writer.Write(c.transform.rotation.x);
                writer.Write(c.transform.rotation.y);
                writer.Write(c.transform.rotation.z);
                writer.Write(c.transform.rotation.w);

                writer.Write(c.isTrigger);

                Mesh m = c.sharedMesh;
                writer.Write(m.vertexBufferCount);
                writer.Write(m.vertexCount);

                writer.Write(m.vertices.Length);

                foreach (var v in m.vertices)
                {
                    writer.Write(v.x);
                    writer.Write(v.y);
                    writer.Write(v.z);
                }
            }

            i++;
        }

        Debug.Log("Found " + i + " Mesh Colliders!");
        #endregion
        #region Terrain_Colliders
        path = Directory.GetCurrentDirectory() + "/Colliders/Terrain Colliders/";

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        fileName = "";
        i = 0;

        foreach (var c in getTerrainColliders())
        {
            fileName = i + ".bin";

            if (File.Exists(path + fileName))
                File.Delete(path + fileName);

            using (BinaryWriter writer = new BinaryWriter(new FileStream(path + fileName, FileMode.Create)))
            {
                writer.Write(c.transform.position.x);
                writer.Write(c.transform.position.y);
                writer.Write(c.transform.position.z);

                writer.Write(c.transform.rotation.x);
                writer.Write(c.transform.rotation.y);
                writer.Write(c.transform.rotation.z);
                writer.Write(c.transform.rotation.w);

                writer.Write(c.isTrigger);

                TerrainData t = c.terrainData;
                writer.Write(t.size.x);
                writer.Write(t.size.y);
                writer.Write(t.size.z);

                writer.Write(t.heightmapResolution);
                writer.Write(t.heightmapResolution);
                writer.Write(t.heightmapResolution);

                writer.Write(t.heightmapScale.x);
                writer.Write(t.heightmapScale.y);
                writer.Write(t.heightmapScale.z);
            }

            i++;
        }

        Debug.Log("Found " + i + " Terrain Colliders!");
        #endregion
        #region Wheel_Colliders
        path = Directory.GetCurrentDirectory() + "/Colliders/Wheel Colliders/";

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        fileName = "";
        i = 0;

        foreach (var c in getWheelColliders())
        {
            fileName = i + ".bin";

            if (File.Exists(path + fileName))
                File.Delete(path + fileName);

            using (BinaryWriter writer = new BinaryWriter(new FileStream(path + fileName, FileMode.Create)))
            {
                writer.Write(c.transform.position.x);
                writer.Write(c.transform.position.y);
                writer.Write(c.transform.position.z);

                writer.Write(c.transform.rotation.x);
                writer.Write(c.transform.rotation.y);
                writer.Write(c.transform.rotation.z);
                writer.Write(c.transform.rotation.w);

                writer.Write(c.mass);
                writer.Write(c.radius);
                writer.Write(c.wheelDampingRate);
                writer.Write(c.suspensionDistance);
                writer.Write(c.forceAppPointDistance);

                writer.Write(c.center.x);
                writer.Write(c.center.y);
                writer.Write(c.center.z);
            }

            i++;
        }

        Debug.Log("Found " + i + " Wheel Colliders!");
        #endregion
    }
}