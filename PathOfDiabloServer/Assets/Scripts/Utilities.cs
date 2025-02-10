using System;

public static class Utilities
{
    public static string Concatenate(int signifier, params string[] values)
    {
        const string SepChar = ",";
        string conStr =  ((int)signifier).ToString();

        foreach (string v in values)
        {
            conStr = conStr + SepChar + v;
        }

        return conStr;
    }

}