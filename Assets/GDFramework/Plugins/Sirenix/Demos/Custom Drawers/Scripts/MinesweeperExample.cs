#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Demos
{
    using System;
    using UnityEngine;
    using Utilities;

#if UNITY_EDITOR
    using Sirenix.Utilities.Editor;
    using Editor;
    using UnityEditor;

#endif

    public class MinesweeperExample : MonoBehaviour
    {
        [Minesweeper] public int NumberOfBombs;
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class MinesweeperAttribute : Attribute
    {
    }

#if UNITY_EDITOR

    /// <summary>
    /// Minesweeper.
    /// </summary>
    public sealed class MinesweeperAttributeDrawer : OdinAttributeDrawer<MinesweeperAttribute, int>
    {
        private enum Tile
        {
            Empty = 0, // Empty tile.

            // 1-8.

            Open = 9,
            Bomb = 10,
            Flag = 11
        }

        private readonly Color[] NumberColors = new Color[8]
        {
            new Color32(42, 135, 238, 255), // 1
            new Color32(57, 233, 48, 255), // 2
            new Color32(253, 0, 0, 255), // 3
            new Color32(31, 23, 173, 255), // 4
            new Color32(36, 30, 155, 255), // 5
            new Color32(131, 29, 29, 255), // 6
            new Color32(40, 40, 40, 255), // 7
            new Color32(132, 132, 132, 255) // 8
        };

        private const float TileSize = 20;
        private const int BoardSize = 25;

        private bool isRunning;
        private bool gameOver;
        private int flaggedBombs;
        private int numberOfBombs;
        private Tile[,] visibleTiles;
        private Tile[,] tiles;
        private double time;
        private double prevTime;

        protected override void Initialize()
        {
            isRunning = false;
            visibleTiles = new Tile[BoardSize, BoardSize];
            tiles = new Tile[BoardSize, BoardSize];
        }

        private void StartGame(int bombs)
        {
            numberOfBombs = bombs;

            for (var x = 0; x < BoardSize; x++)
            for (var y = 0; y < BoardSize; y++)
            {
                visibleTiles[x, y] = Tile.Empty;
                tiles[x, y] = Tile.Empty;
            }

            // Spawn bombs.
            for (var count = 0; count < numberOfBombs;)
            {
                var x = UnityEngine.Random.Range(0, BoardSize);
                var y = UnityEngine.Random.Range(0, BoardSize);

                if (tiles[x, y] != Tile.Bomb)
                {
                    tiles[x, y] = Tile.Bomb;

                    if (x + 1 < BoardSize && tiles[x + 1, y] != Tile.Bomb)
                        tiles[x + 1, y] = (Tile)((int)tiles[x + 1, y] + 1);
                    if (x + 1 < BoardSize && y + 1 < BoardSize && tiles[x + 1, y + 1] != Tile.Bomb)
                        tiles[x + 1, y + 1] = (Tile)((int)tiles[x + 1, y + 1] + 1);
                    if (y + 1 < BoardSize && tiles[x, y + 1] != Tile.Bomb)
                        tiles[x, y + 1] = (Tile)((int)tiles[x, y + 1] + 1);
                    if (x - 1 >= 0 && y + 1 < BoardSize && tiles[x - 1, y + 1] != Tile.Bomb)
                        tiles[x - 1, y + 1] = (Tile)((int)tiles[x - 1, y + 1] + 1);

                    if (x - 1 >= 0 && tiles[x - 1, y] != Tile.Bomb) tiles[x - 1, y] = (Tile)((int)tiles[x - 1, y] + 1);
                    if (x - 1 >= 0 && y - 1 >= 0 && tiles[x - 1, y - 1] != Tile.Bomb)
                        tiles[x - 1, y - 1] = (Tile)((int)tiles[x - 1, y - 1] + 1);
                    if (y - 1 >= 0 && tiles[x, y - 1] != Tile.Bomb) tiles[x, y - 1] = (Tile)((int)tiles[x, y - 1] + 1);
                    if (x + 1 < BoardSize && y - 1 >= 0 && tiles[x + 1, y - 1] != Tile.Bomb)
                        tiles[x + 1, y - 1] = (Tile)((int)tiles[x + 1, y - 1] + 1);

                    count++;
                }
            }

            gameOver = false;
            isRunning = true;
            flaggedBombs = 0;
            prevTime = EditorApplication.timeSinceStartup;
            time = 0.0;
        }

        /// <summary>
        /// Handles the Minesweeper game.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var rect = EditorGUILayout.GetControlRect();
            ValueEntry.SmartValue =
                Mathf.Clamp(
                    SirenixEditorFields.IntField(rect.AlignLeft(rect.width - 80 - 4), "Number of Bombs",
                        ValueEntry.SmartValue), 1, BoardSize * BoardSize / 4);

            // Start game
            if (GUI.Button(rect.AlignRight(80), "Start")) StartGame(ValueEntry.SmartValue);

            // Game
            SirenixEditorGUI.BeginShakeableGroup(3f);
            if (isRunning) Game();
            SirenixEditorGUI.EndShakeableGroup();
        }

        private void Game()
        {
            var rect = EditorGUILayout.GetControlRect(true, TileSize * BoardSize + 20);
            rect = rect.AlignCenter(TileSize * BoardSize);

            // Toolbar
            {
                SirenixEditorGUI.DrawSolidRect(rect.AlignTop(20), new Color(0.5f, 0.5f, 0.5f, 1f));
                SirenixEditorGUI.DrawBorders(rect.AlignTop(20).SetHeight(21).SetWidth(rect.width + 1), 1);

                if (Event.current.type == EventType.Repaint && !gameOver)
                {
                    var t = EditorApplication.timeSinceStartup;
                    this.time += t - prevTime;
                    prevTime = t;
                }

                var time = GUIHelper.TempContent(((int)this.time).ToString());
                GUIHelper.PushContentColor(Color.black);
                GUI.Label(
                    rect.AlignTop(20).HorizontalPadding(4).AlignMiddle(18)
                        .AlignRight(EditorStyles.label.CalcSize(time).x), time);
                GUIHelper.PopContentColor();

                GUIHelper.PushColor(Color.yellow);
                GUI.Label(rect.AlignTop(20).AlignCenter(20), EditorIcons.PacmanGhost.Raw);
                GUIHelper.PopColor();

                if (gameOver)
                {
                    GUIHelper.PushContentColor(flaggedBombs == numberOfBombs ? Color.green : Color.red);
                    GUI.Label(rect.AlignTop(20).HorizontalPadding(4).AlignMiddle(18),
                        flaggedBombs == numberOfBombs ? "You win!" : "Game over!");
                    GUIHelper.PopContentColor();
                }
            }

            rect = rect.AlignBottom(rect.height - 20);
            SirenixEditorGUI.DrawSolidRect(rect, new Color(0.7f, 0.7f, 0.7f, 1f));

            for (var i = 0; i < BoardSize * BoardSize; i++)
            {
                var tileRect = rect.SplitGrid(TileSize, TileSize, i);
                SirenixEditorGUI.DrawBorders(tileRect.SetWidth(tileRect.width + 1).SetHeight(tileRect.height + 1), 1);

                var x = i % BoardSize;
                var y = i / BoardSize;
                var tile = tiles[x, y];
                var visible = visibleTiles[x, y];

                if (gameOver || visible == Tile.Open)
                    SirenixEditorGUI.DrawSolidRect(
                        new Rect(tileRect.x + 1, tileRect.y + 1, tileRect.width - 1, tileRect.height - 1),
                        new Color(0.3f, 0.3f, 0.3f, 1f));

                if ((gameOver || visible == Tile.Open) && tile == Tile.Bomb)
                {
                    GUIHelper.PushColor(visible == Tile.Flag ? Color.black : Color.white);
                    GUI.Label(tileRect.AlignCenter(18).AlignMiddle(18), EditorIcons.SettingsCog.ActiveGUIContent);
                    GUIHelper.PopColor();
                }

                if (visible == Tile.Flag)
                {
                    GUIHelper.PushColor(Color.red);
                    GUI.Label(tileRect.AlignCenter(18).AlignMiddle(18), EditorIcons.Flag.ActiveGUIContent);
                    GUIHelper.PopColor();
                }

                if ((gameOver || visible == Tile.Open) && (int)tile >= 1 && (int)tile <= 8)
                {
                    GUIHelper.PushColor(NumberColors[(int)tile - 1]);
                    GUI.Label(tileRect.AlignCenter(18).AlignCenter(18).AddX(2).AddY(2), ((int)tile).ToString(),
                        EditorStyles.boldLabel);
                    GUIHelper.PopColor();
                }

                if (!gameOver && tileRect.Contains(Event.current.mousePosition))
                {
                    SirenixEditorGUI.DrawSolidRect(
                        new Rect(tileRect.x + 1, tileRect.y + 1, tileRect.width - 1, tileRect.height - 1),
                        new Color(0f, 1f, 0f, 0.3f));

                    // Input
                    // Reveal
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        if (visible != Tile.Flag)
                        {
                            if (tile == Tile.Bomb)
                            {
                                // LOSE
                                gameOver = true;
                                SirenixEditorGUI.StartShakingGroup();
                            }
                            else
                            {
                                Reveal(x, y);
                            }
                        }

                        Event.current.Use();
                    }
                    // Place flag
                    else if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
                    {
                        if (visible == Tile.Empty)
                        {
                            visibleTiles[x, y] = Tile.Flag;

                            if (tile == Tile.Bomb)
                            {
                                flaggedBombs++;

                                if (flaggedBombs == numberOfBombs) gameOver = true;
                            }
                        }
                        else if (visible == Tile.Flag)
                        {
                            visibleTiles[x, y] = Tile.Empty;

                            if (tile == Tile.Bomb) flaggedBombs--;
                        }

                        Event.current.Use();
                    }
                }
            }

            GUIHelper.RequestRepaint();
        }

        private void Reveal(int x, int y)
        {
            if (x < 0 || x >= BoardSize || y < 0 || y >= BoardSize) return;

            if (visibleTiles[x, y] == Tile.Open) return;

            if (tiles[x, y] == Tile.Bomb) return;

            if ((int)tiles[x, y] <= 8)
            {
                visibleTiles[x, y] = Tile.Open;

                if (tiles[x, y] != Tile.Empty) return;
            }

            // Recursive reveal.
            Reveal(x + 1, y);
            Reveal(x + 1, y + 1);
            Reveal(x, y + 1);
            Reveal(x - 1, y + 1);

            Reveal(x - 1, y);
            Reveal(x - 1, y - 1);
            Reveal(x, y - 1);
            Reveal(x + 1, y - 1);
        }
    }

#endif
}
#endif