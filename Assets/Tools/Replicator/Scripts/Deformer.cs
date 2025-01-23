using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Replicator
{

    public static class Deformer
    {

        //private static SerializableDictionary<GameObject, Mesh> cachedMeshes = new SerializableDictionary<GameObject, Mesh>();

        public static List<GameObject> ApplyDeforms(List<GameObject> prefabsToDeform, float twistAmount, float noiseAmount, bool enableNoise, bool enableTwist, float noiseFrequency, Vector3 twistAxis, SerializableDictionary<GameObject, Mesh> cachedMeshes)
        {
            List<GameObject> alteredPrefabs = new List<GameObject>(prefabsToDeform);
            List<Mesh> alteredMeshes = new List<Mesh>();


            //loop through list of prefabs to be replicated
            for (int i = 0; i < prefabsToDeform.Count; i++)
            {
                Mesh prefabMesh = FindMesh(prefabsToDeform[i]);

                if (FindMesh(prefabsToDeform[i]) == null)
                {
                    alteredPrefabs[i] = prefabsToDeform[i];
                    continue;
                }

                // Check if the mesh exists in the cached meshes dictionary
                if (!cachedMeshes.ContainsKey(prefabsToDeform[i]))
                {
                    // If it doesn't, add it to the dictionary
                    cachedMeshes.Add(prefabsToDeform[i], prefabMesh);
                }

                // Create a copy of the original mesh
                Mesh originalMesh = cachedMeshes[prefabsToDeform[i]];
                Mesh copyMesh = Mesh.Instantiate(originalMesh);
                copyMesh.name = originalMesh.name;

                // Apply deformations to the copy of the mesh
                if (enableTwist)
                {
                    //Debug.Log("do the twist!");
                    copyMesh = TwistDeform(copyMesh, twistAmount, Vector3.one, Vector3.zero);
                }

                if (enableNoise)
                {
                    copyMesh = NoiseDeform(copyMesh, noiseAmount, 1, Vector3.zero);
                }

                copyMesh.RecalculateNormals();

                alteredMeshes.Add(copyMesh);
            }

            for (int i = 0; i < alteredPrefabs.Count; i++)
            {
                if (FindMesh(alteredPrefabs[i]) != null)
                {
                    // Apply the altered mesh to the prefab

                    if (alteredPrefabs[i].GetComponentInChildren<MeshFilter>() == null)
                    {
                        alteredPrefabs[i].GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh = alteredMeshes[i];
                    }
                    else
                    {
                        alteredPrefabs[i].GetComponentInChildren<MeshFilter>().sharedMesh = alteredMeshes[i];
                    }
                }
            }

            return alteredPrefabs;
        }

      
        public static List<GameObject> RevertPrefabsToCachedMeshes(List<GameObject> prefabsToRevert, SerializableDictionary<GameObject, Mesh> cachedMeshes)
        {
            List<GameObject> revertedPrefabs = new List<GameObject>();
            foreach (var prefab in prefabsToRevert)
            {
                if (cachedMeshes.ContainsKey(prefab))
                {

                    prefab.GetComponent<MeshFilter>().mesh = cachedMeshes[prefab];
                    revertedPrefabs.Add(prefab);

                }
            }

            return revertedPrefabs;

        }

        public static Mesh TwistDeform(Mesh inputMesh, float twistAmount, Vector3 twistAxis, Vector3 twistCenter)
        {

            // Copy the original vertex positions
            Vector3[] originalVertices = inputMesh.vertices;

            // Create an array to store the twisted vertex positions
            Vector3[] twistedVertices = new Vector3[originalVertices.Length];

            // Calculate the rotation matrix for the twist
            Quaternion twistRotation = Quaternion.AngleAxis(twistAmount, twistAxis);

            // Apply the twist to each vertex
            for (int i = 0; i < originalVertices.Length; i++)
            {
                // Get the vector from the vertex to the twist center
                Vector3 offset = originalVertices[i] - twistCenter;

                // Create the twist transformation matrix
                Matrix4x4 twistMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(twistAmount * offset.magnitude, twistAxis), Vector3.one);

                // Apply the twist transformation to the vertex
                twistedVertices[i] = twistMatrix.MultiplyPoint3x4(originalVertices[i]);
            }

            // Update the mesh with the twisted vertices
            inputMesh.vertices = twistedVertices;

            // Recalculate the mesh normals
            inputMesh.RecalculateNormals();


            return inputMesh;
        }

        public static Mesh NoiseDeform(Mesh mesh, float amplitude, float frequency, Vector3 noiseOffset)
        {
            Vector3[] originalVertices = mesh.vertices;

            // Create an array to store the deformed vertex positions
            Vector3[] deformedVertices = new Vector3[originalVertices.Length];

            // Apply the noise deformation to each vertex
            for (int i = 0; i < originalVertices.Length; i++)
            {
                // Get the original vertex position
                Vector3 vertex = originalVertices[i];

                // Calculate the noise value using the vertex position and the noise offset
                float x = vertex.x * frequency + noiseOffset.x;
                float y = vertex.y * frequency + noiseOffset.y;
                float z = vertex.z * frequency + noiseOffset.z;

                // Change the Perlin noise function to one that returns values between -0.5 and 0.5
                float noise = (Mathf.PerlinNoise(x, y) - 0.5f) * 2f;

                // Add the noise value multiplied by the amplitude to the original vertex position
                vertex.x += noise * amplitude;
                vertex.y += noise * amplitude;
                vertex.z += noise * amplitude;

                // Store the deformed vertex position
                deformedVertices[i] = vertex;
            }

            // Update the mesh with the deformed vertices
            mesh.vertices = deformedVertices;

            // Recalculate the mesh normals
            mesh.RecalculateNormals();

            return mesh;
        }

        public static Mesh FindMesh(GameObject prefab)
        {
            //Debug.Log("Finding mesh");
            Mesh foundMesh = null;

            if (prefab.GetComponentInChildren<MeshFilter>())
            {
                foundMesh = prefab.GetComponentInChildren<MeshFilter>().sharedMesh;
            }
            else if (!prefab.GetComponentInChildren<MeshFilter>())
            {
                if (prefab.GetComponentInChildren<SkinnedMeshRenderer>())
                {
                    //Debug.Log("found skinned mesh");
                    foundMesh = prefab.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
                }
            }

            //Debug.Log(foundMesh);
            return foundMesh;

        }

    }
}