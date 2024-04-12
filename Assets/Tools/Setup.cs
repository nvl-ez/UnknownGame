using System.IO;
using UnityEngine;
using UnityEditor;
public static class Setup {
    [MenuItem("Tools/Setup/Create Default Folders")]
    public static void CreateDefaultFolders() {
        Folders.CreateDefault("Resources", "Art", "Prefabs", "Scripts");
        UnityEditor.AssetDatabase.Refresh();
    }

    static class Folders {
        public static void CreateDefault(string root, params string[] folders) {
            string fullpath = Path.Combine(Application.dataPath, root);
            
            foreach (string folder in folders) {
                string path = Path.Combine(fullpath, folder);
                if(!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }
            }
        }
    }
}