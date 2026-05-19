using System;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Control.Tools
{
    [CreateAssetMenu(fileName = "GradientTextureGenerator", menuName = "Control Tools/Gradient Texture Generator")]
    public sealed class GradientTextureGenerator : ScriptableObject
    {
        private const int DefaultWidth = 256;
        private const int TextureHeight = 1;

#if UNITY_EDITOR
        private bool regenerationQueued;
#endif

        [SerializeField]
        private Gradient gradient = new();

        [SerializeField, Min(1)]
        private int width = DefaultWidth;

        [SerializeField, HideInInspector]
        private Texture2D generatedTexture;

        // Preserves assets created when the generator inherited Odin's SerializedScriptableObject.
#pragma warning disable 0169
        [SerializeField, HideInInspector]
        private LegacyOdinSerializationData serializationData;
#pragma warning restore 0169

        public Gradient Gradient => gradient;

        public int Width => width;

        public Texture2D Texture => generatedTexture;

        public void Regenerate()
        {
#if UNITY_EDITOR
            RegenerateInternal();
#endif
        }

        private bool ClampWidth()
        {
            int clampedWidth = Mathf.Max(1, width);
            if (width == clampedWidth)
                return false;

            width = clampedWidth;
            return true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            QueueRegeneration();
        }

        private void QueueRegeneration()
        {
            if (regenerationQueued)
                return;

            regenerationQueued = true;
            EditorApplication.delayCall += RegenerateFromDelayCall;
        }

        private void RegenerateFromDelayCall()
        {
            regenerationQueued = false;

            if (this == null)
                return;

            if (AssetDatabase.IsAssetImportWorkerProcess())
                return;

            RegenerateInternal();
        }

        private void RegenerateInternal()
        {
            bool generatorChanged = ClampWidth();

            if (EditorApplication.isCompiling)
                return;

            if (AssetDatabase.IsAssetImportWorkerProcess())
                return;

            string assetPath = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(assetPath))
                return;

            bool textureChanged = EnsureGeneratedTextureExists(assetPath, out bool generatorReferenceChanged);
            generatorChanged |= generatorReferenceChanged;
            textureChanged |= UpdateTexturePixels();

            if (textureChanged)
            {
                EditorUtility.SetDirty(generatedTexture);
                AssetDatabase.SaveAssetIfDirty(generatedTexture);
            }

            if (generatorChanged)
            {
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssetIfDirty(this);
            }
        }

        private bool EnsureGeneratedTextureExists(string assetPath, out bool generatorReferenceChanged)
        {
            generatorReferenceChanged = false;
            bool changed = false;

            if (generatedTexture != null && AssetDatabase.IsSubAsset(generatedTexture))
            {
                changed |= ApplyTextureAssetSettings(generatedTexture);
                return changed;
            }

            generatedTexture = FindExistingGeneratedTexture(assetPath);
            generatorReferenceChanged = true;

            if (generatedTexture == null)
            {
                generatedTexture = new Texture2D(width, TextureHeight, TextureFormat.RGBA32, false)
                {
                    name = name,
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear,
                    hideFlags = HideFlags.None,
                };

                AssetDatabase.AddObjectToAsset(generatedTexture, this);
                return true;
            }

            changed |= ApplyTextureAssetSettings(generatedTexture);
            return changed;
        }

        private Texture2D FindExistingGeneratedTexture(string assetPath)
        {
            UnityObject[] bundledAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (UnityObject bundledAsset in bundledAssets)
            {
                if (bundledAsset == null || bundledAsset == this)
                    continue;

                if (bundledAsset is Texture2D texture)
                    return texture;
            }

            return null;
        }

        private bool ApplyTextureAssetSettings(Texture2D texture)
        {
            bool changed = false;

            if (texture.name != name)
            {
                texture.name = name;
                changed = true;
            }

            if (texture.hideFlags != HideFlags.None)
            {
                texture.hideFlags = HideFlags.None;
                changed = true;
            }

            return changed;
        }

        private bool UpdateTexturePixels()
        {
            bool changed = false;

            if (generatedTexture.width != width || generatedTexture.height != TextureHeight)
            {
                generatedTexture.Reinitialize(width, TextureHeight, TextureFormat.RGBA32, false);
                changed = true;
            }

            if (generatedTexture.wrapMode != TextureWrapMode.Clamp)
            {
                generatedTexture.wrapMode = TextureWrapMode.Clamp;
                changed = true;
            }

            if (generatedTexture.filterMode != FilterMode.Bilinear)
            {
                generatedTexture.filterMode = FilterMode.Bilinear;
                changed = true;
            }

            Color32[] pixels = new Color32[width];
            float denominator = width > 1 ? width - 1f : 1f;

            for (int x = 0; x < width; x++)
            {
                float t = width == 1 ? 0f : x / denominator;
                pixels[x] = gradient.Evaluate(t);
            }

            if (changed || !TexturePixelsMatch(pixels))
            {
                generatedTexture.SetPixels32(pixels);
                generatedTexture.Apply(false, false);
                changed = true;
            }

            return changed;
        }

        private bool TexturePixelsMatch(Color32[] expectedPixels)
        {
            if (!generatedTexture.isReadable)
                return false;

            Color32[] existingPixels = generatedTexture.GetPixels32();
            if (existingPixels.Length != expectedPixels.Length)
                return false;

            for (int i = 0; i < expectedPixels.Length; i++)
            {
                if (!existingPixels[i].Equals(expectedPixels[i]))
                    return false;
            }

            return true;
        }
#endif

        [Serializable]
        private struct LegacyOdinSerializationData
        {
            public int SerializedFormat;
            public byte[] SerializedBytes;
            public List<UnityObject> ReferencedUnityObjects;
            public string SerializedBytesString;
            public UnityObject Prefab;
            public List<UnityObject> PrefabModificationsReferencedUnityObjects;
            public List<string> PrefabModifications;
            public List<LegacyOdinSerializationNode> SerializationNodes;
        }

        [Serializable]
        private struct LegacyOdinSerializationNode
        {
            public string Name;
            public int Entry;
            public int DataFormat;
            public string StringData;
            public byte[] Data;
        }
    }
}
