using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace app
{
    public class Program
    {
        public static int[] Outcome { get; private set; } = new int[10];
        private static Random Random { get; } = new();
        public static void Main()
        {
            // Game Cycle;
            while (true)
            {
                // Reset "Outcome" static array
                Array.Fill(Outcome, -1);

                Console.WriteLine("Press any Key to generate new values. Press ESC to Exit");
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                // Exit condition
                if (keyInfo.Key == ConsoleKey.Escape)
                    break;

                GenerateRoundValues();
            }
        }

        /// <summary>
        /// Generates values and prints them
        /// </summary>
        private static void GenerateRoundValues()
        {
            // Generate total payout and attempts (tries)
            int totalPay = Random.Next(5, 101);
            int tries = Random.Next(1, 4);

            // Generate values distribution
            List<int> values = GenerateDistribution(new List<int>(), totalPay, tries);

            // Add values to the "Outcome" static array
            for (int i = 0; i < values.Count; i++)
            {
                Outcome[i] = values[i];
            }

            // For testing purposes...
            ValidateOutcome(totalPay, tries);

            PrintValues(totalPay, tries);
        }

        /// <summary>
        /// Generates payout distribution
        /// </summary>
        /// <param name="values">List of distributed values (starts as an empty list)</param>
        /// <param name="remainingTotal">Remaining Payout</param>
        /// <param name="tries">Remaining Attempts (starts as the generated number of attempts)</param>
        /// <returns></returns>
        static List<int> GenerateDistribution(List<int> values, int remainingTotal, int tries)
        {
            // Stop Condition / Last Value
            if (tries == 1)
            {
                if (remainingTotal > 20) values.Add(0);
                values.Add(remainingTotal);
                return values;
            }
            // When 0 is possible there is no minimum
            int min = 0;
            if (remainingTotal < 21) min = Math.Max(1, remainingTotal - (tries - 1) * 20 + (tries - 2) * (tries - 1) / 2);
            int max = Math.Min(remainingTotal + 1 - (tries - 1) * tries / 2, 21);

            // Generate List of possible values
            List<int> possibleValues = RefinePossibleValues(Enumerable.Range(min, max - min).ToList(), remainingTotal, values, tries);

            // Generate Value from list of possible values
            int newValue = possibleValues[Random.Next(0, possibleValues.Count)];
            values.Add(newValue);

            // Stop Condition / A wild 0 has appeared
            if (newValue == 0)
            {
                values.Add(remainingTotal - newValue);
                return values;
            }

            // Generate next value
            return GenerateDistribution(values, remainingTotal - newValue, tries - 1);
        }

        /// <summary>
        /// Modifies list of possible values for the next iteration.
        /// </summary>
        /// <param name="possibleValues">Raw List of possible values from min to max</param>
        /// <param name="remainingTotal">Remating Payout</param>
        /// <param name="values">Previous Values</param>
        /// <param name="tries">Remaining Attempts (tries)</param>
        /// <returns></returns>
        private static List<int> RefinePossibleValues(List<int> possibleValues, int remainingTotal, List<int> values, int tries)
        {
            // Save List in case a scenario appears where the only option is to have a repeated value
            // ex: 5 with 3 attempts needs one repetition (variances of 1 2 2 and 1 1 3)
            List<int> originalValues = new(possibleValues);

            // Remove repeated values
            foreach (int v in values)
            {
                if (possibleValues.Contains(v))
                {
                    possibleValues.Remove(v);
                }
            }

            // Remove potential values that will cause the last value to be equal to a previous one
            if (tries == 2)
            {
                if (remainingTotal % 2 == 0 && possibleValues.Contains(remainingTotal / 2)) possibleValues.Remove(remainingTotal / 2);
                // scalability for tries > 3
                List<int> removable = new();
                foreach (int v in values)
                {
                    for (int i = 0; i < possibleValues.Count; i++)
                    {
                        if (remainingTotal - possibleValues[i] == v) removable.Add(i);
                    }
                }
                for (int i = removable.Count - 1; i >= 0; i--)
                {
                    possibleValues.RemoveAt(removable[i]);
                }
            }

            // If there are no possible values, accept repeated values
            if (possibleValues.Count == 0) possibleValues = originalValues;

            return possibleValues;
        }

        /// <summary>
        /// Prints all generated values
        /// </summary>
        /// <param name="total">Total Payout</param>
        /// <param name="tries">Number of Attempts</param>
        static void PrintValues(int total, int tries)
        {
            Console.WriteLine("\nValor Total = " + total + ", Tentativas = " + tries + ".");
            for (int i = 0; i < Outcome.Length; i++)
            {
                if (Outcome[i] == -1) break;
                Console.Write("Array[" + i + "] = " + Outcome[i] + "; ");
            }
            Console.WriteLine("\n");
        }

        /// <summary>
        /// Validates Outcome for testing purposes
        /// </summary>
        /// <param name="total">Total Payout</param>
        /// <param name="tries">Number of Attempts</param>
        static void ValidateOutcome(int total, int tries)
        {
            int sum = 0;
            int count = 0;
            foreach (int o in Outcome)
            {
                if (o == -1) break;
                sum += o;
                count++;
            }
            if (sum != total) Environment.FailFast("Wrong Total");
            if (count != tries && !Outcome.Contains(0)) Environment.FailFast("Wrong Distribution Length");
        }

    }
}