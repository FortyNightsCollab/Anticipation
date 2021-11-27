using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSelection
{
    SelectionRow[] selectionRows;

    
    public MapSelection(RowOffest offset, int rowWidth, int numRows)
    {
        selectionRows = new SelectionRow[numRows];   

        for(int i = 0; i < selectionRows.Length; i++)
        {
            selectionRows[i] = new SelectionRow();
            selectionRows[i].width = rowWidth;
            selectionRows[i].offset.x = offset.x;
            selectionRows[i].offset.y = offset.y + i;
        }
    }


    public SelectionRow GetRow(int row)
    {
        SelectionRow rowToReturn = new SelectionRow();

        if (row < selectionRows.Length)
        {
            rowToReturn.offset = selectionRows[row].offset;
            rowToReturn.width = selectionRows[row].width;
            return rowToReturn;
        }

        return rowToReturn;
    }
}

public class RowOffest
{
    public int x;
    public int y;

    public RowOffest()
    {
        x = 0;
        y = 0;
    }

    public RowOffest(int setX, int setY)
    {
        x = setX;
        y = setY;
    }
   
}

public class SelectionRow
{
    public RowOffest offset;
    public int width;

    public SelectionRow()
    {
        offset = new RowOffest();
        width = 0;
    }
}

