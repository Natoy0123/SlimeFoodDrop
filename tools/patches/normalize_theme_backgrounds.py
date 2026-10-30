import os
from PIL import Image

base = r'C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Sprites\Background'

TARGET_H = 384

def extend_vertical(folder, filename, is_sky=False):
    p = os.path.join(base, folder, filename)
    if not os.path.exists(p):
        print(f"File not found: {p}")
        return
    
    im = Image.open(p).convert("RGBA")
    w, h = im.size
    
    if h == TARGET_H:
        print(f"Already {TARGET_H}: {folder}/{filename}")
        return
    
    if h > TARGET_H:
        # Scale down preserving aspect ratio
        new_w = int(round(w * (TARGET_H / float(h))))
        im_scaled = im.resize((new_w, TARGET_H), Image.Resampling.LANCZOS)
        im_scaled.save(p)
        print(f"Rescaled {folder}/{filename} from ({w},{h}) to ({new_w},{TARGET_H})")
        return
    
    # h < TARGET_H: anchor at bottom
    new_im = Image.new("RGBA", (w, TARGET_H), (0, 0, 0, 0))
    pad_top = TARGET_H - h
    
    if is_sky:
        # Fill pad_top with the top row color of the sky
        top_row = im.crop((0, 0, w, 1))
        for y in range(pad_top):
            new_im.paste(top_row, (0, y))
    
    new_im.paste(im, (0, pad_top), mask=im)
    new_im.save(p)
    print(f"Extended {folder}/{filename} from ({w},{h}) to ({w},{TARGET_H}) (pad_top={pad_top})")

# Plains
extend_vertical('Theme4_Plains', 'plains_0_sky.png', is_sky=True)
extend_vertical('Theme4_Plains', 'plains_1_mountains.png', is_sky=False)
extend_vertical('Theme4_Plains', 'plains_2_hills.png', is_sky=False)
extend_vertical('Theme4_Plains', 'plains_3_grass.png', is_sky=False)

# Industrial
extend_vertical('Theme8_Industrial', 'industrial_0_sky.png', is_sky=True)
extend_vertical('Theme8_Industrial', 'industrial_1_far_buildings.png', is_sky=False)
extend_vertical('Theme8_Industrial', 'industrial_2_buildings.png', is_sky=False)
extend_vertical('Theme8_Industrial', 'industrial_3_foreground.png', is_sky=False)

# Tropical
extend_vertical('Theme5_Tropical', 'tropical_0_sky.png', is_sky=True)
extend_vertical('Theme5_Tropical', 'tropical_1_mountains.png', is_sky=False)
extend_vertical('Theme5_Tropical', 'tropical_2_hills.png', is_sky=False)
extend_vertical('Theme5_Tropical', 'tropical_3_palms.png', is_sky=False)

# Desert
extend_vertical('Theme6_Desert', 'desert_0_sky.png', is_sky=True)
extend_vertical('Theme6_Desert', 'desert_1_mountains.png', is_sky=False)
extend_vertical('Theme6_Desert', 'desert_2_dunes.png', is_sky=False)
extend_vertical('Theme6_Desert', 'desert_3_palms.png', is_sky=False)

# Underwater
extend_vertical('Theme7_Underwater', 'underwater_0_deep.png', is_sky=False)
extend_vertical('Theme7_Underwater', 'underwater_1_sunrays.png', is_sky=False)
extend_vertical('Theme7_Underwater', 'underwater_2_corals.png', is_sky=False)
extend_vertical('Theme7_Underwater', 'underwater_3_reef.png', is_sky=False)

# Cave
extend_vertical('Theme9_Cave', 'cave_0_back.png', is_sky=False)
extend_vertical('Theme9_Cave', 'cave_1_far.png', is_sky=False)
extend_vertical('Theme9_Cave', 'cave_2_mid.png', is_sky=False)
extend_vertical('Theme9_Cave', 'cave_3_front.png', is_sky=False)

print("All themes normalized to height 384 successfully.")
