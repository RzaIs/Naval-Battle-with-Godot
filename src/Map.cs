using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Map : Node2D
{
    public readonly int size = 10;

    public MainScene Main;

    public TileMap seaMap;
    public TileMap editMap;
    public TileMap navalMap;
    public TileMap highLightMap;

    public bool editMode;
    public bool navalTaken;
    public bool pointChosen;
    public bool isActive;

    public LinkedList<Vector2> validDirections;
    public Vector2 pathOrigin;

    public Naval[] navals;
    public Naval takenNaval;

    public int takenNavalIndex;

    public override void _Ready()
    {
        Main = GetParent<MainScene>();
        seaMap = GetNode<TileMap>("SeaMap");
        editMap = GetNode<TileMap>("EditMap");
        navalMap = GetNode<TileMap>("NavalMap");
        highLightMap = GetNode<TileMap>("HighLightMap");
        Init();
    }

    public void Init()
    {
        editMode = true;
        navalTaken = false;
        pointChosen = false;
        isActive = false;

        validDirections = new LinkedList<Vector2>();

        SpawnNavals();

        foreach (Naval naval in navals)
        {
            AddNaval(naval);
        }
        UpdateBitmask(navalMap);
    }

    public override void _Process(float delta)
    {
        if(isActive)
            IfClickDone();
    }

    public bool IsReady()
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size + 8; j++)
            {
                if (!IsInSea(i, j) && navalMap.GetCell(i, j) != -1)
                {
                    ShowUnreadyNavals();
                    return false;
                }
            }
        }
        return true;
    }

    public void ClearAllTileMaps()
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                seaMap.SetCell(i, j, -1);
                editMap.SetCell(i, j, -1);
                navalMap.SetCell(i, j, -1);
                highLightMap.SetCell(i, j, -1);
            }
        }
    }

    public bool IsLostGame()
    {
        foreach (Naval naval in navals)
        {
            if(!IsSink(naval))
            {
                return false;
            }
        }
        return true;
    }

    public void ShowUnreadyNavals()
    {
        foreach (Naval naval in navals)
        {
            foreach (Cell cell in naval.Cells)
            {
                if(!IsInSea(cell))
                {
                    ShowNavalWarning(naval);
                    break;
                }
            }
        }
    }

    public void UnshowUnreadyNavals()
    {
        for (int i = -1; i <= size; i++)
        {
            for (int j = -1; j <= size + 8; j++)
            {
                if (highLightMap.GetCell(i, j) == 1)
                    highLightMap.SetCell(i, j, -1);
            }
        }
    }

    public void ShowNavalWarning(Naval naval)
    {
        foreach (Cell cell in naval.Cells)
        {
            highLightMap.SetCell(cell.X, cell.Y, 1);
        }
    }


    public void HideNavals()
    {
        navalMap.Visible = false;
    }

    public void ShowNavals()
    {
        navalMap.Visible = true;
    }

    public void IfClickDone()
    {
        if (Input.IsActionJustPressed("Click"))
        {
            Vector2 mouseGrid = ToGrid(GetLocalMousePosition());

            if (editMode)
                EditClick(mouseGrid);
            else
                GameClick(mouseGrid);

            UpdateBitmask(navalMap);
            UpdateBitmask(seaMap);
        }
    }

    public void GameClick(Vector2 mouseGrid)
    {
        int x = (int)mouseGrid.x;
        int y = (int)mouseGrid.y;

        if(IsInSea(mouseGrid) && IsCellHittable(x, y))
        {
            HitCell(x, y);
            if (navalMap.GetCell(x, y) == -1)
                Main.NextTurn();
            else
                Main.UpdateMap();
        }
    }

    public void EditClick(Vector2 mouseGrid)
    {
        if (navalTaken)
        {
            if (IsInSea(mouseGrid.x, mouseGrid.y))
            {
                if (!TakeNaval(mouseGrid) && !pointChosen)
                {
                    if (takenNaval.Cells.Length != 1)
                        ShowValidPathsIfAny(mouseGrid);
                    else if (IsCellEmpty(mouseGrid))
                        DirectMoveNaval(mouseGrid);
                }
                else if (pointChosen && validDirections.Contains(mouseGrid))
                {
                    MoveNaval(mouseGrid);
                }
                else if (pointChosen && !validDirections.Contains(mouseGrid))
                {
                    CancelMove();
                }
            }
            else
            {
                CancelMove();
                TakeNaval(mouseGrid);
            }
        }
        else
        {
            TakeNaval(mouseGrid);
        }
    }

    public void CancelMove()
    {
        ClearSigns();
        ClearHighlights();
        pointChosen = false;
        takenNaval = null;
        navalTaken = false;
    }

    public void DirectMoveNaval(Vector2 mouseGrid)
    {
        Cell cell = takenNaval.Cells[0];
        navalMap.SetCell(cell.X, cell.Y, -1);
        takenNaval.Cells[0] = new Cell((int)mouseGrid.x, (int)mouseGrid.y, 1);
        navals[takenNavalIndex] = takenNaval;
        ClearSigns();
        ClearHighlights();
        AddNaval(takenNaval);
        pointChosen = false;
        takenNaval = null;
        navalTaken = false;
    }

    public void MoveNaval(Vector2 mouseGrid)
    {
        foreach (Cell cell in takenNaval.Cells)
        {
            navalMap.SetCell(cell.X, cell.Y, -1);
        }
        Vector2 direction = mouseGrid - pathOrigin;
        for (int i = 0; i < takenNaval.Cells.Length; i++)
        {
            takenNaval.Cells[i] = new Cell((int)(pathOrigin + direction * i).x, (int)(pathOrigin + direction * i).y, 1);
        }
        navals[takenNavalIndex] = takenNaval;
        ClearSigns();
        ClearHighlights();
        AddNaval(takenNaval);
        pointChosen = false;
        takenNaval = null;
        navalTaken = false;
    }

    public void ShowValidPathsIfAny(Vector2 mouseGrid)
    {
        int pathLength = takenNaval.Cells.Length;
        Vector2[] upPath = CollectPath(mouseGrid, Vector2.Up, pathLength);
        Vector2[] downPath = CollectPath(mouseGrid, Vector2.Down, pathLength);
        Vector2[] leftPath = CollectPath(mouseGrid, Vector2.Left, pathLength);
        Vector2[] rightPath = CollectPath(mouseGrid, Vector2.Right, pathLength);

        if (!(upPath.Length == 0 && downPath.Length == 0 && leftPath.Length == 0 && rightPath.Length == 0))
        {
            editMap.SetCellv(mouseGrid, 2);
            pathOrigin = mouseGrid;

            validDirections.Clear();

            if (upPath.Length != 0)
            {
                editMap.SetCellv(mouseGrid + Vector2.Up, 3);
                validDirections.AddLast(mouseGrid + Vector2.Up);
            }
            if (downPath.Length != 0)
            {
                editMap.SetCellv(mouseGrid + Vector2.Down, 5);
                validDirections.AddLast(mouseGrid + Vector2.Down);
            }
            if (leftPath.Length != 0)
            {
                editMap.SetCellv(mouseGrid + Vector2.Left, 6);
                validDirections.AddLast(mouseGrid + Vector2.Left);
            }
            if (rightPath.Length != 0)
            {
                editMap.SetCellv(mouseGrid + Vector2.Right, 4);
                validDirections.AddLast(mouseGrid + Vector2.Right);
            }
            pointChosen = true;
        }
        else pointChosen = false;
    }

    public Vector2[] CollectPath(Vector2 origin, Vector2 direction, int length)
    {
        LinkedList<Vector2> path = new LinkedList<Vector2>();

        for (int i = 0; i < length; i++)
        {
            Vector2 cell = origin + direction * i;
            if (IsCellEmpty(cell) && IsInSea(cell))
            {
                path.AddLast(cell);
            }
            else
            {
                path.Clear();
                return new Vector2[] { };
            }
        }
        return path.ToArray();
    }

    public bool TakeNaval(Vector2 mouseGrid)
    {
        for (int i = 0; i < navals.Length; i++)
        {
            foreach (Cell cell in navals[i].Cells)
            {
                if (new Vector2(cell.X, cell.Y) == mouseGrid)
                {
                    if (navals[i] != takenNaval)
                    {
                        ClearHighlights();
                        takenNaval = navals[i];
                        takenNavalIndex = i;
                        navalTaken = true;
                        HighlightNaval(takenNaval);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public void HighlightNaval(Naval naval)
    {
        foreach (Cell cell in naval.Cells)
        {
            foreach (Cell nb in GetNeighbors(cell))
            {
                highLightMap.SetCell(nb.X, nb.Y, 0);
            }
        }
    }

    public void ClearSigns()
    {
        for (int i = -1; i <= size; i++)
        {
            for (int j = -1; j <= size + 8; j++)
            {
                    editMap.SetCell(i, j, -1);
            }
        }
    }

    public void ClearHighlights()
    {
        for (int i = -1; i <= size; i++)
        {
            for (int j = -1; j <= size + 8; j++)
            {
                if(highLightMap.GetCell(i, j) == 0)
                    highLightMap.SetCell(i, j, -1);
            }
        }
    }

    public Vector2 ToGrid(Vector2 position)
    {
        position.x = (float)Math.Floor(position.x / 64);
        position.y = (float)Math.Floor(position.y / 64);

        return position;
    }

    public void UpdateBitmask(TileMap tileMap)
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size + 8; j++)
            {
                if(tileMap.GetCell(i, j) != -1)
                    tileMap.UpdateBitmaskArea(new Vector2(i, j));
            }
        }
    }

    public void SpawnNavals()
    {
        if(navals == null)
            navals = new Naval[10];

        navals[0] = new Naval(new Vector2(0, 11), Vector2.Zero, 1);
        navals[1] = new Naval(new Vector2(2, 11), Vector2.Zero, 1);
        navals[2] = new Naval(new Vector2(4, 11), Vector2.Zero, 1);
        navals[3] = new Naval(new Vector2(6, 11), Vector2.Zero, 1);

        navals[4] = new Naval(new Vector2(8, 11), Vector2.Right, 2);
        navals[5] = new Naval(new Vector2(8, 13), Vector2.Right, 2);
        navals[6] = new Naval(new Vector2(8, 15), Vector2.Right, 2);

        navals[7] = new Naval(new Vector2(0, 13), Vector2.Right, 3);
        navals[8] = new Naval(new Vector2(4, 13), Vector2.Right, 3);

        navals[9] = new Naval(new Vector2(3, 15), Vector2.Right, 4);
    }


    public bool IsInSea(Vector2 cell)
    {
        return IsInSea(cell.x, cell.y);
    }

    public bool IsInSea(float x, float y)
    {
        return IsInSea((int)x, (int)y);
    }

    public bool IsInSea(Cell cell)
    {
        return IsInSea(cell.X, cell.Y);
    }

    public bool IsInSea(int x, int y)
    {
        if (x >= 0 && x < size && y >= 0 && y < size)
            return true;
        return false;
    }

    public bool IsCellEmpty(Cell ChosenCell)
    {
        return IsCellEmpty(new Vector2(ChosenCell.X, ChosenCell.Y));
    }

    public bool IsCellEmpty(Vector2 ChosenCell)
    {
        foreach (Cell cell in GetNeighbors(ChosenCell))
        {
            
            if (cell.Tile == 1)
            {
                bool result = false;
                foreach (Cell navalCell in takenNaval.Cells)
                {
                    if (navalCell.X == cell.X && navalCell.Y == cell.Y)
                        result = true;
                }
                if (!result) return result;
            }
        }
        return true;
    }

    public Cell[] GetNeighbors(Cell cell)
    {
        return (GetNeighbors(new Vector2(cell.X, cell.Y)));
    }

    public Cell[] GetNeighbors(Vector2 cell)
    {
        int x = (int)cell.x;
        int y = (int)cell.y;

        LinkedList<Cell> neighbors = new LinkedList<Cell>();

        neighbors.AddFirst(new Cell(x, y, navalMap.GetCell(x, y)));

        if (IsInSea(x + 1, y))
            neighbors.AddFirst(new Cell(x + 1, y, navalMap.GetCell(x + 1, y)));

        if (IsInSea(x - 1, y))
            neighbors.AddFirst(new Cell(x - 1, y, navalMap.GetCell(x - 1, y)));

        if (IsInSea(x, y + 1))
            neighbors.AddFirst(new Cell(x, y + 1, navalMap.GetCell(x, y + 1)));

        if (IsInSea(x, y - 1))
            neighbors.AddFirst(new Cell(x, y - 1, navalMap.GetCell(x, y - 1)));

        if (IsInSea(x + 1, y + 1))
            neighbors.AddFirst(new Cell(x + 1, y + 1, navalMap.GetCell(x + 1, y + 1)));

        if (IsInSea(x + 1, y - 1))
            neighbors.AddFirst(new Cell(x + 1, y - 1, navalMap.GetCell(x + 1, y - 1)));

        if (IsInSea(x - 1, y + 1))
            neighbors.AddFirst(new Cell(x - 1, y + 1, navalMap.GetCell(x - 1, y + 1)));

        if (IsInSea(x - 1, y - 1))
            neighbors.AddFirst(new Cell(x - 1, y - 1, navalMap.GetCell(x - 1, y - 1)));

        return neighbors.ToArray();
    }

    public void AddNaval(Naval naval)
    {
        foreach (Cell cell in naval.Cells)
        {
            navalMap.SetCell(cell.X, cell.Y, cell.Tile);
        }
    }

    public void HitCell(int x, int y)
    {
        if(navalMap.GetCell(x, y) == 1)
            seaMap.SetCell(x, y, 0);
        else
            seaMap.SetCell(x, y, 1);
    }

    public bool IsCellHittable(int x, int y)
    {
        if (IsInSea(x, y))
            return (seaMap.GetCell(x, y) == -1);
        else
            return false;
    }

    public void RevealSinkNavals()
    {
        foreach (Naval naval in navals)
        {
            RevealAroundIfSink(naval);
        }
        UpdateBitmask(seaMap);
    }

    public void RevealAroundIfSink(Naval naval)
    {
        if (IsSink(naval))
        {
            foreach (Cell cell in naval.Cells)
            {
                foreach (Cell neighbour in GetNeighbors(cell))
                {
                    if (neighbour.Tile == 1)
                        seaMap.SetCell(neighbour.X, neighbour.Y, 2);
                    else
                        seaMap.SetCell(neighbour.X, neighbour.Y, 1);
                }
            }
        }
    }

    public bool IsSink(Naval naval)
    {
        foreach (Cell cell in naval.Cells)
        {
            if (seaMap.GetCell(cell.X, cell.Y) == -1)
                return false;
        }
        return true;
    }
}
