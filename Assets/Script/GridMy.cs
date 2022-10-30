using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMy : MonoBehaviour
{
    public int wight;
    public int hight;

    private int[,] gridArray;

    public GridMy(int wight, int hight)
    {
        this.wight = wight;
        this.hight = hight;
        gridArray =new int[wight,hight];
    }
}
