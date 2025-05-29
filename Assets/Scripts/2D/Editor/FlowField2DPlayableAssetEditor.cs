using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FlowField2DPlayableAsset))]
public class FlowField2DPlayableAssetEditor : Editor
{
    private SerializedProperty prop;
    private bool fieldFoldout = true;
    private bool controllerFoldout = true;
    private bool emitterFoldout = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var asset = (FlowField2DPlayableAsset)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Flow Field 2D Timeline Clip", EditorStyles.boldLabel);

        // -----------------------
        // Flow Field Volume
        asset.modifyField = EditorGUILayout.Toggle("Modify Flow Field", asset.modifyField);
        if (asset.modifyField)
        {
            fieldFoldout = EditorGUILayout.Foldout(fieldFoldout, "Flow Field Settings", true);
            if (fieldFoldout)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
                asset.cols = EditorGUILayout.IntField("Cols", asset.cols);
                asset.rows = EditorGUILayout.IntField("Rows", asset.rows);
                asset.cellSize = EditorGUILayout.FloatField("Cell Size", asset.cellSize);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Bounds", EditorStyles.boldLabel);
                asset.perAxisBounds = EditorGUILayout.Toggle("Per Axis Bounds", asset.perAxisBounds);
                if (asset.perAxisBounds)
                {
                    asset.boundsX = (BoundsType2D)EditorGUILayout.EnumPopup("Bounds X", asset.boundsX);
                    asset.boundsY = (BoundsType2D)EditorGUILayout.EnumPopup("Bounds Y", asset.boundsY);
                }
                else
                {
                    asset.bounds = (BoundsType2D)EditorGUILayout.EnumPopup("Bounds", asset.bounds);
                }
                asset.closedBoundRamp = EditorGUILayout.Slider("Closed Bound Ramp", asset.closedBoundRamp, 0f, 1f);
               
                EditorGUILayout.EndVertical();
            }
        }

        // -----------------------
        // Particle Controller
        asset.modifyController = EditorGUILayout.Toggle("Modify Particle Controller", asset.modifyController);
        if (asset.modifyController)
        {
            controllerFoldout = EditorGUILayout.Foldout(controllerFoldout, "Particle Controller Settings", true);
            if (controllerFoldout)
            {
                EditorGUILayout.BeginVertical("box");

                asset.tightness = EditorGUILayout.Slider("Tightness", asset.tightness, 0f, 1f);
                asset.MinInfluence = EditorGUILayout.Slider("Min Influence", asset.MinInfluence, 0.05f, 1f);
                asset.multiplier = EditorGUILayout.FloatField("Multiplier", asset.multiplier);

                asset.AnimateTightness = EditorGUILayout.Toggle("Animate Tightness", asset.AnimateTightness);
                if (asset.AnimateTightness)
                {
                    asset.TightnessOverTime = EditorGUILayout.CurveField("Tightness Over Time", asset.TightnessOverTime);
                }

                asset.AnimateMultiplier = EditorGUILayout.Toggle("Animate Multiplier", asset.AnimateMultiplier);
                if (asset.AnimateMultiplier)
                {
                    asset.MultiplierOverTime = EditorGUILayout.CurveField("Multiplier Over Time", asset.MultiplierOverTime);
                }

                EditorGUILayout.EndVertical();
            }
        }

        // -----------------------
        // Particle Emitter
        asset.modifyEmitter = EditorGUILayout.Toggle("Modify Emitter", asset.modifyEmitter);
        if (asset.modifyEmitter)
        {
            emitterFoldout = EditorGUILayout.Foldout(emitterFoldout, "Particle Emitter Settings", true);
            if (emitterFoldout)
            {
                EditorGUILayout.BeginVertical("box");

                asset.emissionType = (FlowField2DParticleEmitter.Emission)EditorGUILayout.EnumPopup("Emission Type", asset.emissionType);
                asset.coverage = EditorGUILayout.Slider("Coverage", asset.coverage, 0f, 1f);

                EditorGUILayout.EndVertical();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}