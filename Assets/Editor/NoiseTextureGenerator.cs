// ─────────────────────────────────────────────────────────────────────────────
// NoiseTextureGenerator.cs
// Place inside an "Editor" folder  →  Assets/VolumetricFog/Editor/
// ─────────────────────────────────────────────────────────────────────────────
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor utility window that bakes layered Perlin noise into
///   • A 3D Texture3D  (used as _NoiseTex in the volumetric fog shader)
///   • A 2D Texture2D  (used as _NoiseTex2D for detail)
/// Run once; the assets are saved to Assets/VolumetricFog/GeneratedTextures/.
/// </summary>
public class NoiseTextureGenerator : EditorWindow
{
    // ── 3D Texture Settings ──────────────────────────────────────────────────
    int   tex3DSize      = 64;
    float tex3DScale     = 4f;
    int   tex3DOctaves   = 4;
    float tex3DLacunarity = 2f;
    float tex3DPersist   = 0.5f;

    // ── 2D Texture Settings ──────────────────────────────────────────────────
    int   tex2DSize      = 256;
    float tex2DScale     = 3f;
    int   tex2DOctaves   = 5;
    float tex2DLacunarity = 2f;
    float tex2DPersist   = 0.5f;

    // ────────────────────────────────────────────────────────────────────────
    [MenuItem("Tools/Volumetric Fog/Generate Noise Textures")]
    static void OpenWindow()
    {
        GetWindow<NoiseTextureGenerator>("Noise Generator").Show();
    }

    void OnGUI()
    {
        GUILayout.Label("3D Noise (Texture3D)", EditorStyles.boldLabel);
        tex3DSize       = EditorGUILayout.IntField("Size (power of 2)",   tex3DSize);
        tex3DScale      = EditorGUILayout.FloatField("Scale",             tex3DScale);
        tex3DOctaves    = EditorGUILayout.IntField("Octaves",             tex3DOctaves);
        tex3DLacunarity = EditorGUILayout.FloatField("Lacunarity",        tex3DLacunarity);
        tex3DPersist    = EditorGUILayout.FloatField("Persistence",       tex3DPersist);

        EditorGUILayout.Space();
        GUILayout.Label("2D Noise (Texture2D)", EditorStyles.boldLabel);
        tex2DSize       = EditorGUILayout.IntField("Size (power of 2)",   tex2DSize);
        tex2DScale      = EditorGUILayout.FloatField("Scale",             tex2DScale);
        tex2DOctaves    = EditorGUILayout.IntField("Octaves",             tex2DOctaves);
        tex2DLacunarity = EditorGUILayout.FloatField("Lacunarity",        tex2DLacunarity);
        tex2DPersist    = EditorGUILayout.FloatField("Persistence",       tex2DPersist);

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate & Save"))
        {
            Generate3DTexture();
            Generate2DTexture();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Done", "Noise textures saved to Assets/VolumetricFog/GeneratedTextures/", "OK");
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    void EnsureDir(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            var parts = path.Split('/');
            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }
    }

    // ── 3D ──────────────────────────────────────────────────────────────────
    void Generate3DTexture()
    {
        EnsureDir("Assets/VolumetricFog/GeneratedTextures");
        int   sz     = Mathf.NextPowerOfTwo(Mathf.Max(8, tex3DSize));
        var   tex    = new Texture3D(sz, sz, sz, TextureFormat.R8, false);
        tex.filterMode = FilterMode.Trilinear;
        tex.wrapMode   = TextureWrapMode.Repeat;

        Color[] cols = new Color[sz * sz * sz];
        float   invSz = 1f / sz;

        for (int z = 0; z < sz; z++)
        for (int y = 0; y < sz; y++)
        for (int x = 0; x < sz; x++)
        {
            float nx = x * invSz * tex3DScale;
            float ny = y * invSz * tex3DScale;
            float nz = z * invSz * tex3DScale;
            float v  = FractalNoise3D(nx, ny, nz, tex3DOctaves, tex3DLacunarity, tex3DPersist);
            cols[x + y * sz + z * sz * sz] = new Color(v, v, v, v);
        }

        tex.SetPixels(cols);
        tex.Apply();
        AssetDatabase.CreateAsset(tex, "Assets/VolumetricFog/GeneratedTextures/FogNoise3D.asset");
    }

    // ── 2D ──────────────────────────────────────────────────────────────────
    void Generate2DTexture()
    {
        EnsureDir("Assets/VolumetricFog/GeneratedTextures");
        int  sz   = Mathf.NextPowerOfTwo(Mathf.Max(16, tex2DSize));
        var  tex  = new Texture2D(sz, sz, TextureFormat.R8, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode   = TextureWrapMode.Repeat;

        Color[] cols = new Color[sz * sz];
        float   invSz = 1f / sz;

        for (int y = 0; y < sz; y++)
        for (int x = 0; x < sz; x++)
        {
            float nx = x * invSz * tex2DScale;
            float ny = y * invSz * tex2DScale;
            float v  = FractalNoise2D(nx, ny, tex2DOctaves, tex2DLacunarity, tex2DPersist);
            cols[x + y * sz] = new Color(v, v, v, v);
        }

        tex.SetPixels(cols);
        tex.Apply();
        AssetDatabase.CreateAsset(tex, "Assets/VolumetricFog/GeneratedTextures/FogNoise2D.asset");
    }

    // ── Fractal Noise Helpers ────────────────────────────────────────────────
    static float FractalNoise3D(float x, float y, float z, int oct, float lac, float per)
    {
        float val = 0f, amp = 1f, freq = 1f, maxVal = 0f;
        for (int i = 0; i < oct; i++)
        {
            val    += Mathf.PerlinNoise(x * freq, y * freq + z * freq * 0.3f) * amp;
            maxVal += amp;
            amp    *= per;
            freq   *= lac;
        }
        return val / maxVal;
    }

    static float FractalNoise2D(float x, float y, int oct, float lac, float per)
    {
        float val = 0f, amp = 1f, freq = 1f, maxVal = 0f;
        for (int i = 0; i < oct; i++)
        {
            val    += Mathf.PerlinNoise(x * freq, y * freq) * amp;
            maxVal += amp;
            amp    *= per;
            freq   *= lac;
        }
        return val / maxVal;
    }
}
#endif
