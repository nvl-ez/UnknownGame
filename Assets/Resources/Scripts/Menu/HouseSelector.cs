using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proyect
{
    public class HouseSelector : MonoBehaviour
    {
        public GameObject[] houses;
        public Material lightsUp;
        public Material lightsDown;
        public GameObject panel;

        void Update() {
            if (Input.GetMouseButtonDown(0)) {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit)) {
                    Transform clickedTransform = hit.transform;
                    if (clickedTransform != null && clickedTransform.IsChildOf(transform)) {
                        HouseWindow houseWindow = clickedTransform.GetComponent<HouseWindow>();
                        if (houseWindow != null)
                        {
                            clearLights();

                            Renderer window = clickedTransform.GetComponent<HouseWindow>().window.GetComponent<Renderer>();
                            window.material = lightsUp;

                            panel.SetActive(true);
                        }
                    }
                }
            }
        }
        void clearLights() {
            for (int i = 0; i < houses.Length; i++) {
                Renderer window = houses[i].GetComponent<HouseWindow>().window.GetComponent<Renderer>();
                window.material = lightsDown;
            }
        }
    }

    
}
