using Control.Tools;
using UnityEditor;
using UnityEngine;

namespace Control.Tools.Editor
{
    [CustomEditor(typeof(GradientTextureGenerator))]
    public sealed class GradientTextureGeneratorEditor : UnityEditor.Editor
    {
        private SerializedProperty generatedTextureProperty;
        private SerializedProperty gradientProperty;
        private SerializedProperty widthProperty;

        private void OnEnable()
        {
            gradientProperty = serializedObject.FindProperty("gradient");
            widthProperty = serializedObject.FindProperty("width");
            generatedTextureProperty = serializedObject.FindProperty("generatedTexture");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(gradientProperty);
            EditorGUILayout.PropertyField(widthProperty);
            bool changed = EditorGUI.EndChangeCheck();

            serializedObject.ApplyModifiedProperties();

            if (changed)
                RegenerateTargets();

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(generatedTextureProperty);
            }

            DrawPreview(generatedTextureProperty.objectReferenceValue as Texture2D);

            if (GUILayout.Button("Regenerate"))
                RegenerateTargets();
        }

        private void RegenerateTargets()
        {
            foreach (Object selectedTarget in targets)
            {
                if (selectedTarget is GradientTextureGenerator generator)
                    generator.Regenerate();
            }
        }

        private static void DrawPreview(Texture2D texture)
        {
            if (texture == null)
                return;

            Rect previewRect = GUILayoutUtility.GetRect(1f, 32f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawPreviewTexture(previewRect, texture, null, ScaleMode.StretchToFill);
        }
    }
}
