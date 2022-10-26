using Eto.Drawing;

namespace InfoImp_Reversi.State;

public static class GameManager {
    /**
     * The player who is currently at set
     */
    public static PlayingPlayer PlayerAtSet { get; set; } = PlayingPlayer.Player1;

    /**
     * The color of player 1
     */
    public static Color Player1Color { get; set; } = Colors.Red;

    /**
     * The color of player 2
     */
    public static Color Player2Color { get; set; } = Colors.Aqua;

    /**
     * Whether help mode is enabled
     */
    public static bool IsHelpModeEnabled { get; set; } = true;

    /**
     * Whether the game is complete
     */
    public static bool IsGameComplete { get; set; }

    /**
     * Whether the player was able to make a move
     */
    public static bool PlayerCouldNotPlay { get; set; }

    /**
     * Swap the player at set
     */
    public static void SwapPlayingPlayer() {
        PlayerAtSet = PlayerAtSet switch {
            PlayingPlayer.Player1 => PlayingPlayer.Player2,
            PlayingPlayer.Player2 => PlayingPlayer.Player1,
            _ => throw new NotImplementedException("Case not implemented")
        };
    }

    /// <summary>
    /// Get the CellState of the player not currently playing
    /// </summary>
    /// <param name="playingPlayer">The player currently playing</param>
    /// <returns>The CellState</returns>
    /// <exception cref="NotImplementedException"></exception>
    public static CellState GetOtherPlayerCellState(PlayingPlayer playingPlayer) {
        switch (playingPlayer) {
            case PlayingPlayer.Player1:
                return CellState.Player2;
            case PlayingPlayer.Player2:
                return CellState.Player1;
            default:
                throw new NotImplementedException("Case not implemented");
        }
    }
}