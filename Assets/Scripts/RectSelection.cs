using UnityEngine;
using UnityEditor;

public class RectSelection
{
    Rect selectionRect;

    // Start is called before the first frame update
    public RectSelection(int left, int right, int bottom, int top)
    {
        selectionRect = new Rect(left, bottom, right - left, top - bottom);
    }

    public RectSelection(int numSpaces)
    {
        
    }

}