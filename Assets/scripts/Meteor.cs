using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceObject {
    public const float G = 0.0000000000667408f;
    public uint ID { get; private set; }
    public Transform Trans { get; set; }
    public SpaceObjectScript Script { get; set; }
    /// <summary>
    /// Scaled position of the object in meters.
    /// </summary>
    public Vector3 Position
    {
        get
        {
            Vector3 pos = Trans.position;
            return PlanetSimulator.Instance.ToScaledPosition(pos.x, pos.y, pos.z);
        }
    }
    /// <summary>
    /// Velocity of the object in meters/s.
    /// </summary>
    public Vector3 Velocity { get; set; }
    /// <summary>
    /// Gravitational force of the object in meters/s/s.
    /// </summary>
    public Vector3 Gravity { get; set; }
    /// <summary>
    /// Volume of the object in meters^3.
    /// </summary>
    public float Volume { get; private set; }
    /// <summary>
    /// Density of the object in kg/m^3.
    /// </summary>
    public float Density { get; private set; }
    /// <summary>
    /// Mass of the object in kilograms.
    /// </summary>
    public float Mass { get; private set; }

    /// <summary>
    /// Create a space object.
    /// </summary>
    /// <param name="trans">Volume of the object in meters^3</param>
    /// <param name="volume">Density of the object in kg/m^3.</param>
    /// <param name="density">Mass of the object in kilograms.</param>
    public SpaceObject(uint id, Transform trans, float volume, float density) {
        ID = id;
        Trans = trans;
        Script = Trans.GetComponent<SpaceObjectScript>();
        Script.ID = ID;
        Volume = volume;
        Density = density;
        Mass = Volume * Density;
        UpdateScale();
    }

    public void Update(float time) {
        Vector3 position = Position;
        if (IsNAN(position)) {
            //Debug.Log(ID + " position NAN.");
            return;
        }
        if (IsNAN(Gravity)) {
            //Debug.Log(ID + " Gravity NAN.");
            return;
        }
        Velocity += (Gravity * time);
        if (IsNAN(Velocity)) {
            //Debug.Log(ID + " Velocity NAN.");
            return;
        }
        position += (Velocity * time);
        Trans.position = PlanetSimulator.Instance.ToUnityPosition(position.x, position.y, position.z);
    }

    public void AddVolume(float v) {
        Volume += v;
        Mass = Volume * Density;
        UpdateScale();
    }

    public void UpdateScale() {
        Script.SetRadius(GetRadius(Volume));
    }

    public void SetOrbit(SpaceObject target, Vector2 rotDir) {
        float oV = Mathf.Sqrt((G * target.Mass) / Vector3.Distance(target.Position, Position)) / 1.3f;
        Vector3 dir = (target.Position - Position).normalized;
        dir = new Vector3(dir.y * rotDir.x, dir.x * rotDir.y, 0);
        Velocity = dir * oV;
    }

    private bool IsNAN(Vector2 testVect) {
        return (float.IsNaN(testVect.x) || float.IsNaN(testVect.y));
    }

    private float GetRadius(float volume) {
        return Mathf.Pow((volume / 3.14159265359f) * (3 / (float)4), 1 / (float)3);
    }
}
