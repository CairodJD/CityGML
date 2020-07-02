using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//https://www.habrador.com/tutorials/math/
public class Plane : MonoBehaviour{
    public Vector3 pos;

    public Vector3 normal;

    public Plane(Vector3 pos, Vector3 normal) {
        this.pos = pos;

        this.normal = normal;
    }
}
