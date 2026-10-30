using UnityEngine;

/// <summary>
/// Central source of truth for UI typography tokens, scales, and accessibility font sizing.
/// Standardizes font sizes across all screens, preventing ad-hoc literals and container drift.
/// </summary>
public static class UiType
{
    // Global typography scale multiplier (supports in-game accessibility "Large Text" mode)
    public static float Scale = 1.0f;

    // Core Semantic Typography Tokens (Authored for 1080x1920 reference canvas)
    public const int Display = 44;    // Primary Hero Titles (e.g., "SLIME FOOD DROP", "GAME OVER")
    public const int H1      = 36;    // Section Headers & Modals (e.g., "SELECT DIFFICULTY", "ALMANAC")
    public const int H2      = 32;    // Sub-headers & Major Callouts (e.g., Tutorial Lesson Title, Final Score)
    public const int Body    = 28;    // Primary Dialogue, Lore, Mechanics & Instructions
    public const int Label   = 26;    // Button Labels, Key Badges, Stage Subtitles
    public const int Caption = 24;    // Compact Indicators, Secondary Card Badges, Prompts
    public const int Micro   = 22;    // Smallest allowable text (Speaker Name, Footers)

    /// <summary>
    /// Computes the active pixel font size for a token given the current global scale.
    /// Guaranteed never to drop below 20px on reference canvas.
    /// </summary>
    public static int Px(int token)
    {
        return Mathf.Max(20, Mathf.RoundToInt(token * Scale));
    }
}
