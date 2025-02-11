using System;
using UnityEngine;

public static class Utilities
{
    public static string Concatenate(int signifier, params string[] values)
    {
        const string SepChar = ",";
        string conStr = ((int)signifier).ToString();

        foreach (string v in values)
        {
            conStr = conStr + SepChar + v;
        }

        return conStr;
    }


    public static float GetDistanceBetween(Vector2 pos1, Vector2 pos2)
    {
        float xDif = Mathf.Abs(pos1.x - pos2.x);
        float yDif = Mathf.Abs(pos1.y - pos2.y);
        float dist = Mathf.Sqrt(xDif * xDif + yDif * yDif);

        return dist;
    }

}