import os, wave, numpy as np

audio_dir = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Audio"
os.makedirs(audio_dir, exist_ok=True)

SAMPLE_RATE = 44100
DURATION = 2.0
num_samples = int(SAMPLE_RATE * DURATION)
t = np.linspace(0, DURATION, num_samples, endpoint=False)

def save_wav(filename, samples):
    # Normalize to peak amplitude of 28000
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

# 1. Shield Active Shimmer Loop (Resonant Crystalline Barrier Forcefield)
# Ensure exact integer periods so the loop is 100% seamless
f_base = 330.0 # 330 * 2 = 660 cycles in 2 seconds
f_sub = 165.0
f_hi = 660.0
f_hi2 = 990.0
lfo = 3.0 # 3 * 2 = 6 cycles in 2s
shimmer = np.sin(2 * np.pi * lfo * t)
shield_wave = (
    0.40 * np.sin(2 * np.pi * f_base * t + 0.35 * shimmer) +
    0.25 * np.sin(2 * np.pi * f_sub * t) +
    0.20 * np.sin(2 * np.pi * f_hi * t) +
    0.15 * np.sin(2 * np.pi * f_hi2 * t)
)
save_wav("sfx_powerup_shield_active.wav", shield_wave)

# 2. Magnet Active Whir Loop (Vibrant Electromagnetic Tractor-Beam Pulse)
# 4 Hz pulse = 8 cycles in 2s
pulse = 0.5 + 0.5 * np.sin(2 * np.pi * 4.0 * t - np.pi/2)
pulse_power = pulse ** 1.5
f_mag = 220.0 # 440 cycles in 2s
f_mag2 = 440.0
f_mag3 = 660.0
buzz = (
    0.50 * np.sin(2 * np.pi * f_mag * t) +
    0.30 * np.sin(2 * np.pi * f_mag2 * t) +
    0.20 * np.sin(2 * np.pi * f_mag3 * t)
)
# Add subtle electric pitch wobble
magnet_wave = buzz * (0.35 + 0.65 * pulse_power)
save_wav("sfx_powerup_magnet_active.wav", magnet_wave)

# 3. Time-Slow Active Ticking Loop (Crisp Mechanical Clock Ticks)
timeslow_wave = np.zeros(num_samples, dtype=np.float64)
# 4 ticks per 2 seconds (0.0s, 0.5s, 1.0s, 1.5s)
tick_positions = [0.0, 0.5, 1.0, 1.5]
for i, pos in enumerate(tick_positions):
    start_idx = int(pos * SAMPLE_RATE)
    # Tick duration 0.08s
    tick_len = int(0.08 * SAMPLE_RATE)
    tt = np.linspace(0, 0.08, tick_len, endpoint=False)
    freq = 1400.0 if i % 2 == 0 else 1150.0 # High tick, low tock
    decay = np.exp(-45.0 * tt)
    tick_sample = np.sin(2 * np.pi * freq * tt) * decay
    # Add small woodblock resonance
    tick_sample += 0.4 * np.sin(2 * np.pi * (freq * 0.5) * tt) * np.exp(-30.0 * tt)
    timeslow_wave[start_idx:start_idx + tick_len] += tick_sample

save_wav("sfx_powerup_timeslow_active.wav", timeslow_wave)
