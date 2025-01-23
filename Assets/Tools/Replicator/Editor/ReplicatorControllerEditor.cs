using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Replicator
{

    [CustomEditor(typeof(ReplicatorController))]
    [CanEditMultipleObjects]
    public class ReplicatorControllerEditor : Editor
    {
        //replicator
        SerializedProperty propReplications;
        SerializedProperty propReplicatorShape;
        SerializedProperty propRandomOrder;
        SerializedProperty propObjectsToReplicate;
        SerializedProperty propReplicatedObjects;
        SerializedProperty propLookAt;
        SerializedProperty propAutoArrange;
        SerializedProperty propShapeObject;
        SerializedProperty propCircleRadius;
        SerializedProperty propSphereRadius;
        SerializedProperty propSphereVortex;
        SerializedProperty propSphereSquish;
        //SerializedProperty propSphereTaper;
        SerializedProperty propLineStart;
        SerializedProperty propLineEnd;
        SerializedProperty propGridSpacing;
        SerializedProperty propGridSize;
        

        //modifications
        SerializedProperty objectPositionOffset;
        SerializedProperty objectRotationOffset;
        SerializedProperty objectPositionRandom;
        SerializedProperty objectRotationRandom;
        SerializedProperty objectScaleOffset;
        SerializedProperty objectScaleOffsetXYZ;
        SerializedProperty objectScaleRandom;
        SerializedProperty objectScaleRandomXYZ;
        SerializedProperty randomSeed;

        SerializedProperty propNoiseAmount;
        SerializedProperty propNoiseEnable;
        SerializedProperty propTwistAmount;
        SerializedProperty propTwistEnable;

        ReplicatorController rc;

        private Texture2D lightLogo = null;
        private Texture2D darkLogo = null;

        // Start is called before the first frame update
        void OnEnable()
        {

            rc = (ReplicatorController)target;

            //logo setup
            lightLogo = (Texture2D)Resources.Load("Replicator/ReplicatorLogo_Light", typeof(Texture2D));
            darkLogo = (Texture2D)Resources.Load("Replicator/ReplicatorLogo_Dark", typeof(Texture2D));

            //shape options
            propReplications = serializedObject.FindProperty("replications");
            propReplicatorShape = serializedObject.FindProperty("currentReplicatorShape");
            propRandomOrder = serializedObject.FindProperty("randomOrder");
            propObjectsToReplicate = serializedObject.FindProperty("objectsToReplicate");
            propReplicatedObjects = serializedObject.FindProperty("replicatedObjects");
            propAutoArrange = serializedObject.FindProperty("autoArrange");
            propShapeObject = serializedObject.FindProperty("shapeObject");
            //propShapeScale = serializedObject.FindProperty("shapeScale");
            propCircleRadius = serializedObject.FindProperty("circleRadius");
            propSphereRadius = serializedObject.FindProperty("sphereRadius");
            propSphereVortex = serializedObject.FindProperty("sphereVortex");
            propSphereSquish = serializedObject.FindProperty("sphereSquish");
            //propSphereTaper = serializedObject.FindProperty("sphereTaper");
            propLineStart = serializedObject.FindProperty("lineStart");
            propLineEnd = serializedObject.FindProperty("lineEnd");
            propGridSpacing = serializedObject.FindProperty("gridSpacing");
            propLookAt = serializedObject.FindProperty("lookAt");

            //offsets
            objectPositionOffset = serializedObject.FindProperty("objectPositionOffset");
            objectRotationOffset = serializedObject.FindProperty("objectRotationOffset");
            objectScaleOffset = serializedObject.FindProperty("objectScaleOffset");
            objectScaleOffsetXYZ = serializedObject.FindProperty("objectScaleOffsetXYZ");

            //randomness
            objectPositionRandom = serializedObject.FindProperty("objectPositionRandom");
            objectRotationRandom = serializedObject.FindProperty("objectRotationRandom");
            objectScaleRandom = serializedObject.FindProperty("objectScaleRandom");
            objectScaleRandomXYZ = serializedObject.FindProperty("objectScaleRandomXYZ");
            randomSeed = serializedObject.FindProperty("randomSeed");

            //deforms
            propNoiseEnable = serializedObject.FindProperty("noiseEnable");
            propNoiseAmount = serializedObject.FindProperty("noiseAmount");
            propTwistEnable = serializedObject.FindProperty("twistEnable");
            propTwistAmount = serializedObject.FindProperty("twistAmount");

        }

        void OnSceneGUI()
        {
            float size = HandleUtility.GetHandleSize(rc.lineStart) * 0.5f;
            Vector3 snap = Vector3.one * 0.5f;

            // Do your drawing here using Handles.
            EditorGUI.BeginChangeCheck();
            if (rc.currentReplicatorShape == ReplicatorShape.Line)
            {
                rc.lineStart = Handles.PositionHandle(rc.lineStart, Quaternion.identity);
                rc.lineEnd = Handles.PositionHandle(rc.lineEnd, Quaternion.identity);
            }
            if (rc.currentReplicatorShape == ReplicatorShape.Sphere)
            {
                rc.sphereRadius = Handles.RadiusHandle(Quaternion.identity, rc.transform.position, rc.sphereRadius);
            }
            // Do your drawing here using GUI.
            if (EditorGUI.EndChangeCheck())
            {
                //UpdatePreview();
            }
            Handles.EndGUI();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (EditorGUIUtility.isProSkin)
            {
                GUILayout.Label(lightLogo, GUILayout.Height(60));
            }
            else if (!EditorGUIUtility.isProSkin)
            {
                GUILayout.Label(darkLogo, GUILayout.Height(60));
            }
            EditorGUILayout.LabelField("Shape Options", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(propLookAt, false);

            //Debug.Log("replicator shape: "+propReplicatorShape.intValue);
            switch (propReplicatorShape.intValue)
            {
                case (int)ReplicatorShape.Line:
                    {
                        EditorGUILayout.PropertyField(propLineStart, false);
                        EditorGUILayout.PropertyField(propLineEnd, false);
                        break;
                    }

                case (int)ReplicatorShape.Object:
                    {
                        //EditorGUILayout.PropertyField(propShapeScale, false);
                        break;
                    }

                case (int)ReplicatorShape.Sphere:
                    {
                        EditorGUILayout.PropertyField(propSphereRadius, false);
                        break;
                    }

                case (int)ReplicatorShape.Circle:
                    {
                        EditorGUILayout.PropertyField(propCircleRadius, false);
                        break;
                    }
                case (int)ReplicatorShape.Grid:
                    {
                        EditorGUILayout.PropertyField(propGridSpacing, false);
                        break;
                    }
            }


            EditorGUILayout.LabelField("Offset", EditorStyles.boldLabel);
            //EditorGUILayout.PropertyField(objectPositionOffset, false);
            EditorGUILayout.PropertyField(objectRotationOffset, false);
            EditorGUILayout.PropertyField(objectScaleOffset, false);
            EditorGUILayout.PropertyField(objectScaleOffsetXYZ, false);

            EditorGUILayout.LabelField("Random", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(objectPositionRandom, false);
            EditorGUILayout.PropertyField(objectRotationRandom, false);
            EditorGUILayout.PropertyField(objectScaleRandom, false);
            EditorGUILayout.PropertyField(objectScaleRandomXYZ, false);
            EditorGUILayout.PropertyField(randomSeed, false);

            EditorGUILayout.LabelField("Deform", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(propNoiseEnable, false);
            EditorGUILayout.PropertyField(propNoiseAmount, false);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(propTwistEnable, false);
            EditorGUILayout.PropertyField(propTwistAmount, false);
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                //Debug.Log("Deforming!");
                serializedObject.ApplyModifiedProperties();
                rc.DeformValuesChanged = true;
                rc.DeformObjects();
                
            }

            serializedObject.ApplyModifiedProperties();
        }



    }
    

}


