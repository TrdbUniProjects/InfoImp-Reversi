namespace InfoImp_Reversi.State; 

/**
 * The state of a cell on the board
 */
public enum CellState {
    /// <summary>
    /// The cell is Empty
    /// </summary>
    // Using = 0 to ensure it is the default value
    None = 0,
    /// <summary>
    /// The cell is owned by Player 1
    /// </summary>
    Player1,
    /// <summary>
    /// The cell is owned by Player 2
    /// </summary>
    Player2,
}