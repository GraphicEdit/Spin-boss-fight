using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting;
using UnityEngine;

namespace Replicator
{

    public static class Arranger
    {

        public static SerializableDictionary<GameObject, Vector3> currentShapePositions = new SerializableDictionary<GameObject, Vector3>();
        public static SerializableDictionary<GameObject, Vector3> currentShapeRotations = new SerializableDictionary<GameObject, Vector3>();
        public static SerializableDictionary<GameObject, Vector3> currentShapeScales = new SerializableDictionary<GameObject, Vector3>();

        public static void LineArrange(List<GameObject> objectsToModify, Vector3 lineStart, Vector3 lineEnd)
        {
            if (objectsToModify == null)
            {
                return;
            }

            Arranger.currentShapePositions.Clear();
            Arranger.currentShapeRotations.Clear();
            Arranger.currentShapeScales.Clear();



            for (int i = 0; i < objectsToModify.Count; i++)
            {
                Vector3 currentObjPosition = Vector3.Lerp(lineStart, lineEnd, ((float)i / (float)objectsToModify.Count));

                objectsToModify[i].transform.localPosition = currentObjPosition;

                Arranger.currentShapePositions.Add(objectsToModify[i], objectsToModify[i].transform.localPosition);
                Arranger.currentShapeRotations.Add(objectsToModify[i], objectsToModify[i].transform.rotation.eulerAngles);
                Arranger.currentShapeScales.Add(objectsToModify[i], objectsToModify[i].transform.localScale);

            }
        }

        public static void CircleArrange(List<GameObject> objectsToModify, float circleRadius)
        {
            if (objectsToModify == null)
            {
                return;
            }

            Arranger.currentShapePositions.Clear();
            Arranger.currentShapeRotations.Clear();
            Arranger.currentShapeScales.Clear();

            for (int i = 0; i < objectsToModify.Count; i++)
            {

                var angle = i * Mathf.PI * 2 / objectsToModify.Count;
                var pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * circleRadius;

                Vector3 currentObjPosition = pos;

                objectsToModify[i].transform.localPosition = currentObjPosition;

                Arranger.currentShapePositions.Add(objectsToModify[i], objectsToModify[i].transform.localPosition);
                Arranger.currentShapeRotations.Add(objectsToModify[i], objectsToModify[i].transform.rotation.eulerAngles);
                Arranger.currentShapeScales.Add(objectsToModify[i], objectsToModify[i].transform.localScale);
            }
        }

        public static void SphereArrange(List<GameObject> objectsToModify, float sphereTaper, float sphereSquish, float sphereVortex, float sphereRadius)
        {

            if (objectsToModify == null)
            {
                return;
            }

            Arranger.currentShapePositions.Clear();
            Arranger.currentShapeRotations.Clear();
            Arranger.currentShapeScales.Clear();



            var nPoints = objectsToModify.Count;
            float fPoints = (float)nPoints;

            //Vector3[] points = new Vector3[nPoints];

            float inc = Mathf.PI * (3 - Mathf.Sqrt(5));
            float off = 2 / fPoints;

            for (int i = 0; i < nPoints; i++)
            {
                float y = i * off - 1 + (off / 2);
                float r = Mathf.Sqrt(1 - y * y) * sphereSquish;
                float phi = i * inc * sphereVortex;

                Vector3 currentObjPosition = new Vector3(Mathf.Cos(phi) * r, y, Mathf.Sin(phi) * r) * sphereRadius;

                objectsToModify[i].transform.localPosition = currentObjPosition;

                Arranger.currentShapePositions.Add(objectsToModify[i], objectsToModify[i].transform.localPosition);
                Arranger.currentShapeRotations.Add(objectsToModify[i], objectsToModify[i].transform.rotation.eulerAngles);
                Arranger.currentShapeScales.Add(objectsToModify[i], objectsToModify[i].transform.localScale);

            }

        }

        public static void ObjectArrange(List<GameObject> objectsToModify, GameObject shapeObject, float shapeScale)
        {
            if (shapeObject == null)
            {
                Debug.LogError("You must select a Shape Object");
                return;
            }


            if (objectsToModify == null)
            {
                return;
            }



            Arranger.currentShapePositions.Clear();
            Arranger.currentShapeRotations.Clear();
            Arranger.currentShapeScales.Clear();


            Mesh sourceMesh = Deformer.FindMesh(shapeObject);
            Mesh objectMesh = new Mesh();
            objectMesh.vertices = sourceMesh.vertices;
            objectMesh.triangles = sourceMesh.triangles;


            if (objectMesh == null)
            {
                Debug.LogError("Your Shape Object must be a GameObject with an associated mesh.");
            }

            Vector3[] meshVertices = objectMesh.vertices;

            for (int i = 0; i < objectsToModify.Count; i++)
            {
                if (i > meshVertices.Length - 1)
                {
                    break;
                }
                Vector3 world_v = shapeObject.transform.TransformPoint(meshVertices[i] * shapeScale);

                Vector3 currentObjPosition = world_v;

                objectsToModify[i].transform.localPosition = currentObjPosition;
                Arranger.currentShapePositions.Add(objectsToModify[i], objectsToModify[i].transform.localPosition);
                Arranger.currentShapeRotations.Add(objectsToModify[i], objectsToModify[i].transform.rotation.eulerAngles);
                Arranger.currentShapeScales.Add(objectsToModify[i], objectsToModify[i].transform.localScale);
            }
        }



        public static void GridArrange(List<GameObject> objectsToModify, Vector3Int gridSize, Vector3 gridSpacing)
        {
            if (objectsToModify == null || objectsToModify.Count < gridSize.x * gridSize.y * gridSize.z)
            {
                return;
            }

            Arranger.currentShapePositions.Clear();
            Arranger.currentShapeRotations.Clear();
            Arranger.currentShapeScales.Clear();

            int index = 0;
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    for (int z = 0; z < gridSize.z; z++)
                    {
                        Vector3 position = new Vector3(x * gridSpacing.x, y * gridSpacing.y, z * gridSpacing.z);
                        GameObject currentObject = objectsToModify[index % objectsToModify.Count];
                        currentObject.transform.localPosition = position;

                        Arranger.currentShapePositions.Add(currentObject, currentObject.transform.localPosition);
                        Arranger.currentShapeRotations.Add(currentObject, currentObject.transform.rotation.eulerAngles);
                        Arranger.currentShapeScales.Add(currentObject, currentObject.transform.localScale);
                        index++;
                    }
                }
            }
        }

        public static void LookAt(List<GameObject> objectsToModify, Transform lookAt, Vector3 objectRotationOffset)
        {
            if (lookAt != null)
            {
                for (int i = 0; i < objectsToModify.Count; i++)
                {
                    Quaternion rotation = Quaternion.LookRotation(lookAt.position - objectsToModify[i].transform.position);
                    Vector3 eulerAngles = rotation.eulerAngles;
                    eulerAngles += objectRotationOffset;
                    objectsToModify[i].transform.rotation = Quaternion.Euler(eulerAngles);
                }
            }
        }

        public static void OffsetRotation(List<GameObject> objectsToModify, Vector3 objectRotationOffset)
        {
            foreach (var obj in objectsToModify)
            {
                //save original rotation
                Vector3 originalRotation = obj.transform.rotation.eulerAngles;
                //apply rotation offset to original rotation
                Vector3 newRotation = originalRotation + objectRotationOffset;
                obj.transform.rotation = Quaternion.Euler(newRotation);
            }
        }

        public static void ModifyObjects(List<GameObject> objectsToModify, Vector3 objectPositionOffset, Vector3 objectRotationOffset, Vector3 objectPositionRandom, Vector3 objectRotationRandom, int randomSeed)
        {
            //position offset
            OffsetPosition(objectsToModify, objectPositionOffset);

            //rotation offset
            OffsetRotationInspector(objectsToModify, objectRotationOffset, currentShapeRotations);

            //random position
            ModifyRandomPosition(objectsToModify, randomSeed, objectPositionRandom);

            //random rotation
            ModifyRandomRotation(objectsToModify, randomSeed, objectRotationRandom);

        }

        public static void ModifyObjectsInspector(List<GameObject> objectsToModify, Vector3 objectPositionOffset, Vector3 objectRotationOffset, Vector3 objectPositionRandom, Vector3 objectRotationRandom, int randomSeed, SerializableDictionary<GameObject, Vector3> originalPositions, SerializableDictionary<GameObject, Vector3> originalRotations)
        {
            //position offset
            OffsetPositionInspector(objectsToModify, objectPositionOffset, originalPositions);

            //rotation offset
            OffsetRotationInspector(objectsToModify, objectRotationOffset, originalRotations);

            //random position
            //ModifyRandomPositionInspector(objectsToModify,objectPositionRandom,originalPositions,randomSeed);

            //random rotation
            ModifyRandomRotation(objectsToModify, randomSeed, objectRotationRandom);
        }

        public static void ModifyRotation(List<GameObject> objectsToModify, Transform lookAt, Vector3 objectRotationOffset, Vector3 objectRotationRandom, int randomSeed)
        {
            for (int i = 0; i < objectsToModify.Count; i++)
            {
                UnityEngine.Random.InitState(randomSeed + i);
                Vector3 randomVector = new Vector3(UnityEngine.Random.Range(-1f, 1f) * objectRotationRandom.x, UnityEngine.Random.Range(-1f, 1f) * objectRotationRandom.y, UnityEngine.Random.Range(-1f, 1f) * objectRotationRandom.z);
                Vector3 currentRotation = objectsToModify[i].transform.rotation.eulerAngles;

                if (lookAt != null)
                {
                    objectsToModify[i].transform.LookAt(lookAt);
                    currentRotation = objectsToModify[i].transform.rotation.eulerAngles;
                }

                Vector3 newRotation = currentRotation + objectRotationOffset + randomVector;
                objectsToModify[i].transform.rotation = Quaternion.Euler(newRotation);
            }
        }

        public static void OffsetPosition(List<GameObject> objectsToModify, Vector3 objectPositionOffset)
        {
            if (objectsToModify == null)
            {
                return;
            }

            foreach (var obj in objectsToModify)
            {
                obj.transform.Translate(objectPositionOffset, Space.World);
            }
        }



        public static void OffsetPositionInspector(List<GameObject> objectsToModify, Vector3 objectPositionOffset, SerializableDictionary<GameObject, Vector3> originalPositions)
        {
            foreach (var obj in objectsToModify)
            {
                obj.transform.localPosition = originalPositions[obj] + objectPositionOffset;

            }
        }

        public static void OffsetRotationInspector(List<GameObject> objectsToModify, Vector3 objectRotationOffset, SerializableDictionary<GameObject, Vector3> originalRotations)
        {
            foreach (var obj in objectsToModify)
            {
                Vector3 rotation = originalRotations[obj];
                Vector3 newRotation = rotation + objectRotationOffset;
                obj.transform.rotation = Quaternion.Euler(objectRotationOffset);
            }
        }

        public static void ModifyRandomPosition(List<GameObject> objectsToModify, int randomSeed, Vector3 objectPositionRandom)
        {
            for (int i = 0; i < objectsToModify.Count; i++)
            {
                UnityEngine.Random.InitState(randomSeed + i);
                Vector3 randomVector = new Vector3(UnityEngine.Random.Range(-1f, 1f) * objectPositionRandom.x, UnityEngine.Random.Range(-1f, 1f) * objectPositionRandom.y, UnityEngine.Random.Range(-1f, 1f) * objectPositionRandom.z);

                objectsToModify[i].transform.Translate(randomVector, Space.World);
            }
        }

        public static void ModifyRandomPositionInspector(List<GameObject> objectsToModify, Vector3 objectPositionRandom, SerializableDictionary<GameObject, Vector3> originalPositions, int randomSeed)
        {
            for (int i = 0; i < objectsToModify.Count; i++)
            {
                UnityEngine.Random.InitState(randomSeed + i);
                Vector3 randomVector = new Vector3(UnityEngine.Random.Range(-1f, 1f) * objectPositionRandom.x, UnityEngine.Random.Range(-1f, 1f) * objectPositionRandom.y, UnityEngine.Random.Range(-1f, 1f) * objectPositionRandom.z);

                //objectsToModify[i].transform.Translate(randomVector, Space.World);
                objectsToModify[i].transform.localPosition = originalPositions[objectsToModify[i]] + randomVector;
            }
        }

        public static void ModifyRandomRotation(List<GameObject> objectsToModify, int randomSeed, Vector3 objectRotationRandom)
        {
            for (int i = 0; i < objectsToModify.Count; i++)
            {
                UnityEngine.Random.InitState(randomSeed + i);
                Vector3 randomVector = new Vector3(UnityEngine.Random.Range(-1f, 1f) * objectRotationRandom.x, UnityEngine.Random.Range(-1f, 1f) * objectRotationRandom.y, UnityEngine.Random.Range(-1f, 1f) * objectRotationRandom.z);
                Vector3 currentRotation = objectsToModify[i].transform.rotation.eulerAngles;
                //Vector3 randomRotation = new Vector3(currentRotation.x * randomVector.x, currentRotation.y * randomVector.y, currentRotation.z * randomVector.z);
                Vector3 newRotation = currentRotation + randomVector;
                objectsToModify[i].transform.rotation = Quaternion.Euler(newRotation);
            }
        }

        public static void OffsetScale(List<GameObject> objectsToModify, Vector3 objectScaleOffsetXYZ, float objectScaleOffset)
        {
            foreach (var obj in objectsToModify)
            {
                Vector3 newScale = Vector3.one + objectScaleOffsetXYZ;
                newScale = new Vector3(newScale.x + objectScaleOffset, newScale.y + objectScaleOffset, newScale.z + objectScaleOffset);
                obj.transform.localScale = newScale;
            }
        }

        public static void ModifyRandomScale(List<GameObject> objectsToModify, int randomSeed, Vector3 objectScaleRandomXYZ, float objectScaleRandom)
        {
            for (int i = 0; i < objectsToModify.Count; i++)
            {
                UnityEngine.Random.InitState(randomSeed + i);
                Vector3 currentScale = objectsToModify[i].transform.localScale;
                currentScale.x *= 1 + UnityEngine.Random.Range(-objectScaleRandomXYZ.x, objectScaleRandomXYZ.x);
                currentScale.y *= 1 + UnityEngine.Random.Range(-objectScaleRandomXYZ.y, objectScaleRandomXYZ.y);
                currentScale.z *= 1 + UnityEngine.Random.Range(-objectScaleRandomXYZ.z, objectScaleRandomXYZ.z);
                float randomValue = UnityEngine.Random.Range(-1f, 1f) * objectScaleRandom;
                currentScale += new Vector3(randomValue, randomValue, randomValue);
                objectsToModify[i].transform.localScale = currentScale;
            }
        }
        public static void OffsetScaleInspector(List<GameObject> objectsToModify, Vector3 objectScaleOffsetXYZ, SerializableDictionary<GameObject, Vector3> originalScales, float objectScaleOffset)
        {
            foreach (var obj in objectsToModify)
            {
                //Vector3 scale = originalScales[obj];
                //Vector3 newScale = scale + objectScaleOffset;
                //obj.transform.localScale = newScale;

                Vector3 newScale = Vector3.one + objectScaleOffsetXYZ;
                newScale = new Vector3(newScale.x + objectScaleOffset, newScale.y + objectScaleOffset, newScale.z + objectScaleOffset);
                obj.transform.localScale = newScale;

            }
        }

        public static void ModifyRandomScaleInspector(List<GameObject> objectsToModify, int randomSeed, Vector3 objectScaleRandomXYZ, float objectScaleRandom)
        {
            for (int i = 0; i < objectsToModify.Count; i++)
            {
                UnityEngine.Random.InitState(randomSeed + i);
                Vector3 currentScale = objectsToModify[i].transform.localScale;
                currentScale.x *= 1 + UnityEngine.Random.Range(-objectScaleRandomXYZ.x, objectScaleRandomXYZ.x);
                currentScale.y *= 1 + UnityEngine.Random.Range(-objectScaleRandomXYZ.y, objectScaleRandomXYZ.y);
                currentScale.z *= 1 + UnityEngine.Random.Range(-objectScaleRandomXYZ.z, objectScaleRandomXYZ.z);
                float randomValue = UnityEngine.Random.Range(-1f, 1f) * objectScaleRandom;
                currentScale += new Vector3(randomValue, randomValue, randomValue);
                objectsToModify[i].transform.localScale = currentScale;
            }
        }
    }
}