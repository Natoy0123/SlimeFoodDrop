import os, wave, numpy as np

audio_dir = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Audio"
os.makedirs(audio_dir, exist_ok=True)

SAMPLE_RATE = 44100
DURATION = 2.0
num_samples = int(SAMPLE_RATE * DURATION)
t = np.linspace(0, DURATION, num_samples, endpoint=False)

def save_wav(filename, samples, peak_target=9000.0):
    peak = np.max(np.abs(samples))
    if peak > 0:
        samples = samples / peak * peak_target
    int_samples = np.int16(samples)
    path = os.path.join(audio_dir, filename)
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SAMPLE_RATE)
        w.writeframes(int_samples.tobytes())
    print(f"Saved {filename}: peak={np.max(np.abs(int_samples))}, size={os.path.getsize(path)}")

# 1. Gentle Shield Forcefield Pad (Warm Celestial Major Chord, Soft Ambient Hum)
# Seamless integer periods over 2 seconds:
# A3 = 220Hz (440 cyc), C#4 = 275Hz (550 cyc), E4 = 330Hz (660 cyc), A4 = 440Hz (880 cyc)
# LFO = 1.0Hz (2 cycles in 2s) for gentle breathing swell
lfo_swell = 0.85 + 0.15 * np.sin(2 * np.pi * 1.0 * t - np.pi/2)
shield_wave = (
    0.45 * np.sin(2 * np.pi * 220.0 * t) +
    0.30 * np.sin(2 * np.pi * 275.0 * t) +
    0.20 * np.sin(2 * np.pi * 330.0 * t) +
    0.10 * np.sin(2 * np.pi * 440.0 * t)
) * lfo_swell
save_wav("sfx_powerup_shield_active.wav", shield_wave, peak_target=8500.0)

# 2. Friendly Star Magnet Chime (Warm, Playful Music-Box Glockenspiel Twinkle)
# Soft chime notes at t = 0.0s, 0.4s, 0.8s, 1.2s, 1.6s
# Pentatonic frequencies: C5(523.25), E5(659.25), G5(783.99), A5(880.0), C6(1046.5)
# Soft sine decay per note
magnet_wave = np.zeros(num_samples, dtype=np.float64)
notes = [
    (0.0, 523.25), # C5
    (0.4, 659.25), # E5
    (0.8, 783.99), # G5
    (1.2, 880.00), # A5
    (1.6, 1046.50),# C6
]
for start_t, freq in notes:
    idx_start = int(start_t * SAMPLE_RATE)
    note_dur = 0.38
    idx_len = int(note_dur * SAMPLE_RATE)
    nt = np.linspace(0, note_dur, idx_len, endpoint=False)
    # Warm bell envelope with soft attack and exponential decay
    attack = np.minimum(1.0, nt / 0.015)
    decay = np.exp(-10.0 * nt)
    envelope = attack * decay
    # Fundamental + warm soft octave harmonic
    tone = np.sin(2 * np.pi * freq * nt) + 0.35 * np.sin(2 * np.pi * (freq * 2.0) * nt)
    magnet_wave[idx_start:idx_start + idx_len] += tone * envelope

save_wav("sfx_powerup_magnet_active.wav", magnet_wave, peak_target=9200.0)
print("Friendly powerup audio generated successfully!")
