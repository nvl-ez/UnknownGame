using Proyect;
using UnityEngine;

public class GravitySource : MonoBehaviour {

    public float priority;

    void OnEnable() {
        CustomGravity.Register(this);
    }

    void OnDisable() {
        CustomGravity.Unregister(this);
    }

    public virtual Vector3 GetGravity(Vector3 position) {
        return Physics.gravity;
    }
}