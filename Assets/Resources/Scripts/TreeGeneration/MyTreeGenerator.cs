using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using static UnityEngine.GridBrushBase;

namespace Proyect
{
    public class MyTreeGenerator : MonoBehaviour
    {
        
        [Range(3, 40)]
        public int faces;
        public float radius;
        public int floors;
        public float floorHeight;
        [Range(0f, 1f)]
        public float reductionRate;
        [Range(0f, 100f)]
        public float rootTwistiness;
        [Range(0f, 100f)]
        public float branchTwistiness;
        [Range(0, 10)]
        public int detail;

        protected MeshFilter meshFilter;

        void Awake()
        {

            Mesh mesh = new Mesh();
            TreeBranch tree = GenerateTree(faces, floors, floorHeight, radius, reductionRate, rootTwistiness, branchTwistiness,Vector3.zero, detail);
            mesh.vertices = tree.vertices.ToArray();
            mesh.triangles = tree.triangles.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
        }

        public TreeBranch GenerateTree(int faces, int floors, float floorHeight, float radius, float reductionRate, float rootTwistiness, float branchTwistiness, Vector3 origin, int detail) {
            if (detail == 0 || faces < 3 || radius < 0.02f) {
                return null; // Termination condition
            }

            
            // Generate trunk
            TreeBranch trunk = new TreeBranch(faces, floors, floorHeight, radius, reductionRate, rootTwistiness, branchTwistiness, origin);

            // Generate branches with reduced complexity
            int facesBranch1 = Random.Range(3, faces);
            int facesBranch2 = (faces+2) - facesBranch1;

            //Generate the radius of the branches
            float radiusBranch1 = trunk.finalRadius * ((float)facesBranch1 / (float)faces);
            float radiusBranch2 = trunk.finalRadius * ((float)facesBranch2 / (float)faces);

            //Displacement
            Vector3 direction = new Vector3(-1, 0, 1);
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, trunk.growthEnd);
            Vector3 displacement1 = rotation*direction * (trunk.finalRadius - radiusBranch1) + trunk.growthEnd;
            Vector3 displacement2 = rotation *(- direction) * (trunk.finalRadius - radiusBranch2) + trunk.growthEnd;


            // Recursive call to generate smaller trees as branches
            TreeBranch branch1 = GenerateTree(facesBranch1, Random.Range(1, floors), floorHeight, radiusBranch1, reductionRate, rootTwistiness, branchTwistiness, displacement1, detail - 1);
            TreeBranch branch2 = GenerateTree(facesBranch2, Random.Range(1, floors), floorHeight, radiusBranch2, reductionRate, rootTwistiness, branchTwistiness, displacement2, detail - 1);

            // Weave branches into a single tree structure
            if (branch1 == null || branch2 == null) {
                return trunk; // If no more branches, return trunk as the tree
            } else {
                return TreeBranch.weeveBranches(trunk, branch1, branch2);
            }
        }
    }

    public class TreeBranch {
        public List<Vector3> vertices;
        public List<int > triangles;
        public int branchFaces;
        public float finalRadius;

        private int nfloors;

        public Vector3 growthEnd;


        public TreeBranch(int faces,  int floors, float floorHeight, float radius, float reductionRate, float baseTwistiness, float branchTwistiness,  Vector3 origin) {
            branchFaces = faces;
            nfloors = floors;
            vertices = GenerateVertices(faces, floors, floorHeight, radius, reductionRate, baseTwistiness, branchTwistiness, origin, out growthEnd, out finalRadius);
            triangles = GenerateTris(faces, floors);
        }

        public int[] getTopVerticesIndices() {
            int[] topVerticesIndex = new int[branchFaces];

            for(int i = 0; i < branchFaces; i++) {
                topVerticesIndex[branchFaces-1-i] = triangles[triangles.Count- 3 -6*i];
            }
            return topVerticesIndex;
        }

        public int[] getBottomVerticesIndices() {
            int[] bottomVerticesIndex = new int[branchFaces];

            for (int i = 0; i < branchFaces; i++) {
                bottomVerticesIndex[i] = triangles[6*i];
            }
            return bottomVerticesIndex;
        }

        private List<int> GenerateTris(int faces, int floors) {
            List<int> tris = new List<int>();
            int t = 0; // Triangle index tracker
            for (int i = 0; i < faces * floors; i++) {
                int floor = i / faces;
                int face = i % faces;
                int current = floor * faces + face;
                int next = current + faces;
                int nextFace = (face + 1) % faces;

                // Lower triangle
                tris.Add(current);
                tris.Add(next);
                tris.Add(floor * faces + nextFace);

                // Upper triangle
                tris.Add(next);
                tris.Add((floor + 1) * faces + nextFace);
                tris.Add(floor * faces + nextFace);
            }
            return tris;
        }

        private List<Vector3> GenerateVertices(int faces, int floors, float floorHeight, float radius, float reductionRate, float baseTwistiness, float nextTwistiness, Vector3 origin, out Vector3 growthEnd, out float finalRadius) {
            List<Vector3> vertices = new List<Vector3>();

            float angularStep = 360 / faces * Mathf.Deg2Rad;
            float actualRadius = 0;

            // Start pivot at the base of the trunk
            Vector3 pivot = origin;

            Vector3 growthDirection = randomGuidedVector(origin, baseTwistiness);

            for (int floor = 0; floor <= floors; floor++) {
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, growthDirection);

                for (int face = 0; face < faces; face++) {
                    float angle = face * angularStep;
                    actualRadius = radius - radius * Mathf.Pow(Mathf.Asin(floor / (float)(floors + 1) * 2 - 1) / Mathf.PI + 0.5f, 1 / reductionRate);

                    // Calculate vertex position using local floor pivot
                    Vector3 originalPosition = new Vector3(actualRadius * Mathf.Cos(angle), 0, actualRadius * Mathf.Sin(angle));

                    Vector3 actualPosition = pivot + rotation * originalPosition;
                    vertices.Add(actualPosition);
                }

                // Update the pivot for the next floor; keeping it aligned with growth direction mainly in the x-z plane
                pivot += growthDirection.normalized * floorHeight;
                
                growthDirection = randomGuidedVector(growthDirection, nextTwistiness);
            }
            growthEnd = pivot;
            finalRadius = actualRadius;
            return vertices;
        }

        private Vector3 randomGuidedVector(Vector3 vector, float change) {
            change = change/nfloors;
            return (new Vector3(Random.Range(-change, change), Random.Range(0, (100 - change)), Random.Range(-change, change)) / 100 + vector).normalized;
        }

        public static TreeBranch weeveBranches(TreeBranch trunk, TreeBranch branch1, TreeBranch branch2) {
            //Deal with possible errors
            if(trunk.branchFaces != branch1.branchFaces + branch2.branchFaces-2) {
                Debug.LogError("Child Branch Faces dont add up to exactly Trunk Faces: "+ branch1.branchFaces+" "+branch2.branchFaces);
                return null;
            }
            if(trunk.branchFaces < 3 || branch1.branchFaces < 3 || branch2.branchFaces < 3) {
                Debug.LogError("Either the trunk or one of the branches doesnt have at least 3 faces");
                return null;
            }

            //Store the topVertexIndices of the trunk
            int[] trunkTopVertexIndices = trunk.getTopVerticesIndices();

            //Offset the index of all the tris in branch1
            for(int i = 0; i < branch1.triangles.Count; i++) {
                branch1.triangles[i] += trunk.vertices.Count;
            }
            //Add the triangles and vertices to the trunk
            trunk.vertices.AddRange(branch1.vertices);
            trunk.triangles.AddRange(branch1.triangles);
            int[] branch1BottomVertexIndices = branch1.getBottomVerticesIndices();

            for (int i = 0; i < branch1.branchFaces; i++) {
                //Add bottom triangle
                trunk.triangles.Add(branch1BottomVertexIndices[i == 0 ? branch1.branchFaces - 1 : i - 1]);
                trunk.triangles.Add(branch1BottomVertexIndices[i]);
                trunk.triangles.Add(trunkTopVertexIndices[i]);
                //Add top triangle
                trunk.triangles.Add(trunkTopVertexIndices[i]);
                trunk.triangles.Add(trunkTopVertexIndices[i == 0 ? branch1.branchFaces - 1 : i - 1]);
                trunk.triangles.Add(branch1BottomVertexIndices[i == 0 ? branch1.branchFaces - 1 : i - 1]);
            }

            //Offset the index of all the tris in branch2
            for (int i = 0; i < branch2.triangles.Count; i++) {
                branch2.triangles[i] += trunk.vertices.Count;
            }
            //Add the triangles and vertices to the trunk
            trunk.vertices.AddRange(branch2.vertices);
            trunk.triangles.AddRange(branch2.triangles);
            int[] branch2bottomVertexIndices = branch2.getBottomVerticesIndices();

            int offset = branch1.branchFaces-1;

            for (int i = 0; i < branch2.branchFaces; i++) {
                //Add bottom triangle
                int j;
                if (i == 1) j = 0;
                else if (i == 0) j = trunk.branchFaces - 1;
                else j = i - 2 + offset;
                trunk.triangles.Add(trunkTopVertexIndices[j]);
                trunk.triangles.Add(branch2bottomVertexIndices[i]);
                trunk.triangles.Add(trunkTopVertexIndices[i == 0 ? i : i - 1 + offset]);

                //Add top triangle
                trunk.triangles.Add(trunkTopVertexIndices[j]);
                trunk.triangles.Add(branch2bottomVertexIndices[i == 0 ? branch2.branchFaces - 1 : i - 1]);
                trunk.triangles.Add(branch2bottomVertexIndices[i]);
            }

            return trunk;
        }
    }
}
