using UnityEditor;
using UnityEngine;

public class PixelArtImportSetup : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/Sprites/"))
            return;

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;

        if (assetPath.Contains("/Background/"))
        {
            importer.filterMode = FilterMode.Point;
            // 272x160 @ 28 PPU = 9.71 x 5.71 world units (eliminates giant chunky background pixels on portrait phones!)
            importer.spritePixelsPerUnit = 28f;
        }
        else if (assetPath.Contains("/Slime/"))
        {
            importer.filterMode = FilterMode.Bilinear;
            // 758x466 full-union canvas (with 344px Slime body centered): 350 PPU makes the Slime body ~0.98 world units wide with 0% animation clipping!
            importer.spritePixelsPerUnit = 350f;
        }
        else if (assetPath.Contains("/SplashCutscene/") || assetPath.Contains("/HomeSlimeEmotions/"))
        {
            importer.filterMode = FilterMode.Bilinear;
            importer.spritePixelsPerUnit = 200f;
        }
        else if (assetPath.Contains("/UI/"))
        {
            importer.filterMode = FilterMode.Point;
            importer.spritePixelsPerUnit = 32f;
        }
        else if (assetPath.EndsWith("bonus_heart.png"))
        {
            importer.filterMode = FilterMode.Point;
            // 32px / 52 PPU = ~0.61 world units wide
            importer.spritePixelsPerUnit = 52f;
        }
        else
        {
            // Food, Hazards, Pickups (18x18 pixel art): 18px / 30 PPU = 0.60 world units wide
            importer.filterMode = FilterMode.Point;
            importer.spritePixelsPerUnit = 30f;
        }
    }

    void OnPreprocessAudio()
    {
        if (!assetPath.StartsWith("Assets/Audio/"))
            return;

        AudioImporter audioImporter = (AudioImporter)assetImporter;
        bool isBgm = assetPath.Contains("BGM_") || assetPath.Contains("Ambience");

        audioImporter.loadInBackground = isBgm;
        AudioImporterSampleSettings settings = audioImporter.defaultSampleSettings;
        if (isBgm)
        {
            // Keep BGM compressed in memory and loaded asynchronously in background so theme transitions NEVER hitch or freeze!
            settings.loadType = AudioClipLoadType.CompressedInMemory;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.65f;
            settings.preloadAudioData = true;
        }
        else
        {
            // Short SFX clips decompressed on load for instant zero-latency playback
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.preloadAudioData = true;
        }
        audioImporter.defaultSampleSettings = settings;
        audioImporter.SetOverrideSampleSettings("Android", settings);
        audioImporter.SetOverrideSampleSettings("Standalone", settings);
    }

    public static void ForceReimportAllSprites()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        string[] audioGuids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio" });
        foreach (string guid in audioGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
