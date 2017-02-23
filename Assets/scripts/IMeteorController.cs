using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ObjectController {
    protected Dictionary<uint, SpaceObject> objects;

    protected ObjectController() {
        objects = new Dictionary<uint, SpaceObject>();
    }

    public abstract void Spawn();

    public abstract SpaceObject CreateObject(Vector3 pos, float volume, float density);

    public virtual SpaceObject[] GetMeteors() {
        return objects.Values.ToArray();
    }

    public virtual void Step(float d) {
        float seconds = (PlanetSimulator.Instance.TimeScale * PlanetSimulator.SECONDS_IN_WEEK);
        float time = d * seconds;
        PlanetSimulator.Instance.totalSeconds += (int)time;
        foreach (SpaceObject obj in objects.Values) {
            obj.Update(time);
        }
    }
    
    public abstract void SubmitCollison(uint id, uint otherId);
}

public enum Densities {
    Copper = 8960,
}