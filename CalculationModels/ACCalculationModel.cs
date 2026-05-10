using System;
using System.Collections.Generic;
using System.Linq;

namespace ArmyCommander.CalculationModel
{
    public static class ACCalculationModel
    {
        private sealed class NumberEntry
        {
            public int Value { get; set; }
            public int OriginalIndex { get; set; }
        }

        public static List<int> DistributeToSmallestKeepOriginalOrder(
            IEnumerable<int> numberList,
            int amountToAdd)
        {
            if (numberList == null)
                throw new ArgumentNullException(nameof(numberList));

            if (amountToAdd < 0)
                throw new ArgumentOutOfRangeException(nameof(amountToAdd), "amountToAdd não pode ser negativo.");

            List<NumberEntry> entries = numberList
                .Select((value, index) => new NumberEntry
                {
                    Value = value,
                    OriginalIndex = index
                })
                .OrderBy(entry => entry.Value)
                .ThenBy(entry => entry.OriginalIndex)
                .ToList();

            int n = entries.Count;

            if (n == 0 || amountToAdd == 0)
                return ToOriginalOrder(entries);

            long amount = amountToAdd;
            long level = entries[0].Value;

            // Quantidade de elementos no grupo dos menores.
            int i = 1;

            while (i < n && amount > 0)
            {
                // Inclui valores empatados no menor grupo.
                if (entries[i].Value == level)
                {
                    i++;
                    continue;
                }

                long gap = entries[i].Value - level;
                long costToReachNextLevel = gap * i;

                if (amount >= costToReachNextLevel)
                {
                    amount -= costToReachNextLevel;
                    level = entries[i].Value;
                    i++;
                }
                else
                {
                    long increase = amount / i;
                    int remainder = (int)(amount % i);

                    level += increase;

                    for (int j = 0; j < i - remainder; j++)
                        entries[j].Value = checked((int)level);

                    for (int j = i - remainder; j < i; j++)
                        entries[j].Value = checked((int)(level + 1));

                    return ToOriginalOrder(entries);
                }
            }

            if (amount > 0)
            {
                long increase = amount / n;
                int remainder = (int)(amount % n);

                level += increase;

                for (int j = 0; j < n - remainder; j++)
                    entries[j].Value = checked((int)level);

                for (int j = n - remainder; j < n; j++)
                    entries[j].Value = checked((int)(level + 1));
            }
            else
            {
                for (int j = 0; j < i; j++)
                    entries[j].Value = checked((int)level);
            }

            return ToOriginalOrder(entries);
        }

        private static List<int> ToOriginalOrder(List<NumberEntry> entries)
        {
            return entries
                .OrderBy(entry => entry.OriginalIndex)
                .Select(entry => entry.Value)
                .ToList();
        }
    }
}