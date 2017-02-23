using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceObjectScript : MonoBehaviour {
    public float minSize = 25;
    public uint ID;
    public Renderer render;
    public SphereCollider coll;

    private Color nColor;
    private Color sColor;

    public void SetRadius(float radius) {
        float localRadius = Mathf.Max(minSize / 2, radius / PlanetSimulator.Instance.metersPmeter * PlanetSimulator.Instance.SizeScale);
        coll.radius = localRadius;
        render.material.SetFloat("_Scale", localRadius * 2);
    }

    public void SetColors(Color normal, Color selected) {
        nColor = normal;
        sColor = selected;
        render.material.SetColor("_Color", nColor);
    }

    public void SetSelected(bool selected) {
        if (selected)
            render.material.SetColor("_Color", sColor);
        else
            render.material.SetColor("_Color", nColor);
    }

    /*public void OnTriggerEnter(Collider other) {
        return;
        if (other.tag.ToLower() == "spaceobject") {
            SpaceObjectScript obj = other.GetComponent<SpaceObjectScript>();
            if (obj) {
                PlanetSimulator.Instance.Collision(ID, obj.ID);
            }
        }
    }*/
}
