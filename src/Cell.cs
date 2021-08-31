using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Cell
{
    private int x, y, tile;


    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }
    public int Tile { get => tile; set => tile = value; }

    public Cell(int x, int y, int tile)
    {
        this.x = x;
        this.y = y;
        this.tile = tile;
    }
}

public class SeaTiles
{
    public static int hiddenHitNaval = 0;
    public static int hitSea = 1;
    public static int sinkNaval = 2;
}

public class EditMap
{
    public static int place = 0;
    public static int full = 1;
}
