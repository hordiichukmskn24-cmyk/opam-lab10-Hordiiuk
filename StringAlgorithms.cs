using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OpamLab10
{
    public static class StringAlgorithms
    {
        // =========================================================================
        // ЗАВДАННЯ 1: Три пастки
        // =========================================================================

        public static string ConcatWithPlusEquals(int count)
        {
            string s = "";
            for (int i = 0; i < count; i++)
            {
                s += 'а';
            }
            return s;
        }

        public static string ConcatWithStringBuilder(int count)
        {
            var sb = new StringBuilder(count);
            for (int i = 0; i < count; i++)
            {
                sb.Append('а');
            }
            return sb.ToString();
        }

        public static int ComparePairsToLower(string[] words, int iterations)
        {
            int equalCount = 0;
            int n = words.Length;
            for (int i = 0; i < iterations; i++)
            {
                string a = words[i % n];
                string b = words[(i + 1) % n];
                if (a.ToLower() == b.ToLower()) equalCount++;
            }
            return equalCount;
        }

        public static int ComparePairsOrdinalIgnoreCase(string[] words, int iterations)
        {
            int equalCount = 0;
            int n = words.Length;
            for (int i = 0; i < iterations; i++)
            {
                string a = words[i % n];
                string b = words[(i + 1) % n];
                if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase)) equalCount++;
            }
            return equalCount;
        }

        public static (string[] OrdinalSorted, string[] UkrainianSorted) CompareUkrainianSorting()
        {
            string[] wordsWithGhe =
            {
                "ґанок", "ганок", "ґрунт", "грунт", "ґудзик", "гудзик",
                "ґрати", "грати", "ґедзь", "гедзь", "ґава", "гава",
                "ґніт", "гніт", "ґуля", "гуля", "ґазда", "газда",
                "ґринджоли", "гринджоли"
            };

            string[] ord = (string[])wordsWithGhe.Clone();
            string[] ukr = (string[])wordsWithGhe.Clone();

            Array.Sort(ord, StringComparer.Ordinal);
            var ukrCulture = new CultureInfo("uk-UA");
            Array.Sort(ukr, StringComparer.Create(ukrCulture, ignoreCase: false));

            return (ord, ukr);
        }

        // =========================================================================
        // ЗАВДАННЯ 2: Ваша задача (таблиця A) — наївне та швидке рішення
        // =========================================================================

        // --- a = 0: Найдовший підрядок без повторів символів ---
        public static int LongestUniqueSubstringNaive(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            int maxLen = 0;
            for (int i = 0; i < s.Length; i++)
            {
                for (int j = i; j < s.Length; j++)
                {
                    bool allUnique = true;
                    for (int k = i; k < j; k++)
                    {
                        if (s[k] == s[j])
                        {
                            allUnique = false;
                            break;
                        }
                    }
                    if (!allUnique) break;
                    int len = j - i + 1;
                    if (len > maxLen) maxLen = len;
                }
            }
            return maxLen;
        }

        public static int LongestUniqueSubstringFast(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            var window = new HashSet<char>();
            int left = 0, maxLen = 0;

            for (int right = 0; right < s.Length; right++)
            {
                while (window.Contains(s[right]))
                {
                    window.Remove(s[left]);
                    left++;
                }
                window.Add(s[right]);
                int len = right - left + 1;
                if (len > maxLen) maxLen = len;
            }
            return maxLen;
        }

        // --- a = 1: K найчастіших слів у тексті ---
        public static List<string> ExtractWords(string text)
        {
            var words = new List<string>();
            if (string.IsNullOrEmpty(text)) return words;

            var sb = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (char.IsLetter(ch) || ch == '\'' || ch == '’' || ch == '-')
                {
                    sb.Append(char.ToLowerInvariant(ch));
                }
                else if (sb.Length > 0)
                {
                    words.Add(sb.ToString());
                    sb.Clear();
                }
            }
            if (sb.Length > 0) words.Add(sb.ToString());
            return words;
        }

        public static List<(string Word, int Count)> TopKWordsNaive(string text, int topK)
        {
            var words = ExtractWords(text);
            var unique = new List<(string Word, int Count)>();

            for (int i = 0; i < words.Count; i++)
            {
                bool seen = false;
                for (int u = 0; u < unique.Count; u++)
                {
                    if (string.Equals(unique[u].Word, words[i], StringComparison.Ordinal))
                    {
                        seen = true;
                        break;
                    }
                }
                if (seen) continue;

                int count = 0;
                for (int j = 0; j < words.Count; j++)
                {
                    if (string.Equals(words[j], words[i], StringComparison.Ordinal))
                        count++;
                }
                unique.Add((words[i], count));
            }

            var arr = unique.ToArray();
            Array.Sort(arr, (x, y) =>
            {
                int cmp = y.Count.CompareTo(x.Count);
                return cmp != 0 ? cmp : string.Compare(x.Word, y.Word, StringComparison.Ordinal);
            });

            int take = Math.Min(topK, arr.Length);
            var res = new List<(string, int)>(take);
            for (int i = 0; i < take; i++) res.Add(arr[i]);
            return res;
        }

        public static List<(string Word, int Count)> TopKWordsFast(string text, int topK)
        {
            var words = ExtractWords(text);
            var freq = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int i = 0; i < words.Count; i++)
            {
                freq.TryGetValue(words[i], out int c);
                freq[words[i]] = c + 1;
            }

            var arr = new (string Word, int Count)[freq.Count];
            int idx = 0;
            foreach (var kv in freq)
            {
                arr[idx++] = (kv.Key, kv.Value);
            }

            Array.Sort(arr, (x, y) =>
            {
                int cmp = y.Count.CompareTo(x.Count);
                return cmp != 0 ? cmp : string.Compare(x.Word, y.Word, StringComparison.Ordinal);
            });

            int take = Math.Min(topK, arr.Length);
            var res = new List<(string, int)>(take);
            for (int i = 0; i < take; i++) res.Add(arr[i]);
            return res;
        }

        // --- a = 2: Нормалізація (один регістр, прибрати зайві пробіли, зібрати слова) ---
        public static string NormalizeTextNaive(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            // Заміна переносів/табуляцій і стискання пробілів через Replace та Substring у циклі
            string res = text.ToLowerInvariant().Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ');

            while (res.StartsWith(" ", StringComparison.Ordinal))
                res = res.Substring(1);
            while (res.EndsWith(" ", StringComparison.Ordinal))
                res = res.Substring(0, res.Length - 1);

            int i = 0;
            while (i < res.Length - 1)
            {
                if (res[i] == ' ' && res[i + 1] == ' ')
                {
                    res = res.Substring(0, i) + res.Substring(i + 1);
                }
                else
                {
                    i++;
                }
            }
            return res;
        }

        public static string NormalizeTextFast(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            var sb = new StringBuilder(text.Length);
            bool inSpace = true; // щоб відкинути провідні пробіли

            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (char.IsWhiteSpace(ch))
                {
                    if (!inSpace)
                    {
                        sb.Append(' ');
                        inSpace = true;
                    }
                }
                else
                {
                    sb.Append(char.ToLowerInvariant(ch));
                    inSpace = false;
                }
            }

            if (sb.Length > 0 && sb[sb.Length - 1] == ' ')
                sb.Length--;

            return sb.ToString();
        }

        // =========================================================================
        // ЗАВДАННЯ 3: Власний наївний пошук підрядка з лічильником порівнянь
        // =========================================================================
        public static int NaiveIndexOf(string text, string pat, out int comparisons)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (pat == null) throw new ArgumentNullException(nameof(pat));

            comparisons = 0;
            if (pat.Length == 0) return 0;
            if (pat.Length > text.Length) return -1;

            int limit = text.Length - pat.Length;
            for (int i = 0; i <= limit; i++)
            {
                int j = 0;
                while (j < pat.Length)
                {
                    comparisons++;
                    if (text[i + j] != pat[j]) break;
                    j++;
                }
                if (j == pat.Length) return i;
            }
            return -1;
        }

        // =========================================================================
        // ДОДАТКОВО (Таблиця C, c = 2): Substring у циклі vs індекси без копіювання
        // =========================================================================
        public static (int Matches, int CreatedStrings) CountPatternWithSubstring(string text, string pat)
        {
            int matches = 0;
            int created = 0;
            int limit = text.Length - pat.Length;
            for (int i = 0; i <= limit; i++)
            {
                string slice = text.Substring(i, pat.Length);
                created++;
                if (string.Equals(slice, pat, StringComparison.Ordinal))
                    matches++;
            }
            return (matches, created);
        }

        public static (int Matches, int CreatedStrings) CountPatternByIndex(string text, string pat)
        {
            int matches = 0;
            int limit = text.Length - pat.Length;
            for (int i = 0; i <= limit; i++)
            {
                bool ok = true;
                for (int j = 0; j < pat.Length; j++)
                {
                    if (text[i + j] != pat[j])
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok) matches++;
            }
            return (matches, 0);
        }
    }
}