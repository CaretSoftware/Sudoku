using System;
using System.Collections.Generic;

public enum Symmetry { Vertical, Horizontal, Diagonal }
    
public static class Extensions {
    public static void Shuffle<T>(this IList<T> list, Random random) {  
        int n = list.Count;
        while (n > 1) {
            n--;  
            int k = random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }  
    }

    public static T RandomEnumValue<T>(this T enumeration, Random random = null) where T : Enum {
        random ??= new Random();
        Array array = Enum.GetValues(typeof(T));
        return (T)array.GetValue(random.Next(array.Length));
    }
}
