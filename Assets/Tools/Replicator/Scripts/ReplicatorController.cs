using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Replicator
{
    [ExecuteAlways]
    public class ReplicatorController : MonoBehaviour
    {
        public List<GameObject> replicatedObjectInstances = new List<GameObject>();
        public List<GameObject> replicatedSourceObjects = new List<GameObject>();

        public SerializableDictionary<GameObject, Mesh> cachedMeshes = new SerializableDictionary<GameObject, Mesh>();

        public SerializableDictionary<GameObject, Vector3> originalShapePositions = new SerializableDictionary<GameObject, Vector3>();
        public SerializableDictionary<GameObject, Vector3> originalShapeRotations = new SerializableDictionary<GameObject, Vector3>();
        public SerializableDictionary<GameObject, Vector3> originalShapeScales = new SerializableDictionary<GameObject, Vector3>();

        //bool listHasBeenRandomized = false;

        public Transform lookAt;

        public bool updateDeforms = false;

        public bool isInitialized = false;

        public ReplicatorShape currentReplicatorShape;

        public bool randomOrder = false;

        private int replications;

        //SHAPES OPTIONS

        //line
        public Vector3 lineStart;
        public Vector3 lineEnd = new Vector3(1, 0, 0);

        //circle
        public float circleRadius;

        //sphere
        public float sphereRadius;
        public float sphereVortex;
        public float sphereSquish;
        private float sphereTaper;

        //object
        public GameObject shapeObject;
        public float shapeScale;

        //grid
        private Vector3Int gridSize;
        public Vector3 gridSpacing;

        //modifications

        //offsets
        public Vector3 objectRotationOffset;
        private Vector3 objectPositionOffset;
        public Vector3 objectScaleOffsetXYZ;
        public float objectScaleOffset;

        //randomness 
        public Vector3 objectPositionRandom;
        public Vector3 objectRotationRandom;
        public float objectScaleRandom;
        public Vector3 objectScaleRandomXYZ;
        public int randomSeed;

        //deforms
        public bool noiseEnable;
        public float noiseAmount;
        private float noiseFrequency;
        public float twistAmount;
        public bool twistEnable;
        private Vector3 twistAxis;

        public bool DeformValuesChanged = false;

        public void Initialize(ModificationValues modificationValues, ShapeValues shapeValues, DeformValues deformValues)
        {

            //deform values
            noiseEnable = deformValues.noiseEnable;
            twistEnable = deformValues.twistEnable;
            twistAmount = deformValues.twistAmount;
            noiseFrequency = deformValues.noiseFrequency;
            twistAxis = deformValues.twistAxis;
            noiseAmount = deformValues.noiseAmount;

            //mod values
            objectPositionOffset = modificationValues.objectPositionOffset;
            objectRotationOffset = modificationValues.objectRotationOffset;
            objectPositionRandom = modificationValues.objectPositionRandom;
            objectRotationRandom = modificationValues.objectRotationRandom;
            objectScaleOffset = modificationValues.objectScaleOffset;
            objectScaleOffsetXYZ = modificationValues.objectScaleOffsetXYZ;
            objectScaleRandomXYZ = modificationValues.objectScaleRandomXYZ;
            objectScaleRandom = modificationValues.objectScaleRandom;
            randomSeed = modificationValues.randomSeed;

            //shape values
            replications = shapeValues.replications;
            currentReplicatorShape = shapeValues.replicatorShape;
            lineStart = shapeValues.lineStart;
            lineEnd = shapeValues.lineEnd;
            circleRadius = shapeValues.circleRadius;
            sphereRadius = shapeValues.sphereRadius;
            sphereSquish = shapeValues.sphereSquish;
            sphereVortex = shapeValues.sphereVortex;
            shapeObject = shapeValues.shapeObject;
            gridSize = shapeValues.gridSize;
            gridSpacing = shapeValues.gridSpacing;
            shapeScale = shapeValues.shapeScale;

            DeformObjects();

            isInitialized = true;

        }

        //private void OnAnimatorMove()
        //{
        //    if (noiseEnable == true || twistEnable == true)
        //    {
        //        DeformObjects();
        //    }
        //}

        private void Update()
        {
            ApplyValues();

            // If the animation component exists
            if (GetComponentInParent(typeof(Animator),true))
            {
                if (noiseEnable == true || twistEnable == true)
                {
                    DeformObjects();
                }
            }
        }

        void ApplyValues()
        {
            if (isInitialized == true)
            {
                if (lookAt == null)
                {
                    //apply modifications
                    //Arranger.LookAt(replicatedObjectInstances, lookAt,objectRotationOffset);
                    Arranger.OffsetScaleInspector(replicatedObjectInstances, objectScaleOffsetXYZ, originalShapeScales,objectScaleOffset);
                    Arranger.ModifyRandomScaleInspector(replicatedObjectInstances, randomSeed, objectScaleRandomXYZ, objectScaleRandom);
                    Arranger.ModifyObjectsInspector(replicatedObjectInstances, objectPositionOffset, objectRotationOffset, objectPositionRandom, objectRotationRandom, randomSeed, originalShapePositions, originalShapeRotations);
                    ArrangeObjects();
                    Arranger.ModifyRandomPosition(replicatedObjectInstances, randomSeed, objectPositionRandom);
                    if (noiseEnable == true || twistEnable == true)
                    {
                        if (DeformValuesChanged == true)
                            DeformObjects();
                        DeformValuesChanged = false;
                    }
                }

                else if (lookAt != null)
                {
                    //apply modifications
                    Arranger.ModifyRotation(replicatedObjectInstances, lookAt, objectRotationOffset, objectRotationRandom, randomSeed);
                    Arranger.OffsetScaleInspector(replicatedObjectInstances, objectScaleOffsetXYZ,originalShapeScales, objectScaleOffset);
                    Arranger.ModifyRandomScaleInspector(replicatedObjectInstances, randomSeed, objectScaleRandomXYZ, objectScaleRandom);
                    //Arranger.ModifyObjects(objectsToModify, objectPositionOffset, objectRotationOffset, objectPositionRandom, objectRotationRandom, randomSeed);
                    Arranger.OffsetPositionInspector(replicatedObjectInstances, objectPositionOffset,originalShapePositions);
                    ArrangeObjects();
                    Arranger.ModifyRandomPosition(replicatedObjectInstances, randomSeed, objectPositionRandom);
                    if (noiseEnable == true || twistEnable == true)
                    {
                        if (DeformValuesChanged == true)
                            DeformObjects();
                        DeformValuesChanged = false;
                    }
                }

            }
        }

        void ArrangeObjects()
        {
            List<GameObject> objectsToArrange = new List<GameObject>();

            objectsToArrange = replicatedObjectInstances;

            //Debug.Log("rearranging "+replicatedObjects.Count +" objects into "+currentReplicatorShape);
            switch (currentReplicatorShape)
            {
                case ReplicatorShape.Line:
                    Arranger.LineArrange(objectsToArrange, lineStart, lineEnd);
                    break;
                case ReplicatorShape.Circle:
                    Arranger.CircleArrange(objectsToArrange, circleRadius);
                    break;
                case ReplicatorShape.Sphere:
                    Arranger.SphereArrange(objectsToArrange, sphereTaper, sphereSquish, sphereVortex, sphereRadius);
                    break;
                case ReplicatorShape.Object:
                    Arranger.ObjectArrange(objectsToArrange, shapeObject, shapeScale);
                    break;
                case ReplicatorShape.Grid:
                    Arranger.GridArrange(objectsToArrange, gridSize, gridSpacing);
                    break;
            }
        }

        public void DeformObjects()
        {
            //deform the source objects
            Deformer.ApplyDeforms(replicatedObjectInstances, twistAmount, noiseAmount, noiseEnable, twistEnable, noiseFrequency, twistAxis, cachedMeshes);
        }
    }

    public struct DeformValues
    {
        public bool noiseEnable, twistEnable;
        public float noiseAmount, noiseFrequency, twistAmount;
        public Vector3 twistAxis;

        public DeformValues(bool noiseEnable, bool twistEnable, float noiseAmount, float noiseFrequency, float twistAmount, Vector3 twistAxis)
        {
            this.noiseEnable = noiseEnable;
            this.twistEnable = twistEnable;
            this.noiseAmount = noiseAmount;
            this.noiseFrequency = noiseFrequency;
            this.twistAmount = twistAmount;
            this.twistAxis = twistAxis;

        }
    }

    public struct ModificationValues
    {
        public Vector3 objectPositionOffset, objectRotationOffset, objectPositionRandom, objectRotationRandom, objectScaleOffsetXYZ, objectScaleRandomXYZ;
        public float objectScaleRandom,objectScaleOffset;
        public int randomSeed;

        public ModificationValues(Vector3 objectPositionOffset, Vector3 objectRotationOffset, Vector3 objectPositionRandom, Vector3 objectRotationRandom, int randomSeed, float objectScaleRandom, Vector3 objectScaleOffsetXYZ, Vector3 objectScaleRandomXYZ, float objectScaleOffset)
        {
            this.objectPositionOffset = objectPositionOffset;
            this.objectRotationOffset = objectRotationOffset;
            this.objectPositionRandom = objectPositionRandom;
            this.objectRotationRandom = objectRotationRandom;
            this.objectScaleRandom = objectScaleRandom;
            this.objectScaleOffsetXYZ = objectScaleOffsetXYZ;
            this.objectScaleOffset = objectScaleOffset;
            this.objectScaleRandomXYZ = objectScaleRandomXYZ;
            this.randomSeed = randomSeed;
        }

    }

    public struct ShapeValues
    {
        public int replications;
        public ReplicatorShape replicatorShape;
        public Vector3 lineStart;
        public Vector3 lineEnd;
        public float circleRadius;
        public float sphereRadius;
        public float sphereVortex;
        public float sphereSquish;
        public GameObject shapeObject;
        public Vector3Int gridSize;
        public Vector3 gridSpacing;
        public float shapeScale;

        public ShapeValues(int replications, ReplicatorShape replicatorShape, Vector3 lineStart, Vector3 lineEnd, float circleRadius, float sphereRadius, float sphereVortex, float sphereSquish, GameObject shapeObject, Vector3Int gridSize, Vector3 gridSpacing, float shapeScale)
        {
            this.replications = replications;
            this.replicatorShape = replicatorShape;
            this.lineStart = lineStart;
            this.lineEnd = lineEnd;
            this.circleRadius = circleRadius;
            this.sphereRadius = sphereRadius;
            this.sphereVortex = sphereVortex;
            this.sphereSquish = sphereSquish;
            this.shapeObject = shapeObject;
            this.gridSize = gridSize;
            this.gridSpacing = gridSpacing;
            this.shapeScale = shapeScale;

        }

    }

    //This SerializeableDictionary class is written by Rune Skovbo Johansen https://gist.github.com/runevision

    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new List<TKey>();

        [SerializeField]
        private List<TValue> values = new List<TValue>();

        // save the dictionary to lists
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        // load dictionary from lists
        public void OnAfterDeserialize()
        {
            this.Clear();

            if (keys.Count != values.Count)
                throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

            for (int i = 0; i < keys.Count; i++)
                this.Add(keys[i], values[i]);
        }
    }

    public enum ReplicatorShape
    {
        Line = 0,
        Circle = 1,
        Sphere = 2,
        Object = 3,
        Grid = 4
    }
}