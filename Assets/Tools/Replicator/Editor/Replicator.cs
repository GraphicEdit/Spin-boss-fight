
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.AnimatedValues;
    using System;
    using System.Collections;

namespace Replicator
{

    public class Replicator : EditorWindow
    {
        public List<GameObject> objectsToReplicate = new List<GameObject>();
        public List<GameObject> replicatedObjects = new List<GameObject>();

        public List<GameObject> randomObjectOrderCache = new List<GameObject>();

        public bool listHasBeenRandomized = false;

        public Transform lookAt;

        public SerializableDictionary<GameObject, Vector3> originalPositions = new SerializableDictionary<GameObject, Vector3>();
        public SerializableDictionary<GameObject, Vector3> originalRotations = new SerializableDictionary<GameObject, Vector3>();
        public SerializableDictionary<GameObject, Vector3> originalScales = new SerializableDictionary<GameObject, Vector3>();

        public SerializableDictionary<GameObject, Mesh> cachedMeshes = new SerializableDictionary<GameObject, Mesh>();

        public ReplicatorShape currentReplicatorShape;

        Vector3 lastLookAtPosition = new Vector3(0, 0, 0);

        public bool randomOrder = false;

        private bool objectsPopulated = false;

        GameObject currentReplicatorParent;
        GameObject replicatorParent;
        GameObject currentReplicatorPreview;

        bool showObjectModifications = true;
        bool showDeformerModifications = false;
        bool showOffestObjectModifications = false;
        bool showRandomObjectModifications = false;

        public bool isActivePreview = false;
        public bool showGizmos = false;

        Vector3 storedParentPosition;
        Quaternion storedParentRotation;
        Vector3 storedParentScale = Vector3.one;

        SerializedObject so;
        SerializedProperty propObjectsToReplicate;
        SerializedProperty propReplications;
        SerializedProperty propReplicatedObjects;
        SerializedProperty propLookAt;
        SerializedProperty propReplicatorShape;
        SerializedProperty propLineStart;
        SerializedProperty propLineEnd;
        SerializedProperty propAutoArrange;
        SerializedProperty propRandomOrder;
        SerializedProperty propShapeObject;
        SerializedProperty propShapeScale;

        SerializedProperty propCircleRadius;
        SerializedProperty propSphereRadius;
        SerializedProperty propSphereSquish;
        SerializedProperty propSphereVortex;
        SerializedProperty propSphereCircle;
        SerializedProperty propSphereTaper;

        SerializedProperty propGridSize;
        SerializedProperty propGridSpacing;

        //object modifications props
        SerializedProperty propPositionRandom;
        SerializedProperty propRotationRandom;
        SerializedProperty propRotationOffset;
        SerializedProperty propPositionOffset;
        SerializedProperty propScaleOffsetXYZ;
        SerializedProperty propScaleOffset;
        SerializedProperty propScaleRandom;
        SerializedProperty propScaleRandomXYZ;
        SerializedProperty propRandomSeed;

        //deform props
        SerializedProperty propTwistEnable;
        SerializedProperty propTwistAmount;
        SerializedProperty propNoiseEnable;
        SerializedProperty propNoiseAmount;
        SerializedProperty propNoiseFrequency;
        SerializedProperty propTwistAxis;

        SerializedProperty propIsActivePreview;
        SerializedProperty propShowGizmos;

        public Vector3 objectPositionRandom;
        public Vector3 objectRotationRandom;
        public Vector3 objectRotationOffset;
        public Vector3 objectPositionOffset;
        public Vector3 objectScaleOffsetXYZ = new Vector3(0, 0, 0);
        public float objectScaleOffset = 0f;
        public float objectScaleRandom = 0f;
        public Vector3 objectScaleRandomXYZ = new Vector3(0, 0, 0);
        public int randomSeed;

        //deform fields
        public bool twistEnable;
        public float twistAmount;
        public bool noiseEnable;
        public float noiseAmount;
        public float noiseFrequency;
        public Vector3 twistAxis;

        AnimBool showLineFields;
        AnimBool showObjectFields;
        AnimBool showSphereFields;
        AnimBool showCircleFields;
        AnimBool showGridFields;

        public int replications = 1;

        public bool autoArrange = false;
        

        //SHAPES FIELDS

        //line
        public Vector3 lineStart;
        public Vector3 lineEnd = new Vector3(1, 0, 0);

        //circle
        public float circleRadius;

        //sphere
        public float sphereRadius;
        public float sphereVortex;
        public float sphereSquish;
        public float sphereTaper;

        //object
        public GameObject shapeObject;
        public float shapeScale = 1f;

        //grid
        public Vector3Int gridSize;
        public Vector3 gridSpacing;

        private Texture2D lightLogo = null;
        private Texture2D darkLogo = null;

        [MenuItem("Tools/Replicator/Replicator Window",false,0)]
        public static void OpenWindow()
        {
            EditorWindow editorWindow = GetWindow<Replicator>();
            editorWindow.autoRepaintOnSceneChange = true;
            editorWindow.Show();
            editorWindow.minSize = new Vector2(300, 400);
        }

        [MenuItem("Tools/Replicator/Actions/Destroy All Instances")]
        public static void DestroyAllInstances()
        {
            ReplicatorController[] instances;
            instances = GameObject.FindObjectsOfType<ReplicatorController>();

            Undo.RecordObjects(instances, "Destroy All Instances");
            
            foreach (var controller in instances)
            {
                Undo.DestroyObjectImmediate(controller.gameObject);
            }
        }

        [MenuItem("Tools/Replicator/Actions/Hide All Instances")]
        public static void HideAllInstances()
        {
            ReplicatorController[] instances;
            instances = GameObject.FindObjectsOfType<ReplicatorController>();

            foreach (var controller in instances)
            {
                controller.gameObject.SetActive(false);
            }
        }

        void OnInspectorUpdate()
        {
            // Call Repaint on OnInspectorUpdate as it repaints the windows
            // less times as if it was OnGUI/Update
            Repaint();

        }

        private void OnEnable()
        {
            listHasBeenRandomized = false;

            //logo setup
            lightLogo = (Texture2D)Resources.Load("Replicator/ReplicatorLogo_Light", typeof(Texture2D));
            darkLogo = (Texture2D)Resources.Load("Replicator/ReplicatorLogo_Dark", typeof(Texture2D));

            //fade stuff setup
            showLineFields = new AnimBool(true);
            showObjectFields = new AnimBool(false);
            showSphereFields = new AnimBool(false);
            showCircleFields = new AnimBool(false);
            showGridFields = new AnimBool(false);

            //Load save data if it exists
            var data = EditorPrefs.GetString("ReplicatorSave", JsonUtility.ToJson(this, false));
            JsonUtility.FromJsonOverwrite(data, this);

            ScriptableObject target = this;
            so = new SerializedObject(target);

            //setting up props
            propReplications = so.FindProperty("replications");
            propReplicatorShape = so.FindProperty("currentReplicatorShape");
            propObjectsToReplicate = so.FindProperty("objectsToReplicate");
            propReplicatedObjects = so.FindProperty("replicatedObjects");
            propAutoArrange = so.FindProperty("autoArrange");
            propRandomOrder = so.FindProperty("randomOrder");
            propShapeObject = so.FindProperty("shapeObject");
            propShapeScale = so.FindProperty("shapeScale");
            propCircleRadius = so.FindProperty("circleRadius");
            propSphereRadius = so.FindProperty("sphereRadius");
            propSphereVortex = so.FindProperty("sphereVortex");
            propSphereSquish = so.FindProperty("sphereSquish");
            propSphereTaper = so.FindProperty("sphereTaper");
            propScaleOffset = so.FindProperty("objectScaleOffset");
            propScaleOffsetXYZ = so.FindProperty("objectScaleOffsetXYZ");
            propScaleRandom = so.FindProperty("objectScaleRandom");
            propScaleRandomXYZ = so.FindProperty("objectScaleRandomXYZ");
            propLookAt = so.FindProperty("lookAt");

            //setting up object mod props
            propPositionRandom = so.FindProperty("objectPositionRandom");
            propRotationRandom = so.FindProperty("objectRotationRandom");
            propPositionOffset = so.FindProperty("objectPositionOffset");
            propRotationOffset = so.FindProperty("objectRotationOffset");
            
            propRandomSeed = so.FindProperty("randomSeed");
            

            //deforms
            propNoiseEnable = so.FindProperty("noiseEnable");
            propNoiseAmount = so.FindProperty("noiseAmount");
            propTwistEnable = so.FindProperty("twistEnable");
            propTwistAmount = so.FindProperty("twistAmount");
            propNoiseFrequency = so.FindProperty("noiseFrequency");
            propTwistAxis = so.FindProperty("twistAxis");

            //shapes

            //line
            propLineStart = so.FindProperty("lineStart");
            propLineEnd = so.FindProperty("lineEnd");

            //grid
            propGridSize = so.FindProperty("gridSize");
            propGridSpacing = so.FindProperty("gridSpacing");

            propIsActivePreview = so.FindProperty("isActivePreview");
            propShowGizmos = so.FindProperty("showGizmos");

            if (isActivePreview == true)
            {
                //Debug.Log("active preview!");
                
                //Debug.Log("window open active preview true");
                if (autoArrange && replicatedObjects != null && replicatedObjects.Count > 0)
                {
                    
                    UpdatePreview();

                    
                    isActivePreview = true;

                    //Debug.Log("window open preview");
                }
            }

            

            if (randomOrder == true)
            {
                ClearAndUpdatePreview();
                listHasBeenRandomized = false;            
            }

        }

        private void OnDisable()
        {
            //Save the window
            // We get the Json data
            var data = JsonUtility.ToJson(this, false);
            // And we save it
            EditorPrefs.SetString("ReplicatorSave", data);

        }

        private void OnValidate()
        {
            if (autoArrange && replicatedObjects != null && replicatedObjects.Count > 0)
            {
                //objectsToReplicate = Deformer.ApplyDeforms(objectsToReplicate, twistAmount, noiseAmount, noiseEnable, twistEnable,noiseFrequency,twistAxis);

                ArrangeObjects(replicatedObjects);
            }

        }

        private void Update()
        {
            if (lookAt != null)
            {
                if (lookAt.position != lastLookAtPosition)
                {

                    lastLookAtPosition = lookAt.position;

                }
            }
            ArrangeObjects(replicatedObjects);

        }


        private void OnGUI()
        {
            so.Update();

            if (EditorGUIUtility.isProSkin)
            {
                GUILayout.Label(lightLogo, GUILayout.Height(60));
            }
            else if (!EditorGUIUtility.isProSkin)
            {
                GUILayout.Label(darkLogo, GUILayout.Height(60));
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(propObjectsToReplicate, true);
            if (EditorGUI.EndChangeCheck())
            {
                cachedMeshes.Clear();
            }

            //EditorGUILayout.PropertyField(propReplicatedObjects, true);

            //fade stuff

            showLineFields = new AnimBool(false);
            showObjectFields = new AnimBool(false);
            showCircleFields = new AnimBool(false);
            showSphereFields = new AnimBool(false);
            showGridFields = new AnimBool(false);

            switch (currentReplicatorShape)
            {
                case ReplicatorShape.Line:
                    {
                        showLineFields = new AnimBool(true);
                        break;
                    }

                case ReplicatorShape.Object:
                    {
                        showObjectFields = new AnimBool(true);
                        break;
                    }

                case ReplicatorShape.Sphere:
                    {
                        showSphereFields = new AnimBool(true);
                        break;
                    }

                case ReplicatorShape.Circle:
                    {
                        showCircleFields = new AnimBool(true);
                        break;
                    }
                case ReplicatorShape.Grid:
                    {
                        showGridFields = new AnimBool(true);
                        break;
                    }

            }

            //SHAPE FIELD GUI

            //Int field
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(propReplications, new GUIContent("Amount of Replications: "));
            if (EditorGUI.EndChangeCheck())
            {
                //UpdatePreview();
            }

            //Replicator shape field
            ReplicatorShape previousValue = (ReplicatorShape)propReplicatorShape.enumValueIndex;
            
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(propRandomOrder, new GUIContent("Randomize Order: "));
            if (EditorGUI.EndChangeCheck())
            {
                so.ApplyModifiedProperties();
                
                listHasBeenRandomized = false;
                
                //replicatedObjects = RandomizeReplicationOrder(replicatedObjects);
                ClearAndUpdatePreview();
            }
            
            EditorGUILayout.PropertyField(propReplicatorShape, new GUIContent("Replicator Shape: "));
            if (propReplicatorShape.enumValueIndex == (int)ReplicatorShape.Grid && previousValue != ReplicatorShape.Grid)
            {
                //if we're switching to grid, destroy the preview and rebuild it
               //Debug.Log("changed to grid");
                if (replicatedObjects != null && replicatedObjects.Count > 0)
                {
                    ClearAndUpdatePreview();
                }

            }
            else if ((propReplicatorShape.enumValueIndex != (int)ReplicatorShape.Grid) && (previousValue == ReplicatorShape.Grid))
            {
                //Debug.Log("changed to not grid");
            }


            
            EditorGUILayout.LabelField("Shape Options:", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(propLookAt, true);
            if (EditorGUI.EndChangeCheck())
            {
                so.ApplyModifiedProperties();
                //ArrangeObjects(replicatedObjects);
            }

            if (EditorGUILayout.BeginFadeGroup(showLineFields.faded))
            {
                //Line shape fields
                EditorGUILayout.PropertyField(propLineStart, new GUIContent("Line Start: "), false);
                EditorGUILayout.PropertyField(propLineEnd, new GUIContent("Line End: "), false);
            }
            EditorGUILayout.EndFadeGroup();


            if (EditorGUILayout.BeginFadeGroup(showObjectFields.faded))
            {
                //Object shape fields
                EditorGUILayout.PropertyField(propShapeObject, new GUIContent("Shape Object: "), false);
                EditorGUILayout.PropertyField(propShapeScale, new GUIContent("Shape Scale: "), false);
            }
            EditorGUILayout.EndFadeGroup();

            if (EditorGUILayout.BeginFadeGroup(showSphereFields.faded))
            {
                //Sphere shape fields
                EditorGUILayout.PropertyField(propSphereRadius, false);
                EditorGUILayout.PropertyField(propSphereVortex, false);
                EditorGUILayout.PropertyField(propSphereSquish, false);
                //EditorGUILayout.PropertyField(propSphereTaper, false);

            }
            EditorGUILayout.EndFadeGroup();

            if (EditorGUILayout.BeginFadeGroup(showCircleFields.faded))
            {
                //Circle shape fields
                EditorGUILayout.PropertyField(propCircleRadius, false);

            }
            EditorGUILayout.EndFadeGroup();

            gridSize = new Vector3Int(Math.Max(1, gridSize.x), Math.Max(1, gridSize.y), Math.Max(1, gridSize.z));

            if (EditorGUILayout.BeginFadeGroup(showGridFields.faded))
            {
                //Grid shape fields

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(propGridSize, false);
                if (EditorGUI.EndChangeCheck())
                {
                    //Debug.Log("grid size changed");
                    
                        ClearAndUpdatePreview();
                    

                }

                EditorGUILayout.PropertyField(propGridSpacing, false);

            }
            EditorGUILayout.EndFadeGroup();

            //object modifications
            GUIStyle style = EditorStyles.foldout;
            FontStyle previousStyle = style.fontStyle;
            style.fontStyle = FontStyle.Bold;
            showObjectModifications = EditorGUILayout.Foldout(showObjectModifications, "Object Modifications");
            style.fontStyle = previousStyle;

            if (showObjectModifications)
            {
                EditorGUILayout.BeginHorizontal();
                // Add some indentation before the child foldouts
                GUILayout.Space(20);
                showOffestObjectModifications = EditorGUILayout.Foldout(showOffestObjectModifications, "Offset");
                EditorGUILayout.EndHorizontal();
                if (showOffestObjectModifications)
                {
                    EditorGUILayout.BeginHorizontal();
                    // Add some indentation before the child foldouts
                    GUILayout.Space(20);
                    EditorGUILayout.BeginVertical();
                    //EditorGUILayout.PropertyField(propPositionOffset);
                    EditorGUILayout.PropertyField(propRotationOffset);
                    EditorGUILayout.PropertyField(propScaleOffset);
                    EditorGUILayout.PropertyField(propScaleOffsetXYZ);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.BeginHorizontal();
                // Add some indentation before the child foldouts
                GUILayout.Space(20);
                showRandomObjectModifications = EditorGUILayout.Foldout(showRandomObjectModifications, "Random");
                EditorGUILayout.EndHorizontal();
                if (showRandomObjectModifications)
                {
                    EditorGUILayout.BeginHorizontal();
                    // Add some indentation before the child foldouts
                    GUILayout.Space(20);
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.PropertyField(propPositionRandom);
                    EditorGUILayout.PropertyField(propRotationRandom);
                    EditorGUILayout.PropertyField(propScaleRandom);
                    EditorGUILayout.PropertyField(propScaleRandomXYZ);
                   
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                

                EditorGUILayout.BeginHorizontal();
                // Add some indentation before the child foldouts
                GUILayout.Space(20);
                showDeformerModifications = EditorGUILayout.Foldout(showDeformerModifications, "Deform");
                EditorGUILayout.EndHorizontal();
                if (showDeformerModifications)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.BeginHorizontal();
                    // Add some indentation before the child foldouts
                    GUILayout.Space(20);
                    EditorGUILayout.BeginVertical();
                    //content go here
                    EditorGUILayout.LabelField("Twist", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(propTwistEnable, new GUIContent("Enable"));
                    EditorGUILayout.PropertyField(propTwistAmount, new GUIContent("Amount"));
                    //EditorGUILayout.PropertyField(propTwistAxis, new GUIContent("Amount"));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.LabelField("Noise", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(propNoiseEnable, new GUIContent("Enable"));
                    //EditorGUILayout.PropertyField(propNoiseFrequency, new GUIContent("Frequency"));
                    EditorGUILayout.PropertyField(propNoiseAmount, new GUIContent("Amount"));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (autoArrange == true)
                        {
                            if (objectsPopulated == true)
                            {
                                
                                ClearAndUpdatePreview();
                            }
                        }
                    }

                }
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(propRandomSeed);
            if (EditorGUI.EndChangeCheck())
            {
                listHasBeenRandomized = false;
                ClearAndUpdatePreview();
            }

            //Buttons   
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Preview"))
            {
                UpdatePreview();
                isActivePreview = true;

            }
            if (GUILayout.Button("Clear Preview"))
            {
                cachedMeshes.Clear();
                ClearObjects();
                isActivePreview = false;
            }
            if (GUILayout.Button("Reset Options"))
            {
                cachedMeshes.Clear();
                ClearObjects();
                isActivePreview = false;
                ResetAllValues(); 

            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.PropertyField(propAutoArrange, new GUIContent("Auto Preview: "), false);
            EditorGUILayout.PropertyField(propShowGizmos, new GUIContent("Show Gizmos: "), false);
            EditorGUILayout.EndHorizontal();
            //EditorGUILayout.PropertyField(propIsActivePreview, new GUIContent("Is ActivePreview: "), false);

            if (GUILayout.Button("Replicate!", GUILayout.Height(40)))
            {
                if (objectsToReplicate.Count > 0)
                {

                    UpdatePreview();
                    isActivePreview = true;

                    Build();
                    RecallStoredParentPosition();

                    if (autoArrange == true)
                    {
                        UpdatePreview();
                    }
                }
                else
                {
                    Debug.LogError("Please assign objects to replicate.");
                }
            }

            so.ApplyModifiedProperties();

        }
        void UpdatePreview()
        {
            
            StoreCurrentParentPosition();
            MakePreviewObjects();
            listHasBeenRandomized = false;
            ArrangeObjects(replicatedObjects);

            RecallStoredParentPosition();

            isActivePreview = true;

            so.ApplyModifiedProperties();
        }

        void ClearAndUpdatePreview()
        {
            StoreCurrentParentPosition();
            ClearObjects();

            isActivePreview = false;

            UpdatePreview();
            RecallStoredParentPosition();
        }

        void UpdateDeforms()
        {
            objectsToReplicate = Deformer.ApplyDeforms(objectsToReplicate, twistAmount, noiseAmount, noiseEnable, twistEnable, noiseFrequency, twistAxis, cachedMeshes);
        }

        void MakePreviewObjects()
        {
            //Debug.Log("updating preview: " + replicatedObjects.Count);
            //Debug.Log("replicated objects before clear: " + replicatedObjects.Count);
            //clear the current objects

            if (replicatedObjects == null)
            {
                return;
            }

            if (replicatedObjects.Count > 0)
            {
                ClearObjects();
            }

            if (objectsToReplicate.Count < 1)
            {
                Debug.LogError("Assign at least one GameObject to replicate first.");
                return;
            }

            //if there isn't one, at this point make a new replicator parent
            if (currentReplicatorParent == null)
            {
                //Debug.Log("making default parent");
                currentReplicatorParent = new GameObject();

                currentReplicatorParent.name = "Replicator Parent [PREVIEW]";

                currentReplicatorPreview = currentReplicatorParent;

                currentReplicatorParent.transform.position = Vector3.zero;
                currentReplicatorParent.transform.rotation = Quaternion.identity;
                currentReplicatorParent.transform.localScale = Vector3.one;
            }

            //apply deforms before replication
            //Debug.Log("applying deforms");
            UpdateDeforms();

            CreateObjects();
            
            //Debug.Log("replicated objects afrer instantiate: " + replicatedObjects.Count);
        }

        void CreateObjects()
        {
            //make the new objects
            //Debug.Log("we're making " + replications + " instances per object.");
            if (currentReplicatorShape == ReplicatorShape.Line || currentReplicatorShape == ReplicatorShape.Circle || currentReplicatorShape == ReplicatorShape.Sphere)
            {
                for (int i = 0; i < (replications / objectsToReplicate.Count); i++)
                {
                    //Debug.Log("there are " + objectsToReplicate.Count + " objects in the list.");
                    foreach (GameObject obj in objectsToReplicate)
                    {
                        GameObject instantiatedObj = Instantiate(obj, currentReplicatorParent.transform);
                        instantiatedObj.SetActive(true);
                        replicatedObjects.Add(instantiatedObj);

                        //Debug.Log("made instance");
                    }

                }
                objectsPopulated = true;
            }
            else if (currentReplicatorShape == ReplicatorShape.Grid)
            {
                int totalGridCount = gridSize.x * gridSize.y * gridSize.z;
                //Debug.Log("it's a grid");
                for (int i = 0; i < totalGridCount; i++)
                {
                    //Debug.Log("there are " + objectsToReplicate.Count + " objects in the list.");
                    GameObject obj = objectsToReplicate[i % objectsToReplicate.Count];
                    GameObject instantiatedObj = Instantiate(obj, currentReplicatorParent.transform);
                    instantiatedObj.SetActive(true);
                    replicatedObjects.Add(instantiatedObj);

                    //Debug.Log("made instance");
                }
                objectsPopulated = true;
            }

            else if (currentReplicatorShape == ReplicatorShape.Object)
            {
                if (shapeObject == null)
                {
                    Debug.LogWarning("Please select a Shape Object");
                    return;
                }
                if (shapeObject.GetComponentInChildren<MeshFilter>() == false)
                {
                    Debug.LogWarning("Your Shape Object must have a Mesh Filter component attached to it.");
                    return;
                }
                if (shapeObject.GetComponentInChildren<MeshFilter>().sharedMesh == null)
                {
                    Debug.LogWarning("Your Shape Object must have a Mesh Filter component attached to it that is referencing a mesh.");
                    return;
                }
                int vertexCount = shapeObject.GetComponentInChildren<MeshFilter>().sharedMesh.vertexCount;
                for (int i = 0; i < (replications / objectsToReplicate.Count); i++)
                {
                    //Debug.Log("there are " + objectsToReplicate.Count + " objects in the list.");

                    foreach (GameObject obj in objectsToReplicate)
                    {
                        if (replicatedObjects.Count >= vertexCount)
                        {
                            Debug.LogWarning("When using Object mode, you can only make as many replications as there are verticies in your Shape Object.");
                            replications = vertexCount;
                            return;
                        }
                        GameObject instantiatedObj = Instantiate(obj, currentReplicatorParent.transform);
                        instantiatedObj.SetActive(true);
                        replicatedObjects.Add(instantiatedObj);

                        //Debug.Log("made instance");
                    }
                    objectsPopulated = true;
                }

            }
        }

        public List<GameObject> RandomizeReplicationOrder(List<GameObject> listToRearrange)
        {
            
            if (listHasBeenRandomized == true)
            {
                //Debug.Log("list is already random.");
                return listToRearrange;
            }

            UnityEngine.Random.InitState(randomSeed);
            for (int i = listToRearrange.Count - 1; i > 0; i--)
            {
                // Randomize a number between 0 and i (so that the range decreases each time)
                int rnd = UnityEngine.Random.Range(0, i);

                // Save the value of the current i, otherwise it'll overwrite when the values are swapped.
                GameObject temp = listToRearrange[i];

                // Swap the new and old values
                listToRearrange[i] = listToRearrange[rnd];
                listToRearrange[rnd] = temp;
            }



            listHasBeenRandomized = true;

            return listToRearrange;
        }


        void ArrangeObjects(List<GameObject> originalOrder)
        {
            List<GameObject> objectsToModify = new List<GameObject>();

            if (randomOrder == true)
            {
                //Debug.Log("random order be true");
                objectsToModify = RandomizeReplicationOrder(originalOrder);
            }
            else
            {
                objectsToModify = originalOrder;
            }

            //Debug.Log("arranging");
            if (objectsPopulated == false) { return; }
            switch (currentReplicatorShape)
            {
                case ReplicatorShape.Line:
                    Arranger.LineArrange(objectsToModify, lineStart, lineEnd);
                    break;
                case ReplicatorShape.Circle:
                    Arranger.CircleArrange(objectsToModify, circleRadius);
                    break;
                case ReplicatorShape.Sphere:
                    Arranger.SphereArrange(objectsToModify, sphereTaper, sphereSquish, sphereVortex, sphereRadius);
                    break;
                case ReplicatorShape.Object:
                    Arranger.ObjectArrange(objectsToModify, shapeObject, shapeScale);
                    break;
                case ReplicatorShape.Grid:
                    Arranger.GridArrange(objectsToModify, gridSize, gridSpacing);
                    break;
            }

            //RecallStoredParentPosition();

            ModifyObjects(objectsToModify);
        }

        void ModifyObjects(List<GameObject> objectsToModify)
        {

            if (lookAt == null)
            {
                //Arranger.ModifyRotation(objectsToModify,lookAt,objectRotationOffset,objectRotationRandom,randomSeed);
                Arranger.OffsetScale(objectsToModify, objectScaleOffsetXYZ,objectScaleOffset);
                Arranger.ModifyRandomScale(objectsToModify, randomSeed, objectScaleRandomXYZ, objectScaleRandom);
                Arranger.ModifyObjects(objectsToModify, objectPositionOffset, objectRotationOffset, objectPositionRandom, objectRotationRandom, randomSeed);
                Arranger.OffsetPosition(objectsToModify, objectPositionOffset);
                Arranger.ModifyRandomPosition(objectsToModify, randomSeed, objectPositionRandom);
            }
            else if (lookAt != null)
            {
                Arranger.ModifyRotation(objectsToModify,lookAt,objectRotationOffset,objectRotationRandom,randomSeed);
                Arranger.OffsetScale(objectsToModify, objectScaleOffsetXYZ,objectScaleOffset);
                Arranger.ModifyRandomScale(objectsToModify, randomSeed, objectScaleRandomXYZ, objectScaleRandom);
                //Arranger.ModifyObjects(objectsToModify, objectPositionOffset, objectRotationOffset, objectPositionRandom, objectRotationRandom, randomSeed);
                Arranger.OffsetPosition(objectsToModify, objectPositionOffset);
                Arranger.ModifyRandomPosition(objectsToModify, randomSeed, objectPositionRandom);
            }
        }

        void RecallStoredParentPosition()
        {
            if (storedParentPosition != null && currentReplicatorParent != null)
            {
                //Debug.Log("setting current parent to stored values");
                //currentReplicatorParent.transform.SetLocalPositionAndRotation(storedParentPosition, storedParentRotation);
                currentReplicatorParent.transform.position = storedParentPosition;
                currentReplicatorParent.transform.rotation = storedParentRotation;
                currentReplicatorParent.transform.localScale = storedParentScale;
            }
        }

        void StoreCurrentParentPosition()
        {
            //Debug.Log("storing current parent position");
            if (currentReplicatorParent != null)
            {
                storedParentPosition = currentReplicatorParent.transform.localPosition;
                storedParentRotation = currentReplicatorParent.transform.localRotation;
                storedParentScale = currentReplicatorParent.transform.localScale;
            }
        }

        void ClearObjects()
        {
            //cachedMeshes.Clear();
            StoreCurrentParentPosition();
            //Debug.Log("storing current" + storedParentPosition);

            foreach (var obj in replicatedObjects)
            {
                DestroyImmediate(obj);
            }

            replicatedObjects.Clear();

            if (currentReplicatorParent != null)
            {
                DestroyImmediate(currentReplicatorParent);
            }

            if (currentReplicatorPreview != null)
            {
                DestroyImmediate(currentReplicatorPreview);
            }

            objectsPopulated = false;

            so.ApplyModifiedProperties();
        }

        void Build()
        {

            var noiseEnableCache = noiseEnable;
            var twistEnableCache = twistEnable;
            noiseEnable = false;
            twistEnable = false;
            UpdatePreview();

            currentReplicatorParent.name = "Replicator Parent";
            currentReplicatorPreview = null;
            ReplicatorController rc = currentReplicatorParent.AddComponent<ReplicatorController>();
            rc.replicatedObjectInstances.AddRange(replicatedObjects);
            rc.replicatedSourceObjects.AddRange(objectsToReplicate);
            rc.randomOrder = randomOrder;
            rc.lookAt = lookAt;
            rc.Initialize(new ModificationValues(objectPositionOffset, objectRotationOffset, objectPositionRandom, objectRotationRandom, randomSeed, objectScaleRandom, objectScaleOffsetXYZ, objectScaleRandomXYZ,objectScaleOffset),
                          new ShapeValues(replications, currentReplicatorShape, lineStart, lineEnd, circleRadius, sphereRadius, sphereVortex, sphereSquish, shapeObject, gridSize, gridSpacing, shapeScale),
                          new DeformValues(noiseEnableCache, twistEnableCache, noiseAmount, noiseFrequency, twistAmount, twistAxis
                          ));
            noiseEnable = noiseEnableCache;
            twistEnable = twistEnableCache;

            rc.transform.SetLocalPositionAndRotation(currentReplicatorParent.transform.localPosition, currentReplicatorParent.transform.localRotation);
            rc.transform.localScale = currentReplicatorParent.transform.localScale;
            //Debug.Log("set replicator instance position to current");

            StoreCurrentParentPosition();
            //Debug.Log("storing current" + storedParentPosition);

            if (isActivePreview == false)
            {
                UpdatePreview();
            }

            foreach (var pos in Arranger.currentShapePositions)
            {
                rc.originalShapePositions.Add(pos.Key, pos.Value);
            }

            foreach (var pos in Arranger.currentShapeRotations)
            {
                rc.originalShapeRotations.Add(pos.Key, pos.Value);
            }

            foreach (var pos in Arranger.currentShapeScales)
            {
                rc.originalShapeScales.Add(pos.Key, pos.Value);
            }

            currentReplicatorParent = null;
            replicatedObjects.Clear();
            Arranger.currentShapePositions.Clear();
            Arranger.currentShapeRotations.Clear();
            Arranger.currentShapeScales.Clear();
            isActivePreview = false;

        }

        private void OnDestroy()
        {

            ClearObjects();

            // When the window is destroyed, remove the delegate
            // so that it will no longer do any drawing.
            SceneView.duringSceneGui -= this.OnSceneGUI;

        }
        void OnFocus()
        {
            // Remove delegate listener if it has previously
            // been assigned.
            SceneView.duringSceneGui -= this.OnSceneGUI;
            // Add (or re-add) the delegate.
            SceneView.duringSceneGui += this.OnSceneGUI;
        }

        void ResetAllValues()
        {

            replicatedObjects.Clear();

            objectsToReplicate.Clear();

            currentReplicatorShape = ReplicatorShape.Line;

            objectPositionRandom = Vector3.zero;
            objectRotationRandom = Vector3.zero;
            objectRotationOffset = Vector3.zero;
            objectPositionOffset = Vector3.zero;
            objectScaleOffsetXYZ = Vector3.zero;
            objectScaleRandom = 0f;
            objectScaleRandomXYZ = new Vector3(0, 0, 0);
            randomSeed = 0;

            //deform fields
            twistEnable = false;
            twistAmount = 0f;
            noiseEnable = false;
            noiseAmount = 0f;
            noiseFrequency = 0f;
            twistAxis = Vector3.zero;

            showLineFields = new AnimBool(true);
            showObjectFields = new AnimBool(false);
            showSphereFields = new AnimBool(false);
            showCircleFields = new AnimBool(false);
            showGridFields = new AnimBool(false);

            lookAt = null;
            replications = 1;

            autoArrange = true;


            //SHAPES FIELDS

            //line
            lineStart = Vector3.zero;
            lineEnd = new Vector3(1, 0, 0);

            //circle
            circleRadius = 5f;

            //sphere
            sphereRadius = 5f;
            sphereVortex = 1f;
            sphereSquish = 1f;
            sphereTaper = 0;

            //object
            shapeObject = null;
            shapeScale = 1f;

            //grid
            gridSize = new Vector3Int(1,1,1);
            gridSpacing = Vector3.one;
    }

        void OnSceneGUI(SceneView sceneView)
        {
            float size = HandleUtility.GetHandleSize(this.lineStart) * 0.5f;
            Vector3 snap = Vector3.one * 0.5f;

            // Do your drawing here using Handles.
            EditorGUI.BeginChangeCheck();
            if (currentReplicatorShape == ReplicatorShape.Line && showGizmos == true && isActivePreview)
            {
                lineStart = Handles.PositionHandle(lineStart, Quaternion.identity);
                lineEnd = Handles.PositionHandle(lineEnd, Quaternion.identity);
            }
            if (currentReplicatorShape == ReplicatorShape.Sphere && showGizmos == true && isActivePreview)
            {
                sphereRadius = Handles.RadiusHandle(Quaternion.identity, currentReplicatorParent.transform.position, sphereRadius);

            }

            // Do your drawing here using GUI.
            if (EditorGUI.EndChangeCheck())
            {
                UpdatePreview();
            }
            Handles.EndGUI();
        }
    }
}
