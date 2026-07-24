using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERS.Core.Algorithms
{
    public class TF_IDF
    {
        private Dictionary<string, int> vocabulary = new();
        private Dictionary<string, int> docFreq = new();
        private int totalDocs = 0;

        private HashSet<string> stopWords;
        public bool IsFitted => vocabulary != null && vocabulary.Count > 0;
        public TF_IDF(IEnumerable<string> customStopWords = null)
        {
            if (customStopWords != null)
            {
                stopWords = new HashSet<string>(customStopWords, StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                var defaultStopWords = new[]
                {
                "і", "й", "та", "або", "чи", "а", "але", "зате", "проте", "однак",
                "що", "як", "щоб", "якщо", "в", "у", "на", "з", "із", "зі", "до", "за",
                "and", "or", "but", "if", "so", "because", "as", "until", "while", "of",
                "in", "to", "for", "with", "on", "at", "from", "by", "about", "the", "a", "an"
            };
                stopWords = new HashSet<string>(defaultStopWords, StringComparer.OrdinalIgnoreCase);
            }
        }

        public void Fit(IEnumerable<string> allDoc)
        {
            vocabulary.Clear();
            docFreq.Clear();
            totalDocs = 0;

            foreach (var doc in allDoc)
            {
                totalDocs++;
                var words = Tokenize(doc);
                var uniqueWordsInDoc = words.Distinct();

                foreach (var word in uniqueWordsInDoc)
                {
                    if (!docFreq.ContainsKey(word))
                    {
                        docFreq[word] = 0;
                    }
                    docFreq[word]++;
                }
            }

            int curIndex = 0;
            var sortedWords = docFreq.Keys.OrderBy(w => w).ToList();

            foreach (var word in sortedWords)
            {
                vocabulary[word] = curIndex++;
            }
        }

        public Dictionary<int, double> CreateVector(string text)
        {
            if (vocabulary.Count == 0)
                throw new InvalidOperationException("The dictionary is empty, use the Fit() method to fill it.");

            Dictionary<int, double> vector = new();

            var words = Tokenize(text);
            int totalWordsInDoc = words.Count;

            if (totalWordsInDoc == 0)
                return vector;

            var wordCountsInDoc = words
                .GroupBy(w => w)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var kvp in wordCountsInDoc)
            {
                string word = kvp.Key;
                int countInThisDoc = kvp.Value;

                if (vocabulary.TryGetValue(word, out int wordIndex))
                {
                    double tf = TF(countInThisDoc, totalWordsInDoc);
                    double idf = IDF(totalDocs, docFreq[word]);

                    vector[wordIndex] = tf * idf;
                }
            }

            return vector;
        }

        private double TF(int wordCountInDoc, int totalWordsInDoc)
        {
            return (double)wordCountInDoc / totalWordsInDoc;
        }

        private double IDF(int totalDocs, int docContainingWord)
        { 
            return Math.Log((double)(totalDocs + 1) / (docContainingWord + 1)) + 1.0;
        }

        private List<string> Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            var cleanText = new string(
                text.Where(c => !char.IsPunctuation(c)).ToArray()).ToLower();

            return cleanText
                        .Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(word => !stopWords.Contains(word))
                        .ToList();
        }
    }
}
