# Slime Food Drop 🍮🎮

A fast-paced, arcade-style 2D mobile game built with **Unity 6 (6000.3.0f1)** for Android portrait mode. Players control **Bloop the Slime**, catching falling treats, dodging hazards, building combo multipliers, and clearing themed stages.

---

## 🌟 Key Features

- **Dynamic Slime Mechanics:** Responsive touch/drag steering, physics-based squash & stretch, dynamic contact shadows, and expressive mouth eating animations.
- **Stage Progression & Themes:** 10 handcrafted stages with distinct color palettes, food drop patterns, and increasing difficulty curves.
- **Juicy Arcade UI:**
  - Beveled arcade-style buttons with spring-bounce micro-interactions ([`UIButtonJuice.cs`](Assets/Scripts/UIButtonJuice.cs)).
  - Floating score popups, dynamic combo counters, and high-score tracking.
  - Multi-tier tutorial overlay for onboarding new players.
- **Pixar-Style Splash Intro:** Engaging opening sequence featuring spotlight malfunction animations and expressive character emotions.
- **High-Refresh Display Optimization:** Optimized for fluid 120 FPS gameplay with low input latency on modern high-refresh mobile displays.
- **Responsive Camera & UI Framing:** Built with [`ResponsiveCamera.cs`](Assets/Scripts/ResponsiveCamera.cs) to adapt cleanly across diverse aspect ratios (16:9, 19.5:9, 20:9, and tablets).

---

## 🛠️ Architecture & Core Scripts

```text
Assets/
├── Editor/
│   └── BuildMainGameScene.cs      # Procedural scene builder & automated geometry layout
├── Fonts/
│   ├── GameBody-Rounded.ttf       # Readable rounded UI typography
│   └── PressStart2P-Regular.ttf   # Retro arcade pixel font
├── Scenes/
│   └── MainGameScene.unity        # Main game scene
├── Scripts/
│   ├── SlimeController.cs         # Movement physics, mouth alignment, and eating states
│   ├── ItemSpawner.cs             # Food & obstacle drop sequencing and difficulty curves
│   ├── FallingItem.cs             # Item collision, points, and floor bounce behavior
│   ├── GameManager.cs             # Game states, combo system, and PlayerPrefs persistence
│   ├── UIManager.cs               # HUD overlays, guidebook, high score, and pause menus
│   ├── UIButtonJuice.cs           # Spring scale bounce, haptics, and click juice
│   ├── PixarSplashCinematic.cs    # 4-phase intro cinematic with spotlight malfunction
│   ├── ThemeManager.cs            # Dynamic stage themes and environmental backdrops
│   ├── SoundManager.cs            # SFX triggers and arcade audio feedback
│   └── ResponsiveCamera.cs        # Multi-resolution orthographic viewport scaling
└── Sprites/
    ├── Slime/                     # Bloop emotions, mouths, and squash-stretch frames
    ├── Food/                      # Falling food items and hazard sprites
    └── UI/                        # Custom beveled arcade frames, chips, and wells
```

---

## 📱 Build Specifications

- **Engine:** Unity 6000.3.0f1
- **Platform:** Android (Portrait)
- **Scripting Backend:** IL2CPP
- **Architecture:** ARM64
- **Target Frame Rate:** 60 FPS / 120 FPS High-Refresh

---

## 🚀 Getting Started

1. Clone this repository:
   ```bash
   git clone https://github.com/Natoy0123/SlimeFoodDrop.git
   ```
2. Open **Unity Hub** and click **Add** > **Add project from disk**.
3. Select the `SlimeFoodDrop` directory.
4. Open the project using **Unity 6000.3.0f1** (or compatible Unity 6 release).
5. Load `Assets/Scenes/MainGameScene.unity` and press **Play** in the editor.

---

## 👤 Author

**Patrick Froilan S. Del Ayre**
- GitHub: [@Natoy0123](https://github.com/Natoy0123)
- Role: Lead Developer & Designer
