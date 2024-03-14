using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]

public class GravityBody : MonoBehaviour
{
    public GravityAttractor planet;
    private void Start() {
        planet = GameObject.FindGameObjectWithTag("Planet").GetComponent<GravityAttractor>();
        Rigidbody rigidbody = this.gameObject.GetComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void FixedUpdate() {
        planet.Attract(transform);
    }
}
