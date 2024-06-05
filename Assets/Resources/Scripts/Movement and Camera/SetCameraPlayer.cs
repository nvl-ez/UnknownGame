using Cinemachine;
using FishNet.Managing;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace Proyect
{
    public class SetCameraPlayer : NetworkBehaviour
    {

        [Client(RequireOwnership = true)]
        public override void OnStartClient() {
            base.OnStartClient();

            Camera camera = Camera.main;
            camera.GetComponent<CinemachineBrain>().m_WorldUpOverride = this.transform;

            CinemachineFreeLook cfl = GameObject.FindGameObjectWithTag("FreeLookCamera").GetComponent<CinemachineFreeLook>();

            cfl.Follow = this.transform;
            cfl.LookAt = this.transform.GetChild(0);
        }
    }
}
