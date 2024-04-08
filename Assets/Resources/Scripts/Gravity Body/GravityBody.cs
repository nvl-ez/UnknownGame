using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class GravityBody : MonoBehaviour
{
    public static float GRAVITY_FORCE = 8;
    
    public Vector3 GravityDirection
    {
        get
        {
            if (_gravityAreas.Count == 0) return (Vector3.zero-transform.position).normalized;
            _gravityAreas.Sort((area1, area2) => area1.Priority.CompareTo(area2.Priority));
            return _gravityAreas.Last().GetGravityDirection(this).normalized;
        }
    }

    public bool grabValues = false;
    public Vector3 force;

    private Rigidbody _rigidbody;
    private List<GravityArea> _gravityAreas;

    void Awake()
    {
        _rigidbody = transform.GetComponent<Rigidbody>();
        _gravityAreas = new List<GravityArea>();
    }

    private void FixedUpdate() {
        force = GravityDirection * (GRAVITY_FORCE);
        if(!grabValues)_rigidbody.AddForce(force, ForceMode.Acceleration);

        Quaternion upRotation = Quaternion.FromToRotation(transform.up, -GravityDirection);
        Quaternion newRotation = Quaternion.Slerp(_rigidbody.rotation, upRotation * _rigidbody.rotation, 0.3f);
        if (!grabValues) _rigidbody.MoveRotation(newRotation);
    }

    public void AddGravityArea(GravityArea gravityArea)
    {
        _gravityAreas.Add(gravityArea);
    }

    public void RemoveGravityArea(GravityArea gravityArea)
    {
        _gravityAreas.Remove(gravityArea);
    }
}