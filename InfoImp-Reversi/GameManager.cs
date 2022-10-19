using Eto.Drawing;

namespace InfoImp_Reversi; 

public static class GameManager {
    public static PlayingPlayer PlayerAtSet { get; set; } = PlayingPlayer.Player1;
    public static Color PlayerAColor { get; set; } = Colors.Red;
    public static Color PlayerBColor { get; set; } = Colors.Aqua;

    public static bool IsHelpModeEnabled { get; set; } = true;

    public static bool IsGameComplete { get; set; } = false;
}