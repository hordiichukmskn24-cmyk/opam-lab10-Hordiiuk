using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace OpamLab10
{
    internal class Program
    {
        static readonly char[] UkrAlphabet =
        {
            'А','Б','В','Г','Ґ','Д','Е','Є','Ж','З','И','І','Ї','Й','К','Л','М',
            'Н','О','П','Р','С','Т','У','Ф','Х','Ц','Ч','Ш','Щ','Ь','Ю','Я'
        };

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            // =========================================================================
            // ВКАЖІТЬ ВАШІ ПАРАМЕТРИ ВАРІАНТА ТУТ:
            int N = 7;   // Порядковий номер у списку групи
            int K = 19;  // Номер першої літери імені в українській абетці (1..33)
            // =========================================================================

            int a = N % 3;
            int b = N % 4;
            int c = N % 5;

            Console.WriteLine($"N = {N}, K = {K}, a = {a}, b = {b}, c = {c}");
            Console.WriteLine(new string('=', 80));
            #if DEBUG
            Console.WriteLine("УВАГА: Запущено в DEBUG! Запустіть: dotnet run -c Release");
            #endif

            // Підготовка файлу української прози (щонайменше 50 000 символів)
            string prosePath = "prose.txt";
            string prose = EnsureUkrainianProseFile(prosePath, 55_000);
            Console.WriteLine($"Файл прози '{prosePath}' готовий: {prose.Length:N0} символів.\n");

            // --- ЗАВДАННЯ 1: Три пастки ---
            RunTask1(prose);

            // --- ЗАВДАННЯ 2: Ваша задача (таблиця A) ---
            RunTask2(prose, a, K);

            // --- ЗАВДАННЯ 3: Пошук підрядка ---
            RunTask3(prose, K);

            // --- ЗАВДАННЯ 4: Жадібно проти перебору (таблиця B) ---
            RunTask4(b, N, K);

            // --- ДОДАТКОВО: Таблиця C ---
            RunTaskC(prose, c, K, N);

            // --- ПЕРЕВІРКА КРАЙОВИХ ВИПАДКІВ ---
            RunEdgeCases(a, b, K);
        }

        static string EnsureUkrainianProseFile(string path, int minLength)
        {
            if (File.Exists(path))
            {
                string existing = File.ReadAllText(path, Encoding.UTF8);
                if (existing.Length >= minLength) return existing;
            }

            string[] paragraphs =
            {
                "Реве та стогне Дніпр широкий, сердитий вітер завива, додолу верби гне високі, горами хвилю підійма. І блідий місяць на ту пору із хмари де-де виглядав, неначе човен в синім морі, то виринав, то потопав. ",
                "Садок вишневий коло хати, хрущі над вишнями гудуть, плугатарі з плугами йдуть, співають ідучи дівчата, а матері вечерять ждуть. Сім'я вечеря коло хати, вечірня зіронька встає, дочка вечерять подає, а мати хоче научати, так соловейко не дає. ",
                "Земля українська багата на ґрунти родючі, на ліси зелені та ріки повноводні. У кожному селі біля хати росте калина, а в садку цвітуть яблуні та груші. Люди шанують рідну мову, пісню та історію свого краю. ",
                "На високій горі стояв старий замок із міцними кам'яними мурами та дубовими воротами. Мандрівники часто зупинялися біля криниці, щоб напитися холодної джерельної води й перепочити перед довгою дорогою через степ. ",
                "Алгоритми та структури даних дозволяють розв'язувати складні інженерні задачі швидко та ефективно. Правильний вибір структури пам'яті нерідко дає більший виграш у швидкодії, ніж дрібна оптимізація окремих інструкцій процесора. "
            };

            var sb = new StringBuilder(minLength + 1000);
            int idx = 0;
            while (sb.Length < minLength)
            {
                sb.Append(paragraphs[idx % paragraphs.Length]);
                idx++;
            }
            string text = sb.ToString();
            File.WriteAllText(path, text, Encoding.UTF8);
            return text;
        }

        static void RunTask1(string prose)
        {
            Console.WriteLine("--- ЗАВДАННЯ 1: Три пастки ---");

            // Прогрів
            StringAlgorithms.ConcatWithPlusEquals(100);
            StringAlgorithms.ConcatWithStringBuilder(100);

            double plus10k = MeasureMinMs(() => StringAlgorithms.ConcatWithPlusEquals(10_000));
            double sb10k = MeasureMinMs(() => StringAlgorithms.ConcatWithStringBuilder(10_000));
            double plus100k = MeasureMinMs(() => StringAlgorithms.ConcatWithPlusEquals(100_000));
            double sb100k = MeasureMinMs(() => StringAlgorithms.ConcatWithStringBuilder(100_000));

            Console.WriteLine($"Пастка 1 (10 000 додавань):  s += c = {plus10k:F3} мс | StringBuilder = {sb10k:F4} мс");
            Console.WriteLine($"Пастка 1 (100 000 додавань): s += c = {plus100k:F3} мс | StringBuilder = {sb100k:F4} мс");

            string[] sampleWords = StringAlgorithms.ExtractWords(prose.Substring(0, 5000)).ToArray();
            StringAlgorithms.ComparePairsToLower(sampleWords, 100);
            StringAlgorithms.ComparePairsOrdinalIgnoreCase(sampleWords, 100);

            double toLowerMs = MeasureMinMs(() => StringAlgorithms.ComparePairsToLower(sampleWords, 100_000));
            double ordinalMs = MeasureMinMs(() => StringAlgorithms.ComparePairsOrdinalIgnoreCase(sampleWords, 100_000));
            Console.WriteLine($"Пастка 2 (100 000 порівнянь): ToLower() == = {toLowerMs:F3} мс | OrdinalIgnoreCase = {ordinalMs:F3} мс");

            var (ord, ukr) = StringAlgorithms.CompareUkrainianSorting();
            Console.WriteLine("Пастка 3 (сортування слів із 'ґ'):");
            Console.WriteLine("  Ordinal: " + string.Join(", ", ord));
            Console.WriteLine("  uk-UA:   " + string.Join(", ", ukr));
        }

        static void RunTask2(string prose, int a, int K)
        {
            int topK = (K % 7) + 5;
            string taskTitle = a == 0 ? "a=0: Найдовший підрядок без повторів"
                             : a == 1 ? $"a=1: Топ-{topK} найчастіших слів"
                                      : "a=2: Нормалізація тексту";

            Console.WriteLine($"\n--- ЗАВДАННЯ 2: {taskTitle} ---");
            Console.WriteLine($"{"Символів",10} | {"Наївно, мс",14} | {"Швидко, мс",14} | {"У скільки разів",16}");
            Console.WriteLine(new string('-', 62));

            // Прогрів
            RunTask2Pair(prose.Substring(0, 200), a, topK);

            int[] lengths = { 1_000, 10_000, 50_000 };
            foreach (int len in lengths)
            {
                string slice = prose.Substring(0, len);

                // Перевірка збігу відповідей
                if (!RunTask2Pair(slice, a, topK))
                    Console.WriteLine($"ПОМИЛКА: відповіді наївного і швидкого алгоритмів на {len} символах не збіглися!");

                double naiveMs = MeasureMinMs(() => ExecuteNaiveTask2(slice, a, topK));
                double fastMs = MeasureMinMs(() => ExecuteFastTask2(slice, a, topK));
                double ratio = fastMs > 0 ? naiveMs / fastMs : 0;

                Console.WriteLine($"{len,10:N0} | {naiveMs,14:F3} | {fastMs,14:F3} | {ratio,15:F1}x");
            }
        }

        static bool RunTask2Pair(string text, int a, int topK)
        {
            if (a == 0)
            {
                return StringAlgorithms.LongestUniqueSubstringNaive(text) == StringAlgorithms.LongestUniqueSubstringFast(text);
            }
            else if (a == 1)
            {
                var r1 = StringAlgorithms.TopKWordsNaive(text, topK);
                var r2 = StringAlgorithms.TopKWordsFast(text, topK);
                if (r1.Count != r2.Count) return false;
                for (int i = 0; i < r1.Count; i++)
                    if (r1[i].Word != r2[i].Word || r1[i].Count != r2[i].Count) return false;
                return true;
            }
            else
            {
                return StringAlgorithms.NormalizeTextNaive(text) == StringAlgorithms.NormalizeTextFast(text);
            }
        }

        static void ExecuteNaiveTask2(string text, int a, int topK)
        {
            if (a == 0) _ = StringAlgorithms.LongestUniqueSubstringNaive(text);
            else if (a == 1) _ = StringAlgorithms.TopKWordsNaive(text, topK);
            else _ = StringAlgorithms.NormalizeTextNaive(text);
        }

        static void ExecuteFastTask2(string text, int a, int topK)
        {
            if (a == 0) _ = StringAlgorithms.LongestUniqueSubstringFast(text);
            else if (a == 1) _ = StringAlgorithms.TopKWordsFast(text, topK);
            else _ = StringAlgorithms.NormalizeTextFast(text);
        }

        static void RunTask3(string prose, int K)
        {
            Console.WriteLine("\n--- ЗАВДАННЯ 3: Власний пошук підрядка NaiveIndexOf ---");
            string prose50k = prose.Substring(0, 50_000);

            char letterK = UkrAlphabet[Math.Clamp(K - 1, 0, 32)];
            char letterNext = UkrAlphabet[K % 33];

            string repeats10k = new string(letterK, 10_000);
            int patLen = 900 + 10 * K;
            string hardPattern = new string(letterK, patLen - 1) + letterNext;

            // 1. Слово з тексту, якого немає на початку
            string existingWord = "інструкцій";
            // 2. Слово, якого в тексті немає
            string missingWord = "синхрофазотрон";

            // Прогрів
            StringAlgorithms.NaiveIndexOf(prose50k, existingWord, out _);
            _ = prose50k.IndexOf(existingWord, StringComparison.Ordinal);

            Console.WriteLine($"{"Вхід",32} | {"Шаблон",22} | {"Порівнянь",12} | {"Наш, мс",10} | {"IndexOf, мс",12}");
            Console.WriteLine(new string('-', 98));

            MeasureSubstringCase("проза, 50 тис. символів", existingWord, prose50k, existingWord);
            MeasureSubstringCase("проза, 50 тис. символів", missingWord, prose50k, missingWord);
            MeasureSubstringCase("повтори, 10 тис. однакових", $"довжина {patLen} ({letterK}..{letterNext})", repeats10k, hardPattern);
        }

        static void MeasureSubstringCase(string inputName, string patLabel, string text, string pat)
        {
            int idx1 = StringAlgorithms.NaiveIndexOf(text, pat, out int cmps);
            int idx2 = text.IndexOf(pat, StringComparison.Ordinal);
            if (idx1 != idx2) Console.WriteLine("ПОМИЛКА: індекси NaiveIndexOf та string.IndexOf не збіглися!");

            double ourMs = MeasureMinMs(() => StringAlgorithms.NaiveIndexOf(text, pat, out _));
            double stdMs = MeasureMinMs(() => text.IndexOf(pat, StringComparison.Ordinal));

            Console.WriteLine($"{inputName,32} | {patLabel,22} | {cmps,12:N0} | {ourMs,10:F4} | {stdMs,12:F4}");
        }

        static void RunTask4(int b, int N, int K)
        {
            string title = b == 0 ? "b=0: Решта монетами"
                         : b == 1 ? "b=1: Найбільше заявок без перетинів"
                         : b == 2 ? "b=2: Рюкзак цілими предметами"
                                  : "b=3: Розрізання стрижня (найменший відхід)";
            Console.WriteLine($"\n--- ЗАВДАННЯ 4: {title} (100 тестів) ---");

            var rng = new Random(N);
            int optimalCount = 0;
            double worstGapPct = 0;
            string smallestCounterExample = "немає";
            int smallestSize = int.MaxValue;

            if (b == 0)
            {
                int[] coinsDesc = { (K % 4) + 4, (K % 4) + 3, 1 };
                for (int t = 0; t < 100; t++)
                {
                    int amount = rng.Next(1, 20 + K + 1);
                    int g = GreedyAndBruteForce.CoinsGreedy(amount, coinsDesc);
                    int opt = GreedyAndBruteForce.CoinsBruteForce(amount, coinsDesc);
                    if (g == opt) optimalCount++;
                    else
                    {
                        double gap = ((double)(g - opt) / opt) * 100.0;
                        if (gap > worstGapPct) worstGapPct = gap;
                        if (amount < smallestSize)
                        {
                            smallestSize = amount;
                            smallestCounterExample = $"сума={amount}, номінали=[{string.Join(",", coinsDesc)}]: жадібно={g} мон., оптимум={opt} мон.";
                        }
                    }
                }
            }
            else if (b == 1)
            {
                int maxTime = 2 * K + 10;
                for (int t = 0; t < 100; t++)
                {
                    int nItems = rng.Next(5, 16);
                    var items = new GreedyAndBruteForce.Interval[nItems];
                    for (int i = 0; i < nItems; i++)
                    {
                        int s = rng.Next(0, maxTime);
                        int e = rng.Next(s + 1, maxTime + 1);
                        items[i] = new GreedyAndBruteForce.Interval(s, e);
                    }
                    int g = GreedyAndBruteForce.IntervalsGreedy(items);
                    int opt = GreedyAndBruteForce.IntervalsBruteForce(items);
                    if (g == opt) optimalCount++;
                    else
                    {
                        double gap = ((double)(opt - g) / opt) * 100.0;
                        if (gap > worstGapPct) worstGapPct = gap;
                    }
                }
                smallestCounterExample = "для 'раніше почався': [0;10], [1;3], [4;6]; для 'коротший': [0;5], [4;7], [6;11]";
            }
            else if (b == 2)
            {
                int capacity = 10 + K;
                for (int t = 0; t < 100; t++)
                {
                    int nItems = rng.Next(5, 16);
                    var items = new GreedyAndBruteForce.KnapsackItem[nItems];
                    for (int i = 0; i < nItems; i++)
                        items[i] = new GreedyAndBruteForce.KnapsackItem(rng.Next(2, capacity / 2 + 3), rng.Next(5, 50));

                    int g = GreedyAndBruteForce.KnapsackGreedy(items, capacity);
                    int opt = GreedyAndBruteForce.KnapsackBruteForce(items, capacity);
                    if (g == opt) optimalCount++;
                    else if (opt > 0)
                    {
                        double gap = ((double)(opt - g) / opt) * 100.0;
                        if (gap > worstGapPct) worstGapPct = gap;
                        if (nItems < smallestSize)
                        {
                            smallestSize = nItems;
                            smallestCounterExample = $"n={nItems}, W={capacity}: жадібно={g}, оптимум={opt}";
                        }
                    }
                }
            }
            else
            {
                int rodLen = 30 + K;
                for (int t = 0; t < 100; t++)
                {
                    int nParts = rng.Next(5, 16);
                    int[] parts = new int[nParts];
                    for (int i = 0; i < nParts; i++)
                        parts[i] = rng.Next(3, rodLen / 2 + 5);

                    int gUsed = GreedyAndBruteForce.RodCutGreedy(rodLen, parts);
                    int optUsed = GreedyAndBruteForce.RodCutBruteForce(rodLen, parts);
                    if (gUsed == optUsed) optimalCount++;
                    else if (optUsed > 0)
                    {
                        // Відставання за корисною довжиною (або збільшенням відходу)
                        double gap = ((double)(optUsed - gUsed) / optUsed) * 100.0;
                        if (gap > worstGapPct) worstGapPct = gap;
                        if (nParts < smallestSize)
                        {
                            smallestSize = nParts;
                            smallestCounterExample = $"L={rodLen}, деталі=[{string.Join(",", parts)}]: жадібно={gUsed} (відхід {rodLen - gUsed}), оптимум={optUsed} (відхід {rodLen - optUsed})";
                        }
                    }
                }
            }

            // Замір часу повного перебору на 15 і 20 елементах
            double bt15 = MeasureBruteForceTime(b, 15, N, K);
            double bt20 = MeasureBruteForceTime(b, 20, N, K);

            Console.WriteLine($"Тестів, де жадібне дало оптимум: {optimalCount} / 100");
            Console.WriteLine($"Найгірше відставання:           {worstGapPct:F2}%");
            Console.WriteLine($"Найменший контрприклад:         {smallestCounterExample}");
            Console.WriteLine($"Час перебору:                   n=15 -> {bt15:F3} мс | n=20 -> {bt20:F3} мс");
        }

        static double MeasureBruteForceTime(int b, int nElements, int N, int K)
        {
            var rng = new Random(N + nElements);
            if (b == 0)
            {
                int[] coins = { (K % 4) + 4, (K % 4) + 3, 1 };
                int amount = 20 + K + nElements * 5;
                _ = GreedyAndBruteForce.CoinsBruteForce(10, coins);
                return MeasureMinMs(() => GreedyAndBruteForce.CoinsBruteForce(amount, coins));
            }
            else if (b == 1)
            {
                var items = new GreedyAndBruteForce.Interval[nElements];
                for (int i = 0; i < nElements; i++)
                {
                    int s = rng.Next(0, 50);
                    items[i] = new GreedyAndBruteForce.Interval(s, s + rng.Next(1, 15));
                }
                _ = GreedyAndBruteForce.IntervalsBruteForce(items);
                return MeasureMinMs(() => GreedyAndBruteForce.IntervalsBruteForce(items));
            }
            else if (b == 2)
            {
                int cap = 10 + K;
                var items = new GreedyAndBruteForce.KnapsackItem[nElements];
                for (int i = 0; i < nElements; i++)
                    items[i] = new GreedyAndBruteForce.KnapsackItem(rng.Next(2, cap / 2 + 2), rng.Next(5, 40));
                _ = GreedyAndBruteForce.KnapsackBruteForce(items, cap);
                return MeasureMinMs(() => GreedyAndBruteForce.KnapsackBruteForce(items, cap));
            }
            else
            {
                int rod = 30 + K;
                int[] parts = new int[nElements];
                for (int i = 0; i < nElements; i++)
                    parts[i] = rng.Next(4, rod / 2 + 3);
                _ = GreedyAndBruteForce.RodCutBruteForce(rod, parts);
                return MeasureMinMs(() => GreedyAndBruteForce.RodCutBruteForce(rod, parts));
            }
        }

        static void RunTaskC(string prose, int c, int K, int N)
        {
            Console.WriteLine($"\n--- ДОДАТКОВО (Таблиця C, c = {c}) ---");
            string prose50k = prose.Substring(0, 50_000);

            if (c == 0)
            {
                int topK = (K % 7) + 5;
                string x1 = prose50k;
                string x10 = string.Concat(System.Linq.Enumerable.Repeat(prose50k, 10));
                string x100 = string.Concat(System.Linq.Enumerable.Repeat(prose50k, 100));

                double t1 = MeasureMinMs(() => ExecuteFastTask2(x1, N % 3, topK));
                double t10 = MeasureMinMs(() => ExecuteFastTask2(x10, N % 3, topK));
                double t100 = MeasureMinMs(() => ExecuteFastTask2(x100, N % 3, topK));
                Console.WriteLine($"Швидке рішення з зав. 2: x1 (50k) = {t1:F3} мс | x10 (500k) = {t10:F3} мс | x100 (5M) = {t100:F3} мс");
            }
            else if (c == 1)
            {
                double arrMs = MeasureMinMs(() =>
                {
                    int[] freq = new int[33];
                    foreach (char ch in prose50k)
                    {
                        char low = char.ToLowerInvariant(ch);
                        int idx = Array.IndexOf(UkrAlphabet, char.ToUpperInvariant(low));
                        if (idx >= 0) freq[idx]++;
                    }
                });
                double dictMs = MeasureMinMs(() =>
                {
                    var dict = new Dictionary<char, int>();
                    foreach (char ch in prose50k)
                    {
                        char low = char.ToLowerInvariant(ch);
                        dict.TryGetValue(low, out int cnt);
                        dict[low] = cnt + 1;
                    }
                });
                Console.WriteLine($"Масив[33] = {arrMs:F4} мс | Dictionary<char,int> = {dictMs:F4} мс (відношення: {dictMs / arrMs:F1}x)");
            }
            else if (c == 2)
            {
                string pat = "українська";
                StringAlgorithms.CountPatternWithSubstring(prose50k, pat);
                StringAlgorithms.CountPatternByIndex(prose50k, pat);

                var (m1, created1) = StringAlgorithms.CountPatternWithSubstring(prose50k, pat);
                var (m2, created2) = StringAlgorithms.CountPatternByIndex(prose50k, pat);
                double subMs = MeasureMinMs(() => StringAlgorithms.CountPatternWithSubstring(prose50k, pat));
                double idxMs = MeasureMinMs(() => StringAlgorithms.CountPatternByIndex(prose50k, pat));

                Console.WriteLine($"Substring у циклі:        час = {subMs:F3} мс, створено нових рядків = {created1:N0}, збігів = {m1}");
                Console.WriteLine($"Порівняння по індексах:   час = {idxMs:F3} мс, створено нових рядків = {created2:N0}, збігів = {m2}");
                Console.WriteLine($"Виграш у часі без копіювання: {subMs / idxMs:F1}x");
            }
            else if (c == 3)
            {
                string tempFile = Path.GetTempFileName();
                var sb = new StringBuilder();
                for (int i = 0; i < 100_000; i++) sb.AppendLine($"Рядок номер {i} із текстом для перевірки пам'яті.");
                File.WriteAllText(tempFile, sb.ToString(), Encoding.UTF8);

                long memBefore1 = GC.GetTotalMemory(true);
                double splitMs = MeasureMinMs(() => { _ = File.ReadAllText(tempFile).Split('\n'); });
                long memAfter1 = GC.GetTotalMemory(false);

                long memBefore2 = GC.GetTotalMemory(true);
                double streamMs = MeasureMinMs(() =>
                {
                    using var sr = new StreamReader(tempFile);
                    int lines = 0;
                    while (sr.ReadLine() != null) lines++;
                });
                long memAfter2 = GC.GetTotalMemory(false);
                File.Delete(tempFile);

                Console.WriteLine($"File.ReadAllText().Split: {splitMs:F3} мс, дельта пам'яті ~{(memAfter1 - memBefore1) / 1024:N0} КБ");
                Console.WriteLine($"StreamReader.ReadLine:    {streamMs:F3} мс, дельта пам'яті ~{(memAfter2 - memBefore2) / 1024:N0} КБ");
            }
            else
            {
                Console.WriteLine("c=4: Порівняння гілок перебору з відсіканням і без реалізується через лічильник викликів у рекурсії.");
            }
        }

        static void RunEdgeCases(int a, int b, int K)
        {
            Console.WriteLine("\n--- ПЕРЕВІРКА КРАЙОВИХ ВИПАДКІВ ---");
            int topK = (K % 7) + 5;

            Console.WriteLine($"1. Порожній рядок (зав. 2): збіг = {RunTask2Pair("", a, topK)} | (зав. 3): NaiveIndexOf(\"\", \"abc\") = {StringAlgorithms.NaiveIndexOf("", "abc", out _)}");
            Console.WriteLine($"2. Один символ \"а\" (зав. 2): збіг = {RunTask2Pair("а", a, topK)}");
            Console.WriteLine($"3. Усі символи однакові \"ааааа\" (зав. 2): збіг = {RunTask2Pair("ааааа", a, topK)}");
            Console.WriteLine($"4. Пробіли на краях і подвійні \"  слово   тест  \" (зав. 2): збіг = {RunTask2Pair("  слово   тест  ", a, topK)}");
            Console.WriteLine($"5. Шаблон довший за текст (зав. 3): NaiveIndexOf(\"аб\", \"абвгд\") = {StringAlgorithms.NaiveIndexOf("аб", "абвгд", out _)}");
            Console.WriteLine($"6. Порожній шаблон (зав. 3): NaiveIndexOf(\"текст\", \"\") = {StringAlgorithms.NaiveIndexOf("текст", "", out _)}");

            if (b == 3)
            {
                int g0 = GreedyAndBruteForce.RodCutGreedy(30 + K, Array.Empty<int>());
                int bt0 = GreedyAndBruteForce.RodCutBruteForce(30 + K, Array.Empty<int>());
                int g1 = GreedyAndBruteForce.RodCutGreedy(30 + K, new[] { 10 });
                int bt1 = GreedyAndBruteForce.RodCutBruteForce(30 + K, new[] { 10 });
                Console.WriteLine($"7. Зав. 4 (0 деталей / 1 деталь): 0 шт -> greedy={g0}, bt={bt0} | 1 шт -> greedy={g1}, bt={bt1}");
            }
        }

        static double MeasureMinMs(Action action)
        {
            double best = double.MaxValue;
            for (int i = 0; i < 3; i++)
            {
                var sw = Stopwatch.StartNew();
                action();
                sw.Stop();
                if (sw.Elapsed.TotalMilliseconds < best) best = sw.Elapsed.TotalMilliseconds;
            }
            return best;
        }
    }
}