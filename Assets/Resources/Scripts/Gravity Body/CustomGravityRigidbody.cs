using Proyect;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomGravityRigidbody : MonoBehaviour {

    Rigidbody body;
    [SerializeField]
    bool floatToSleep = false;

    float floatDecay;

    void Awake() {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
    }

    void FixedUpdate() {
        if (floatToSleep) {
            if (body.IsSleeping()) {
                floatDecay = 0f;
                return;
            }

            if (body.velocity.sqrMagnitude < 0.0001f) {
                floatDecay += Time.deltaTime;
                if (floatDecay > 1f) return;
            } else {
                floatDecay = 0f;
            }
        }
        body.AddForce(CustomGravity.GetGravity(body.position), ForceMode.Acceleration);
    }
}