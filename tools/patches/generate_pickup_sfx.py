import os, wave, numpy as np

audio_dir = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Audio"

SAMPLE_RATE = 44100

def save_wav(filename, samples):
    peak = np.max(np.abs(samples))
    if peak > 0:
        samples = samples / peak * 28000.0
    int_samples = np.int16(samples)
    path = os.path.join(audio_dir, filename)
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SAMPLE_RATE)
        w.writeframes(int_samples.tobytes())
    print(f"Saved {filename}: peak={np.max(np.abs(int_samples))}, size={os.path.getsize(path)}")

# 1. Pickup Shield: High crystal chime rising (C6 -> E6 -> G6 -> C7)
t1 = np.linspace(0, 0.45, int(SAMPLE_RATE * 0.45), endpoint=False)
f_t = np.linspace(1046.5, 2093.0, len(t1))
decay1 = np.exp(-6.0 * t1)
shield_pickup = np.sin(2 * np.pi * f_t * t1) * decay1 + 0.3 * np.sin(4 * np.pi * f_t * t1) * decay1
save_wav("sfx_pickup_shield.wav", shield_pickup)

# 2. Pickup Magnet: Magnetic zap / buzz rising (220Hz -> 880Hz)
t2 = np.linspace(0, 0.40, int(SAMPLE_RATE * 0.40), endpoint=False)
f_m = np.linspace(220.0, 880.0, len(t2))
decay2 = np.exp(-5.0 * t2)
mag_pickup = (np.sin(2 * np.pi * f_m * t2) + 0.4 * np.sin(4 * np.pi * f_m * t2)) * decay2
save_wav("sfx_pickup_magnet.wav", mag_pickup)

# 3. Pickup Time-Slow: Chrono warp sweep (1200Hz down to 400Hz then chime)
t3 = np.linspace(0, 0.50, int(SAMPLE_RATE * 0.50), endpoint=False)
f_ts = np.linspace(1400.0, 500.0, len(t3))
decay3 = np.exp(-4.5 * t3)
timeslow_pickup = np.sin(2 * np.pi * f_ts * t3) * decay3 + 0.35 * np.sin(2 * np.pi * 880.0 * t3) * np.exp(-8.0 * t3)
save_wav("sfx_pickup_timeslow.wav", timeslow_pickup)

# 4. Pickup Heart: 3-note ascending arpeggio (C5, E5, G5)
t4 = np.linspace(0, 0.50, int(SAMPLE_RATE * 0.50), endpoint=False)
heart_pickup = np.zeros_like(t4)
notes = [(523.25, 0.0, 0.16), (659.25, 0.12, 0.16), (783.99, 0.24, 0.26)]
for freq, start_s, dur_s in notes:
    idx_s = int(start_s * SAMPLE_RATE)
    idx_e = min(len(t4), idx_s + int(dur_s * SAMPLE_RATE))
    sub_t = t4[idx_s:idx_e] - start_s
    heart_pickup[idx_s:idx_e] += np.sin(2 * np.pi * freq * sub_t) * np.exp(-12.0 * sub_t)
save_wav("sfx_pickup_heart.wav", heart_pickup)
