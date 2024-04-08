using System.Collections.Generic;
using UnityEngine;

public class GravityAreaDirection : GravityArea
{
    public Vector3 direction = new Vector3(0, -1, 0);
    public override Vector3 GetGravityDirection(GravityBody _gravityBody)
    {
        return (direction).normalized;
    }
}
