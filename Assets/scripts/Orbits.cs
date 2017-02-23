using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Orbits : ObjectController {
    public const float SUN_V = 1400000000000000000000000000f;
    public const float SUN_D = 1420.7142857142857142857142857143f;
    public const float G = 0.0000000000667408f;

    public const float Planet_V = 6370000;

    public override void Spawn() {
        SpaceObject star1 = CreateStar(new Vector3(300, 0, 0));
        //SpaceObject star2 = CreateStar(new Vector3(-300, 0, 0));
        //star1.SetOrbit(star2, new Vector2(-1, 1));
        //star2.SetOrbit(star1, new Vector2(-1, 1));
        float range = 5000;
        System.Random rand = new System.Random(System.DateTime.Now.Millisecond);
        for (int i = 0; i < 50; i++) {
            float val = Random.value;
            //SpaceObject obj = val > 0.5 ? star1 : star2;
            SpaceObject obj = star1;
            //Vector2 rotDir = val > 0.5 ? new Vector2(1, -1) : new Vector2(-1, 1);
            float r = Random.Range(0, range);
            float dirX = (Random.Range(-1, (float)1));
            float dirZ = (Random.Range(-1, (float)1));
            Vector2 rotDir = new Vector2(1, -1);
            SpaceObject p = CreatePlanet(new Vector3(dirX, 0, dirZ) * r + obj.Trans.position);
            float oV = Mathf.Sqrt((G * obj.Mass) / Vector2.Distance(obj.Position, p.Position));
            Vector3 dir = (obj.Position - p.Position).normalized;
            dir = new Vector3(dir.z * rotDir.x + ((float)rand.NextDouble() / 2 * rand.Next(-1, 2)),
                              0,
                              dir.x * rotDir.y + ((float)rand.NextDouble() / 2 * rand.Next(-1, 2)));
            p.Velocity = dir * oV;
        }
    }

    public override void SubmitCollison(uint id, uint otherId) {
        if (objects.ContainsKey(id) && objects.ContainsKey(otherId)) {
            uint bID;
            uint sID;
            GetBiggerVolume(id, objects[id].Volume, otherId, objects[otherId].Volume, out bID, out sID);
            //float newV_x = -GetVelocity(objects[bID].Mass, objects[sID].Mass, objects[bID].Velocity.x, objects[sID].Velocity.x);
            //float newV_y = -GetVelocity(objects[bID].Mass, objects[sID].Mass, objects[bID].Velocity.y, objects[sID].Velocity.y);
            objects[bID].AddVolume(objects[sID].Volume);
            //objects[bID].Velocity += new Vector2(newV_x, newV_y);
            Object.Destroy(objects[sID].Trans.gameObject);
            objects.Remove(sID);
        }
    }

    public SpaceObject CreateStar(Vector3 pos) {
        SpaceObject obj = CreateObject(pos, SUN_V, SUN_D);
        obj.Trans.name = "Star";
        obj.Script.SetColors(Color.yellow, Color.yellow);
        return obj;
    }

    public SpaceObject CreatePlanet(Vector3 pos) {
        SpaceObject obj = CreateObject(pos, Planet_V, SUN_D);
        obj.Trans.name = "Planet";
        obj.Script.SetColors(new Color32(135, 206, 235, 255), Color.red);
        return obj;
    }

    public override SpaceObject CreateObject(Vector3 pos, float volume, float density) {
        GameObject SunObj = GameObject.Instantiate(PlanetSimulator.Instance.prefab, pos, Quaternion.identity);
        SunObj.transform.parent = PlanetSimulator.Instance.objectParent;
        uint ind = GetNewID();
        SpaceObject spaceObj = new SpaceObject(ind, SunObj.transform, volume, density);
        objects.Add(ind, spaceObj);
        return spaceObj;
    }

    private uint GetNewID() {
        uint id = (uint)UnityEngine.Random.Range(0, uint.MaxValue);
        if (objects.ContainsKey(id))
            id = GetNewID();
        return id;
    }

    private float GetVelocity(float m1, float m2, float v1, float v2) {
        return ((m1 * v1) + (m2 * v2)) / (m1 + m2);
    }

    private void GetBiggerVolume(uint id1, float v1, uint id2, float v2, out uint bID, out uint sID) {
        if (v1 >= v2) {
            bID = id1;
            sID = id2;
        }
        else {
            bID = id2;
            sID = id1;
        }
    }
}
