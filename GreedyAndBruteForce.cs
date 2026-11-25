using System;
using System.Collections.Generic;

namespace OpamLab10
{
    public static class GreedyAndBruteForce
    {
        // =========================================================================
        // b = 0: Решта заданими номіналами найменшою кількістю монет
        // =========================================================================
        public static int CoinsGreedy(int amount, int[] coinsDesc)
        {
            if (amount == 0) return 0;
            int rem = amount;
            int count = 0;
            for (int i = 0; i < coinsDesc.Length; i++)
            {
                int take = rem / coinsDesc[i];
                count += take;
                rem -= take * coinsDesc[i];
            }
            return rem == 0 ? count : int.MaxValue / 2;
        }

        public static int CoinsBruteForce(int amount, int[] coinsDesc, int idx = 0)
        {
            if (amount == 0) return 0;
            if (idx >= coinsDesc.Length || amount < 0) return int.MaxValue / 2;

            int best = int.MaxValue / 2;
            int maxTake = amount / coinsDesc[idx];
            for (int take = maxTake; take >= 0; take--)
            {
                int sub = CoinsBruteForce(amount - take * coinsDesc[idx], coinsDesc, idx + 1);
                if (sub + take < best) best = sub + take;
            }
            return best;
        }

        // =========================================================================
        // b = 1: Найбільше заявок без перетинів
        // =========================================================================
        public readonly struct Interval
        {
            public readonly int Start;
            public readonly int End;
            public Interval(int start, int end) { Start = start; End = end; }
            public override string ToString() => $"[{Start}; {End}]";
        }

        public static int IntervalsGreedy(Interval[] items)
        {
            if (items.Length == 0) return 0;
            var sorted = (Interval[])items.Clone();
            Array.Sort(sorted, (x, y) => x.End.CompareTo(y.End));

            int count = 0;
            int lastEnd = int.MinValue;
            for (int i = 0; i < sorted.Length; i++)
            {
                if (sorted[i].Start >= lastEnd)
                {
                    count++;
                    lastEnd = sorted[i].End;
                }
            }
            return count;
        }

        public static int IntervalsBruteForce(Interval[] items)
        {
            if (items.Length == 0) return 0;
            var sorted = (Interval[])items.Clone();
            Array.Sort(sorted, (x, y) => x.Start.CompareTo(y.Start));
            return IntervalsBtRec(sorted, 0, int.MinValue);
        }

        private static int IntervalsBtRec(Interval[] sorted, int idx, int lastEnd)
        {
            if (idx >= sorted.Length) return 0;
            int skip = IntervalsBtRec(sorted, idx + 1, lastEnd);
            int take = 0;
            if (sorted[idx].Start >= lastEnd)
            {
                take = 1 + IntervalsBtRec(sorted, idx + 1, sorted[idx].End);
            }
            return Math.Max(skip, take);
        }

        // =========================================================================
        // b = 2: Рюкзак цілими предметами (жадібно за ціною/кг проти перебору)
        // =========================================================================
        public readonly struct KnapsackItem
        {
            public readonly int Weight;
            public readonly int Value;
            public KnapsackItem(int weight, int value) { Weight = weight; Value = value; }
            public override string ToString() => $"(w={Weight}, v={Value})";
        }

        public static int KnapsackGreedy(KnapsackItem[] items, int capacity)
        {
            if (items.Length == 0 || capacity <= 0) return 0;
            var sorted = (KnapsackItem[])items.Clone();
            Array.Sort(sorted, (x, y) => ((double)y.Value / y.Weight).CompareTo((double)x.Value / x.Weight));

            int rem = capacity;
            int totalVal = 0;
            for (int i = 0; i < sorted.Length; i++)
            {
                if (sorted[i].Weight <= rem)
                {
                    rem -= sorted[i].Weight;
                    totalVal += sorted[i].Value;
                }
            }
            return totalVal;
        }

        public static int KnapsackBruteForce(KnapsackItem[] items, int capacity, int idx = 0)
        {
            if (idx >= items.Length || capacity <= 0) return 0;
            int skip = KnapsackBruteForce(items, capacity, idx + 1);
            int take = 0;
            if (items[idx].Weight <= capacity)
            {
                take = items[idx].Value + KnapsackBruteForce(items, capacity - items[idx].Weight, idx + 1);
            }
            return Math.Max(skip, take);
        }

        // =========================================================================
        // b = 3: Розрізати стрижень на деталі заданих довжин з найменшим відходом
        // (повертаємо максимальну корисну використану довжину, відхід = L - used)
        // =========================================================================
        public static int RodCutGreedy(int rodLength, int[] parts)
        {
            if (parts.Length == 0 || rodLength <= 0) return 0;
            var sorted = (int[])parts.Clone();
            Array.Sort(sorted, (x, y) => y.CompareTo(x)); // за спаданням довжини

            int rem = rodLength;
            int used = 0;
            for (int i = 0; i < sorted.Length; i++)
            {
                if (sorted[i] <= rem)
                {
                    rem -= sorted[i];
                    used += sorted[i];
                }
            }
            return used;
        }

        public static int RodCutBruteForce(int rodLength, int[] parts, int idx = 0)
        {
            if (idx >= parts.Length || rodLength <= 0) return 0;
            int skip = RodCutBruteForce(rodLength, parts, idx + 1);
            int take = 0;
            if (parts[idx] <= rodLength)
            {
                take = parts[idx] + RodCutBruteForce(rodLength - parts[idx], parts, idx + 1);
            }
            return Math.Max(skip, take);
        }
    }
}