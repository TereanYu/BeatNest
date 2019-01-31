using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Simple class containing a GameObject and an index.
/// Used to synchronize a GameObject to a frame from RhythmTool.
/// </summary>
public class Line : MonoBehaviour {

    public int index { get; private set; }

    public void Init(Color color, float opacity, int index)
    {
        this.index = index;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        color = Color.Lerp(Color.black, color, opacity * .01f);
        meshRenderer.material.SetColor("_TintColor", color);

        transform.localScale = new Vector3(.5f, 10, .5f);
    }
}
