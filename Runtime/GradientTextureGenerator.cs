using UnityEngine;

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

        public Gradient Gradient => gradient;

        public int Width => width;

        public Texture2D Texture => generatedTexture;

        public void Regenerate()
        {
#if UNITY_EDITOR
            RegenerateInternal();
#endif
        }

        private void ClampWidth()
        {
            width = Mathf.Max(1, width);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ClampWidth();
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
            ClampWidth();

            if (EditorApplication.isCompiling)
                return;

            if (AssetDatabase.IsAssetImportWorkerProcess())
                return;

            string assetPath = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(assetPath))
                return;

            EnsureGeneratedTextureExists(assetPath);
            UpdateTexturePixels();

            EditorUtility.SetDirty(generatedTexture);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(generatedTexture);
            AssetDatabase.SaveAssetIfDirty(this);
        }

        private void EnsureGeneratedTextureExists(string assetPath)
        {
            if (generatedTexture != null && AssetDatabase.IsSubAsset(generatedTexture))
            {
                generatedTexture.name = name;
                generatedTexture.hideFlags = HideFlags.None;
                return;
            }

            generatedTexture = FindExistingGeneratedTexture(assetPath);

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
            }
            else
            {
                generatedTexture.name = name;
                generatedTexture.hideFlags = HideFlags.None;
            }
        }

        private Texture2D FindExistingGeneratedTexture(string assetPath)
        {
            Object[] bundledAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (Object bundledAsset in bundledAssets)
            {
                if (bundledAsset == null || bundledAsset == this)
                    continue;

                if (bundledAsset is Texture2D texture)
                    return texture;
            }

            return null;
        }

        private void UpdateTexturePixels()
        {
            if (generatedTexture.width != width || generatedTexture.height != TextureHeight)
                generatedTexture.Reinitialize(width, TextureHeight, TextureFormat.RGBA32, false);

            generatedTexture.wrapMode = TextureWrapMode.Clamp;
            generatedTexture.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[width];
            float denominator = width > 1 ? width - 1f : 1f;

            for (int x = 0; x < width; x++)
            {
                float t = width == 1 ? 0f : x / denominator;
                pixels[x] = gradient.Evaluate(t);
            }

            generatedTexture.SetPixels(pixels);
            generatedTexture.Apply(false, false);
        }
#endif
    }
}
