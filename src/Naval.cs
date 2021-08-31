using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Naval
{
    private Cell[] cells;

    public Cell[] Cells { get => cells; set => cells = value; }

    public Naval(Vector2 startPoint, Vector2 direction, int length)
    {
        cells = new Cell[length];

        for (int i = 0; i < length; i++)
        {
            Vector2 cell = startPoint + direction * i;
            cells[i] = new Cell((int)cell.x, (int)cell.y, 1);
        }
    }    
}


