using System.Collections.Generic;
using UnityEngine;

public static class WeightedRandom
{
    public static int GetRandomIndex(List<int> chances)
    {
        int total = AddUpTotal(chances);

        float rand = Random.value * total;

        for(int i = 0; i < chances.Count; i++)
        {
            if(rand < chances[i])
                return i;
            rand -= chances[i];
        }

        return 0;
    }

    static int AddUpTotal(List<int> chances)
    {
        int total = 0;
        for (int i = 0; i < chances.Count; i++)
        {
            total += chances[i];
        }
        return total;
    }
    
}
