using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Proyect
{
    public class LoadUnloadScenes : MonoBehaviour
    {
        public string sceneToLoad; // Name of the scene to load
        public TextMeshProUGUI text;

        void Start() {
            // Get the Button component attached to this GameObject
            Button button = GetComponent<Button>();

            // Set the onClick listener
            button.onClick.AddListener(OnPanelClick);
        }

        void OnPanelClick() {
            text.text = "Loading";
            SceneManager.LoadScene(sceneToLoad);

        }
    }
}
