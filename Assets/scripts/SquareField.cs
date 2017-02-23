using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SquareField {
    const int SIZE_X = 2;
    const int SIZE_Y = 1;

    private Dictionary<uint, SpaceObject> objects;

    public SquareField() {
        objects = new Dictionary<uint, SpaceObject>();
    }

    public void Spawn() {
        float vol = GetSphereVolume(1000); // 1km.
        for (int x = 0; x < SIZE_X; x++) {
            for (int y = 0; y < SIZE_Y; y++) {
                int xPos = (x - SIZE_X / 2) * 2;
                int yPos = (y - SIZE_Y / 2) * 2;
                GameObject obj = GameObject.Instantiate(PlanetSimulator.Instance.prefab, new Vector3(xPos, yPos), Quaternion.identity);
                obj.transform.parent = PlanetSimulator.Instance.objectParent;
                uint ind = GetIndex(x, y);
                SpaceObject spaceObj = new SpaceObject(ind, obj.transform, vol, (float)Densities.Copper);
                objects[ind] = spaceObj;
            }
        }
    }

    public SpaceObject[] GetMeteors() {
        return objects.Values.ToArray();
    }

    public void Step(float d) {
        foreach (SpaceObject obj in objects.Values) {
            obj.Update(d);
        }
    }

    public void SubmitCollison(uint id, uint otherId) {
        if (objects.ContainsKey(id) && objects.ContainsKey(otherId)) {
            objects[id].AddVolume(objects[otherId].Volume);
            objects[id].Velocity += objects[otherId].Velocity;
            UnityEngine.Object.Destroy(objects[otherId].Trans.gameObject);
            objects.Remove(otherId);
        }
    }

    private uint GetIndex(int x, int y) {
        return (uint)(x * SIZE_Y + y);
    }

    private float GetSphereVolume(int radius) {
        return (4 / 3) * Mathf.PI * Mathf.Pow(radius, 3);
    }

    int Exponent(float d) {
        return (int)Mathf.Log10(Mathf.Abs(d));
    }

    float Mantissa(float d, int exp) {
        return d / Mathf.Pow(10, exp);
    }
}
