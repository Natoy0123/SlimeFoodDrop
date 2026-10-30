import os
import numpy as np
from PIL import Image

platforms_dir = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Sprites\Background\Platforms"
os.makedirs(platforms_dir, exist_ok=True)

W, H = 512, 64

def make_meta(png_name):
    meta_path = os.path.join(platforms_dir, png_name + ".meta")
    content = f"""fileFormatVersion: 2
guid: {os.urandom(16).hex()}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 0
    wrapV: 0
    wrapW: 0
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 28
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
"""
    with open(meta_path, "w", encoding="utf-8") as f:
        f.write(content)

# Theme Palette Definitions
themes = [
    ("platform_0_forest.png", (72, 168, 56), (112, 216, 72), (32, 80, 24), (52, 34, 24), (40, 24, 16)),
    ("platform_1_plains.png", (100, 200, 36), (146, 232, 68), (56, 120, 20), (88, 60, 36), (68, 44, 24)),
    ("platform_2_mountain.png", (140, 148, 160), (184, 192, 208), (84, 92, 104), (72, 76, 88), (52, 56, 68)),
    ("platform_3_tropical.png", (232, 200, 96), (252, 228, 136), (154, 120, 48), (184, 152, 72), (136, 108, 44)),
    ("platform_4_desert.png", (216, 136, 64), (244, 170, 96), (152, 84, 32), (168, 92, 40), (120, 56, 24)),
    ("platform_5_underwater.png", (32, 184, 160), (76, 232, 208), (16, 104, 88), (24, 56, 80), (14, 36, 56)),
    ("platform_6_industrial.png", (104, 112, 128), (152, 160, 176), (48, 52, 64), (56, 60, 72), (36, 40, 48)),
    ("platform_7_cave.png", (128, 72, 184), (184, 120, 240), (72, 36, 112), (36, 28, 48), (24, 18, 32)),
    ("platform_8_sky.png", (232, 228, 216), (255, 255, 255), (180, 150, 60), (168, 164, 152), (120, 116, 108)),
    ("platform_9_citadel.png", (112, 152, 184), (160, 208, 248), (56, 88, 120), (32, 40, 56), (20, 24, 36)),
]

np.random.seed(42)

for filename, top_c, hi_c, rim_c, body_c, deep_c in themes:
    arr = np.zeros((H, W, 4), dtype=np.uint8)
    
    # 1. Body Base
    for y in range(H):
        t = y / float(H)
        base = tuple(int(body_c[c] * (1 - t) + deep_c[c] * t) for c in range(3))
        for x in range(W):
            noise = np.random.randint(-6, 7)
            r = np.clip(base[0] + noise, 0, 255)
            g = np.clip(base[1] + noise, 0, 255)
            b = np.clip(base[2] + noise, 0, 255)
            arr[y, x] = (r, g, b, 255)
            
    # 2. Add subtle brick / rock seam texture
    for y in range(16, H, 12):
        for x in range(W):
            if (x + (y * 3)) % 32 == 0 or y % 12 == 0:
                arr[y, x, 0] = max(0, arr[y, x, 0] - 25)
                arr[y, x, 1] = max(0, arr[y, x, 1] - 25)
                arr[y, x, 2] = max(0, arr[y, x, 2] - 25)
    
    # 3. Top Rim & Highlights (Top 12 pixels)
    for x in range(W):
        # Organic edge height
        edge_y = int(3 + np.sin(x * 0.15) * 1.5 + np.cos(x * 0.05) * 1.0)
        
        # Transparent above edge
        for y in range(edge_y):
            arr[y, x] = (0, 0, 0, 0)
            
        # Top highlight line
        arr[edge_y, x] = (*hi_c, 255)
        arr[edge_y + 1, x] = (*hi_c, 255)
        
        # Main crest color
        for y in range(edge_y + 2, edge_y + 7):
            if y < H:
                noise = np.random.randint(-5, 6)
                r = np.clip(top_c[0] + noise, 0, 255)
                g = np.clip(top_c[1] + noise, 0, 255)
                b = np.clip(top_c[2] + noise, 0, 255)
                arr[y, x] = (r, g, b, 255)
                
        # Under crest shadow rim
        for y in range(edge_y + 7, edge_y + 10):
            if y < H:
                arr[y, x] = (*rim_c, 255)

    im = Image.fromarray(arr, "RGBA")
    p_path = os.path.join(platforms_dir, filename)
    im.save(p_path)
    make_meta(filename)
    print(f"Generated {filename}")

print("All 10 platform sprites generated successfully.")
