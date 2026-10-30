from PIL import Image, ImageDraw, ImageFilter
import math

# 1024 x 200 Stage Floor Platform
# Rows 0-120: Top Polished Wooden Floor Planks (Rich mahogany/warm oak)
# Rows 120-128: Polished Brass / Golden Trim Lip
# Rows 128-200: Vertical Wooden Fascia / Apron with soft drop shadow

width = 1024
height = 200
img = Image.new("RGBA", (width, height), (0, 0, 0, 0))
draw = ImageDraw.Draw(img)

# Floor plank surface (rows 0 to 120)
for y in range(0, 120):
    t = y / 120.0
    # Gradient from darker rear to warmer front
    r = int(52 + t * 38)
    g = int(32 + t * 26)
    b = int(22 + t * 16)
    draw.line([(0, y), (width, y)], fill=(r, g, b, 255))

# Subtle horizontal plank seams every 24px
for seam_y in [24, 48, 72, 96]:
    draw.line([(0, seam_y), (width, seam_y)], fill=(32, 18, 12, 180))
    draw.line([(0, seam_y + 1), (width, seam_y + 1)], fill=(95, 65, 45, 90))

# Vertical staggered plank grooves
plank_xs = [0, 180, 360, 540, 720, 900, 1024]
for px in plank_xs:
    draw.line([(px, 0), (px, 48)], fill=(30, 16, 10, 150))
plank_xs_2 = [90, 270, 450, 630, 810, 990]
for px in plank_xs_2:
    draw.line([(px, 48), (px, 96)], fill=(30, 16, 10, 150))
for px in plank_xs:
    draw.line([(px, 96), (px, 120)], fill=(30, 16, 10, 150))

# Golden brass trim edge (rows 120 to 128)
brass_colors = [
    (185, 140, 45, 255),
    (245, 205, 95, 255),  # Specular highlight
    (255, 225, 125, 255), # Bright highlight
    (210, 165, 60, 255),
    (150, 105, 30, 255),
    (95, 60, 18, 255),
    (60, 35, 12, 255),
    (40, 22, 8, 255)
]
for i, c in enumerate(brass_colors):
    y = 120 + i
    draw.line([(0, y), (width, y)], fill=c)

# Front apron / fascia (rows 128 to 200)
for y in range(128, height):
    t = (y - 128) / (height - 128)
    r = int(36 - t * 14)
    g = int(22 - t * 9)
    b = int(15 - t * 6)
    draw.line([(0, y), (width, y)], fill=(r, g, b, 255))

# Vertical fluting / panels on front fascia
for fx in range(64, width, 64):
    draw.line([(fx, 128), (fx, height)], fill=(18, 10, 6, 200))
    draw.line([(fx + 1, 128), (fx + 1, height)], fill=(55, 34, 22, 100))

# Bevel corner clipping (theater stage rounded front corners)
# Save out
out_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Sprites\SplashCutscene\stage_floor_planks.png"
img.save(out_path, "PNG")
print("Saved stage_floor_planks.png successfully!")
