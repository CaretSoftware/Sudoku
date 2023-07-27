using System;
using System.Collections.Generic;

public enum Symmetry { Vertical, Horizontal, Diagonal }
    
public static class Extensions {
    public static void Shuffle<T>(this IList<T> list, System.Random random) {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }  
    }

    public static T RandomEnumValue<T>(this Enum e, Random random = null) where T : Enum {
        random ??= new Random();
        Array array = Enum.GetValues(typeof(T));
        return (T)array.GetValue(random.Next(array.Length));
    }
    
    public static int[,] Copy(this int[,] original) {
        if (original == null) return null;
        
        int[,] copy = new int[original.GetLength(0), original.GetLength(1)];
        for (int col = 0; col < original.GetLength(1); col++) {
            for (int row = 0; row < original.GetLength(0); row++) {
                copy[row, col] = original[row, col];
            }
        }

        return copy;
    }
}
