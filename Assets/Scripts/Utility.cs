using System;

public static class Utility {
    /// <summary>
    /// This method implement <strong>Fisher–Yates</strong> algorithm to generate a random
    /// permutation of a finite sequence, or in other way, we shuffle the array.
    /// </summary>
    /// <typeparam name="T">Type of array's elements</typeparam>
    /// <param name="array">Array need to shuffle</param>
    /// <param name="seed"></param>
    /// <returns>A array with elements have been shuffled</returns>
    public static T[] ShuffleArray<T> (T[] array, int seed) {
        Random pseudoRandomGenerator = new Random (seed);

        for (int i = 0; i < array.Length - 1; i++) {
            int randomIndex = pseudoRandomGenerator.Next (i, array.Length);
            var temp = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = temp;
        }

        return array;
    }
}
