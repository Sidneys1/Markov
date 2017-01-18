using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Markov {
    internal class Program {
        private static int Main() {
            Console.Write("Source file or precompiled markov file: ");
            var file = Console.ReadLine()?.Replace("\"", "") ?? string.Empty;
            if (!File.Exists(file)) {
                Console.WriteLine("Invalid file");
                return -1;
            }

            var markov = new Dictionary<string, Dictionary<string, int>>();

            int complexity;
            var isWords = false;
            using (var inFile = File.OpenText(file)) {
                var line = inFile.ReadLine();
                var isMarkov = int.TryParse(line, out complexity);
                if (isMarkov) {
                    line = inFile.ReadLine();
                }
                else {
                    Console.Write("Would you like to use words? (yes/no): ");
                    isWords = Console.ReadLine()?.ToLower() == "yes";

                    Console.Write("Please enter a complexity level: ");
                    complexity = int.Parse(Console.ReadLine() ?? "3");
                }
                markov.Add(string.Join(isWords ? " " : "", Enumerable.Repeat("~", complexity)),
                    new Dictionary<string, int>());
                do {
                    if (line != null)
                        if (isMarkov) {
                            var parts = line.Split('\t');
                            if (!markov.ContainsKey(parts[0]))
                                markov.Add(parts[0], new Dictionary<string, int>());
                            markov[parts[0]].Add(parts[1], int.Parse(parts[2]));
                        }
                        else {
                            if (isWords) {
                                var words = line.Split(' ');

                                var curr = string.Join(" ", Enumerable.Repeat("~", complexity));

                                if (!markov[curr].ContainsKey(words[0]))
                                    markov[curr].Add(words[0], 1);
                                else
                                    markov[curr][words[0]]++;

                                var split = curr.Split(' ').ToList();
                                split.Add(words[0].ToUpper());
                                curr = string.Join(" ", split.Skip(1));

                                for (var c = 1; c < words.Length; c++) {
                                    if (!markov.ContainsKey(curr))
                                        markov[curr] = new Dictionary<string, int>();

                                    var sub = words[c];
                                    if (!markov[curr].ContainsKey(sub))
                                        markov[curr].Add(sub, 1);
                                    else
                                        markov[curr][sub]++;

                                    split.Clear();
                                    split.AddRange(curr.Split(' '));
                                    split.Add(sub.ToUpper());
                                    curr = string.Join(" ", split.Skip(1));
                                }

                                if (!markov.ContainsKey(curr))
                                    markov.Add(curr, new Dictionary<string, int>());
                                if (!markov[curr].ContainsKey("~"))
                                    markov[curr].Add("~", 1);
                                else
                                    markov[curr]["~"]++;
                            }
                            else {
                                var curr = new string('~', complexity);
                                if (!markov[curr].ContainsKey(line.Substring(0, 1)))
                                    markov[curr].Add(line.Substring(0, 1), 1);
                                else
                                    markov[curr][line.Substring(0, 1)]++;

                                curr += line[0];
                                curr = curr.Substring(1).ToUpper();

                                for (var c = 1; c < line.Length; c++) {
                                    if (!markov.ContainsKey(curr))
                                        markov[curr] = new Dictionary<string, int>();

                                    var sub = line.Substring(c, 1);
                                    if (!markov[curr].ContainsKey(sub))
                                        markov[curr].Add(sub, 1);
                                    else
                                        markov[curr][sub]++;

                                    curr += line[c];
                                    curr = curr.Substring(1).ToUpper();
                                }

                                if (!markov.ContainsKey(curr))
                                    markov.Add(curr, new Dictionary<string, int>());
                                if (!markov[curr].ContainsKey("~"))
                                    markov[curr].Add("~", 1);
                                else
                                    markov[curr]["~"]++;
                            }
                        }
                    line = inFile.ReadLine();
                } while (!inFile.EndOfStream);

                if (!isMarkov) {
                    var outFile = Path.Combine(Path.GetDirectoryName(file), "markov.txt");
                    using (var o = File.CreateText(outFile)) {
                        o.WriteLine(complexity);
                        foreach (var j in markov)
                        foreach (var i in j.Value) o.WriteLine($"{j.Key}\t{i.Key}\t{i.Value}");
                        o.Flush();
                    }
                }
            }
            var r = new Random();
            do {
                Console.Clear();
                for (var i = 0; i < 25; i++)
                    Console.WriteLine(isWords
                        ? MakeMarkovSentence(r, markov, complexity)
                        : MakeMarkov(r, markov, complexity));
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            return 0;
        }

        private static string MakeMarkov(Random r, Dictionary<string, Dictionary<string, int>> markov, int complexity) {
            var b = new StringBuilder();

            var lastChar = new string('~', complexity);
            do {
                var opts = markov[lastChar].OrderBy(o => o.Value).ToArray();
                var selected = r.WeightedRandom(opts.Select(o => o.Value).ToList());
                var nextChar = opts[selected].Key;
                if (nextChar == "~")
                    break;
                b.Append(nextChar);

                lastChar += nextChar;
                lastChar = lastChar.Substring(1).ToUpper();
            } while (true);

            return b.ToString();
        }

        private static string MakeMarkovSentence(Random r, Dictionary<string, Dictionary<string, int>> markov,
            int complexity) {
            var b = new StringBuilder();

            var lastChar = string.Join(" ", Enumerable.Repeat("~", complexity));
            var words = new List<string>();
            do {
                var opts = markov[lastChar].OrderBy(o => o.Value).ToArray();
                var selected = r.WeightedRandom(opts.Select(o => o.Value).ToList());
                var nextChar = opts[selected].Key;
                if (nextChar == "~") break;
                b.Append(nextChar);
                b.Append(" ");

                words.Clear();
                words.AddRange(lastChar.Split(' '));
                words.Add(nextChar.ToUpper());
                lastChar = string.Join(" ", words.Skip(1));
            } while (true);

            return b.ToString();
        }
    }
}