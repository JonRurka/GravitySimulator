using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSimulator : MonoBehaviour {
    public const int SECONDS_IN_WEEK = 2419199;
    public const float G = 0.0000000000667408f;

    struct IN {
        public float x;
        public float y;
        public float z;
        public float mass;
        public float volume;

        public IN(Vector3 pos, float mass, float volume) {
            x = pos.x;
            y = pos.y;
            z = pos.z;
            this.mass = mass;
            this.volume = volume;
        }
    }

    public static PlanetSimulator Instance { get; private set; }

    public int numObjects = 0;

    public ComputeShader shader;

    public GameObject prefab;
    public Transform objectParent;

    /// <summary>
    /// km per meter.
    /// </summary>
    public float Distance_Scale = 1000;

    /// <summary>
    /// object size multiplyer.
    /// </summary>
    public float SizeScale = 10;

    /// <summary>
    /// Years per second.
    /// </summary>
    public float TimeScale = 1;

    public int execTime = 0;
    public long years = 0;

    public float metersPmeter { get; private set; }
    public long totalSeconds { get; set;}


    private ObjectController meteorController;
    private ComputeBuffer bufferIn;
    private ComputeBuffer bufferOut;
    private System.Diagnostics.Stopwatch watch;

    private int frameCount = 0;

    // Use this for initialization
    void Start () {
        Instance = this;
        watch = new System.Diagnostics.Stopwatch();
        meteorController = new Orbits();
        metersPmeter = Distance_Scale * 1000;
        SpawnMeteors();
        //Step();
    }
	
	// Update is called once per frame
	void Update () {
        frameCount++;
        if (frameCount > 4) {
            Step();
        }
    }

    private void Step() {
        SpaceObject[] meteors = meteorController.GetMeteors();
        numObjects = meteors.Length;
        //Vector3[] gravity = ProcessGravity_Linear(GetData(meteors));
        Vector4[] gravity = ProcessGravity(GetData(meteors));
        for (int i = 0; i < meteors.Length; i++) {
            meteors[i].Gravity = new Vector3(gravity[i].x, gravity[i].y, gravity[i].z);
            if (gravity[i].w > -1) {
                int other = Mathf.RoundToInt(gravity[i].w);
                meteorController.SubmitCollison(meteors[i].ID, meteors[other].ID);
            }
        }
        float d = Time.deltaTime;
        meteorController.Step(d);
        //Debug.Log(d);
        years = (long)(totalSeconds * 3.171e-8d);
    }

    public Vector3 ToScaledPosition(float x, float y, float z) {
        return new Vector3(x * metersPmeter, y * metersPmeter, z * metersPmeter);
    }

    public Vector3 ToUnityPosition(float x, float y, float z) {
        return new Vector3(x / metersPmeter, y / metersPmeter, z / metersPmeter);
    }

    public void Collision(uint id, uint other) {
        meteorController.SubmitCollison(id, other);
    }

    private IN[] GetData(SpaceObject[] objects) {
        IN[] data = new IN[objects.Length];
        for (int i = 0; i < objects.Length; i++) {
            data[i] = new IN(objects[i].Position, objects[i].Mass, objects[i].Volume);
        }
        return data;
    }

    private Vector4[] ProcessGravity(IN[] data) {
        if (data.Length <= 0)
            return new Vector4[0];
        watch.Reset();
        watch.Start();
        Vector4[] gravity = new Vector4[data.Length];
        bufferIn = new ComputeBuffer(data.Length, 4 * 5);
        bufferOut = new ComputeBuffer(data.Length, 4 * 4);
        bufferIn.SetData(data);
        int kernal = shader.FindKernel("GravityComp");
        shader.SetBuffer(kernal, "dataIn", bufferIn);
        shader.SetBuffer(kernal, "dataOut", bufferOut);
        shader.SetInt("numAsteroids", data.Length);
        shader.SetFloat("scale", SizeScale);
        int numGroups = (int)System.Math.Ceiling((float)data.Length / 256);
        shader.Dispatch(kernal, numGroups, 1, 1);
        bufferOut.GetData(gravity);
        bufferIn.Dispose();
        bufferOut.Dispose();
        watch.Stop();
        execTime = watch.Elapsed.Milliseconds;
        return gravity;
    }

    private Vector3[] ProcessGravity_Linear(Vector3[] data) {
        Vector3[] result = new Vector3[data.Length];
        for (int ind = 0; ind < data.Length; ind++) {
            Vector2 gravResult = Vector2.zero;
            Vector2 pos = new Vector2(data[ind].x, data[ind].y);
            for (int i = 0; i < data.Length; i++) {
                if (ind == i)
                    continue;
                Vector2 other = new Vector2(data[i].x, data[i].y);
                float newt = GetNewtons(data[ind], data[i]);
                float acc = newt / data[ind].z;
                Vector2 dir = -(pos - other).normalized;
                Vector2 grav = dir * acc;
                gravResult += grav;
            }
            result[ind] = new Vector3(gravResult.x, gravResult.y, -1);
        }
        return result;
    }

    private float GetNewtons(Vector3 obj1, Vector3 obj2) {
        Vector2 pos1 = new Vector2(obj1.x, obj1.y);
        Vector2 pos2 = new Vector2(obj2.x, obj2.y);
        float distance = Vector2.Distance(pos1, pos2);
        Debug.Log(distance);
        return (G * obj1.z * obj2.z) / Mathf.Pow(distance, 2);
    }

    private void SpawnMeteors() {
        meteorController.Spawn();
    }
}
