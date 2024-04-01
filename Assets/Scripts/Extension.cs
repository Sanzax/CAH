using System.Collections.Generic;
using UnityEngine;

public static class Extension
{
    public static List<Transform> GetChildren(this Transform parent)
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in parent)
        {
            children.Add(child);
        }

        return children;
    }
}
