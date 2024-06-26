using Proyect;
using UnityEngine;

public class GravitySource : MonoBehaviour {
    public int priority = 0;
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