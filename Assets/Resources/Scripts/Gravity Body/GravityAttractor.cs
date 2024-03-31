using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityAttractor : MonoBehaviour
{
    public float gravity = -10;

    public void Attract(Transform body) {
        Vector3 targetDir = (body.position - transform.position).normalized;

        Vector3 bodyUp = body.up;

        body.rotation = Quaternion.FromToRotation(bodyUp, targetDir)*body.rotation;

        body.gameObject.GetComponent<Rigidbody>().AddForce(targetDir*gravity);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
