using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERS.Core.Algorithms;

namespace ERS.Core.Tests
{
    public class TfIdfTests
    {
        [Fact]
        public void Test1()
        {
            var tfidf = new TF_IDF();
            var documents = new List<string>
            {
                "Python and SQL",
                "Python and C#"
            };


            tfidf.Fit(documents);
            var vector = tfidf.CreateVector("Python");

            Assert.NotEmpty(vector);
            Assert.True(vector.Count > 0);
        }
        [Fact]
        public void Test2()
        {
            var tfidf = new TF_IDF();
            tfidf.Fit(new List<string> { "Python, SQL!" });

            var vector = tfidf.CreateVector("PYTHON SQL");

            Assert.Equal(2, vector.Count);
        }
        [Fact]
        public void Test3()
        {
            var tfidf = new TF_IDF();
            tfidf.Fit(new List<string> { "Python", "SQL" });

            var vector = tfidf.CreateVector("Python and Java");

            Assert.Equal(1, vector.Count);
        }
    }
}
