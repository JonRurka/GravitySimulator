using UnityEngine;
using System.Collections;

public class GravSim : MonoBehaviour {

    public int numAsteroids = 0;
    public ComputeShader shader;

    private System.Diagnostics.Stopwatch watch;

    private struct AsteroidData {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Mass;

        public AsteroidData(Vector2 pos, Vector2 vel, float mass) {
            Position = pos;
            Velocity = vel;
            Mass = mass;
        }
    }

	// Use this for initialization
	void Start () {

        AsteroidData[] asteroids = new AsteroidData[numAsteroids];
        Vector2[] outAsteroids = new Vector2[numAsteroids];
        for (int i = 0; i < numAsteroids; i++) {
            asteroids[i] = new AsteroidData(new Vector2(Random.Range(-100f, 100f), Random.Range(-100f, 100f)), new Vector2(0, 0), 1);
        }

        watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        LinearGravity(asteroids);

        watch.Stop();
        Debug.LogFormat("processed {0} objects in: {1}", asteroids.Length, watch.Elapsed.TotalMilliseconds);
        watch.Reset();
        watch.Start();

        float sq = NthRoot(numAsteroids, 1);
        float asixNumThreads = sq;

        Debug.Log(asixNumThreads);
        int numGroups = (int)System.Math.Ceiling((float)asixNumThreads / 256);
        Debug.Log(numGroups);

        ComputeBuffer bufferIn = new ComputeBuffer(numAsteroids, 20);
        ComputeBuffer bufferOut = new ComputeBuffer(numAsteroids, 8);
        bufferIn.SetData(asteroids);
        int kernal = shader.FindKernel("GravityComp");
        shader.SetBuffer(kernal, "dataIn", bufferIn);
        shader.SetBuffer(kernal, "dataOut", bufferOut);
        shader.SetFloat("numAsteroids", numAsteroids);
        shader.Dispatch(kernal, numGroups, 1, 1);

        watch.Stop();
        Debug.Log("shader execution time: " + watch.Elapsed.Milliseconds);
        watch.Reset();
        watch.Start();

        bufferOut.GetData(outAsteroids);

        watch.Stop();
        Debug.Log("get data time: " + watch.Elapsed.Milliseconds);

        //Debug.Log("shader execution time: " + (watch.ElapsedTicks / (float)System.Diagnostics.Stopwatch.Frequency));
        
        bufferIn.Dispose();
        bufferOut.Dispose();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    private Vector2[] LinearGravity(AsteroidData[] data) {
        int num = data.Length;
        Vector2[] gravity = new Vector2[num];
        for (int i = 0; i < num; i++) {
            Vector2 result = Vector2.zero;
            for (int j = 0; j < num; j++) {
                float distance = Vector2.Distance(data[i].Position, data[j].Position);
                float newt = (data[i].Mass * data[j].Mass) / Mathf.Pow(distance, 2);
                float acc = newt / data[i].Mass;
                Vector2 dir = (data[i].Position - data[j].Position).normalized;
                Vector2 grav = dir * acc;
                result = result + grav;
            }
            gravity[i] = result;
        }
        return gravity;
    }

    public static float NthRoot(float a, int n) {
        return Mathf.Pow(a, 1.0f / n);
    }
}
