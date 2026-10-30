import os
import numpy as np
from PIL import Image, ImageDraw

out_dir = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Sprites\HomeSlimeEmotions"
os.makedirs(out_dir, exist_ok=True)

idle_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Sprites\Slime\01-Idle\FR_Slime4_Idle_000.png"
base_im = Image.open(idle_path).convert("RGBA")
W, H = base_im.size  # 758 x 466

# Surgically erase original eyes to get a clean canvas with perfect shading
arr = np.array(base_im).copy()
for y in range(210, 315):
    row_slice = arr[y, 234:432]
    mask = (row_slice[:, 2] < 185) | (row_slice[:, 3] > 206)
    row_slice[mask] = arr[255, 328]
    arr[y, 234:432] = row_slice
blank_slime = Image.fromarray(arr, "RGBA")


def tint_slime(im, tint_rgb, strength=0.35):
    a = np.array(im).astype(np.float32)
    alpha = a[:, :, 3]
    brightness = (a[:, :, 0] + a[:, :, 1] + a[:, :, 2]) / 3.0
    is_body = (alpha > 40) & (brightness > 40) & (brightness < 215)
    for c_idx, val in enumerate(tint_rgb):
        ch = a[:, :, c_idx]
        ch[is_body] = ch[is_body] * (1.0 - strength) + val * strength
        a[:, :, c_idx] = np.clip(ch, 0, 255)
    return Image.fromarray(a.astype(np.uint8), "RGBA")


def composite_layer(base_img, draw_fn):
    overlay = Image.new("RGBA", base_img.size, (0, 0, 0, 0))
    d = ImageDraw.Draw(overlay)
    draw_fn(d)
    return Image.alpha_composite(base_img, overlay)


def draw_pixar_eye(draw, cx, cy, rx, ry, pupil_dx=0, pupil_dy=0, pupil_scale=0.64, angry=False, scared=False):
    draw.ellipse([cx - rx - 5, cy - ry - 5, cx + rx + 5, cy + ry + 5], fill=(10, 14, 22, 255))
    draw.ellipse([cx - rx, cy - ry, cx + rx, cy + ry], fill=(248, 252, 255, 255))
    prx = max(7, int(rx * pupil_scale))
    pry = max(7, int(ry * pupil_scale))
    px = cx + pupil_dx
    py = cy + pupil_dy
    iris_col = (190, 40, 40, 255) if angry else ((108, 36, 152, 255) if scared else (24, 88, 168, 255))
    draw.ellipse([px - prx - 4, py - pry - 4, px + prx + 4, py + pry + 4], fill=iris_col)
    draw.ellipse([px - prx, py - pry, px + prx, py + pry], fill=(8, 10, 16, 255))
    hl1 = max(5, int(prx * 0.36))
    draw.ellipse([px - prx // 2 - hl1, py - pry // 2 - hl1, px - prx // 2 + hl1, py - pry // 2 + hl1], fill=(255, 255, 255, 255))
    hl2 = max(3, int(prx * 0.20))
    draw.ellipse([px + prx // 3 - hl2, py + pry // 3 - hl2, px + prx // 3 + hl2, py + pry // 3 + hl2], fill=(255, 255, 255, 245))


# 1. IDLE: Cute big front-facing eyes + happy little smile
im_idle = blank_slime.copy()
def d_idle(d):
    draw_pixar_eye(d, 326, 254, 30, 38, 0, 0, 0.64)
    draw_pixar_eye(d, 434, 254, 30, 38, 0, 0, 0.64)
    # Warm smile
    d.arc([358, 296, 402, 332], start=10, end=170, fill=(14, 16, 22, 255), width=7)
    # Pink cheeks
    d.ellipse([272, 282, 312, 306], fill=(245, 120, 160, 120))
    d.ellipse([448, 282, 488, 306], fill=(245, 120, 160, 120))
im_idle = composite_layer(im_idle, d_idle)
im_idle.save(os.path.join(out_dir, "slime_emotion_idle.png"))

# 2. HAPPY: Closed-eye beaming smile (^w^) with pink cheeks
im_happy = blank_slime.copy()
def d_happy(d):
    # Closed upside-down curved eyes (^ ^)
    d.arc([292, 230, 356, 282], start=190, end=350, fill=(12, 16, 24, 255), width=10)
    d.arc([404, 230, 468, 282], start=190, end=350, fill=(12, 16, 24, 255), width=10)
    # Big joyful open grin
    d.chord([348, 280, 412, 340], start=0, end=180, fill=(24, 12, 26, 255), outline=(10, 12, 18, 255), width=6)
    d.chord([356, 302, 404, 338], start=0, end=180, fill=(246, 108, 142, 255))
    # Rosy cheeks
    d.ellipse([260, 274, 314, 312], fill=(255, 110, 155, 170))
    d.ellipse([446, 274, 500, 312], fill=(255, 110, 155, 170))
im_happy = composite_layer(im_happy, d_happy)
im_happy.save(os.path.join(out_dir, "slime_emotion_happy.png"))

# 3. SURPRISED: Wide O.O eyes, raised brows, open "o" mouth
im_surprised = blank_slime.copy()
def d_surprised(d):
    draw_pixar_eye(d, 324, 246, 35, 42, 0, 0, 0.58)
    draw_pixar_eye(d, 436, 246, 35, 42, 0, 0, 0.58)
    d.arc([286, 186, 360, 222], start=190, end=350, fill=(10, 14, 22, 255), width=8)
    d.arc([400, 186, 474, 222], start=190, end=350, fill=(10, 14, 22, 255), width=8)
    d.ellipse([364, 298, 396, 334], fill=(18, 12, 22, 255), outline=(10, 10, 16, 255), width=5)
im_surprised = composite_layer(im_surprised, d_surprised)
im_surprised.save(os.path.join(out_dir, "slime_emotion_surprised.png"))

# 4. ANGRY: Angled brows (>:[), red flush, angry mouth, red irises
im_angry = tint_slime(blank_slime, (225, 75, 65), strength=0.35)
def d_angry(d):
    # Angled angry brows
    d.line([(286, 206), (362, 238)], fill=(18, 12, 14, 255), width=10)
    d.line([(474, 206), (398, 238)], fill=(18, 12, 14, 255), width=10)
    # Angry eyes
    draw_pixar_eye(d, 328, 260, 28, 34, 0, 2, 0.55, angry=True)
    draw_pixar_eye(d, 432, 260, 28, 34, 0, 2, 0.55, angry=True)
    # Angry jagged / frowning mouth
    d.arc([352, 310, 408, 346], start=190, end=350, fill=(20, 10, 14, 255), width=9)
    # Steam puffs on sides
    d.ellipse([240, 250, 286, 286], fill=(255, 80, 80, 150))
    d.ellipse([474, 250, 520, 286], fill=(255, 80, 80, 150))
im_angry = composite_layer(im_angry, d_angry)
im_angry.save(os.path.join(out_dir, "slime_emotion_angry.png"))

# 5. DIZZY: Swirl eyes (@ @ or X X), queasy mouth, wobbly
im_dizzy = tint_slime(blank_slime, (170, 130, 225), strength=0.30)
def d_dizzy(d):
    # X_X eyes
    for cx in [328, 432]:
        cy = 254
        d.line([(cx - 24, cy - 24), (cx + 24, cy + 24)], fill=(16, 12, 24, 255), width=9)
        d.line([(cx - 24, cy + 24), (cx + 24, cy - 24)], fill=(16, 12, 24, 255), width=9)
    # Wobbly squiggly mouth
    d.arc([342, 308, 378, 334], start=180, end=360, fill=(16, 14, 22, 255), width=7)
    d.arc([374, 308, 410, 334], start=0, end=180, fill=(16, 14, 22, 255), width=7)
    d.arc([406, 308, 422, 334], start=180, end=360, fill=(16, 14, 22, 255), width=7)
im_dizzy = composite_layer(im_dizzy, d_dizzy)
im_dizzy.save(os.path.join(out_dir, "slime_emotion_dizzy.png"))

# 6. PEEK / SUSPICIOUS: Side-glance eyes looking right at the player (squinting, looking around)
im_peek = blank_slime.copy()
def d_peek(d):
    # Half-lidded / suspicious squint eyes
    draw_pixar_eye(d, 326, 254, 30, 22, pupil_dx=-10, pupil_dy=-2, pupil_scale=0.68)
    draw_pixar_eye(d, 434, 254, 30, 22, pupil_dx=-10, pupil_dy=-2, pupil_scale=0.68)
    # Lids
    d.line([(290, 242), (362, 242)], fill=(12, 14, 20, 255), width=8)
    d.line([(398, 242), (470, 242)], fill=(12, 14, 20, 255), width=8)
    # Skeptical wry mouth
    d.line([(366, 312), (406, 316)], fill=(14, 16, 22, 255), width=7)
im_peek = composite_layer(im_peek, d_peek)
im_peek.save(os.path.join(out_dir, "slime_emotion_peek.png"))

# 7. ANNOYED / HUFFY: Puffed cheeks, arms-crossed hmph expression
im_annoyed = tint_slime(blank_slime, (220, 110, 80), strength=0.25)
def d_annoyed(d):
    # Closed annoyed tight-eyes (> <)
    d.line([(302, 240), (342, 256)], fill=(14, 16, 22, 255), width=8)
    d.line([(302, 272), (342, 256)], fill=(14, 16, 22, 255), width=8)
    d.line([(458, 240), (418, 256)], fill=(14, 16, 22, 255), width=8)
    d.line([(458, 272), (418, 256)], fill=(14, 16, 22, 255), width=8)
    # Puffed cheek circles
    d.ellipse([244, 260, 314, 320], fill=(235, 120, 100, 210), outline=(12, 14, 18, 255), width=6)
    d.ellipse([446, 260, 516, 320], fill=(235, 120, 100, 210), outline=(12, 14, 18, 255), width=6)
    # Downward pouting mouth
    d.arc([360, 310, 400, 338], start=200, end=340, fill=(12, 14, 20, 255), width=8)
im_annoyed = composite_layer(im_annoyed, d_annoyed)
im_annoyed.save(os.path.join(out_dir, "slime_emotion_annoyed.png"))

print("Successfully generated all 7 Home Slime Emotion Sprites!")
