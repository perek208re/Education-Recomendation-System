using ERS.Core.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERS.Core.Tests
{
    public class KNN
    {
        Algorithms.KNN knn;

        public KNN()
        {
            knn = new Algorithms.KNN();
        }


        /// <summary>
        /// Перевірка для нового користувача.
        /// Вхід: Користувач, якого немає в матриці.
        /// Вихід: Пустий список рекомендацій (немає бази для порівняння).
        /// </summary>
        [Fact]
        public void Test1()
        {
            var matrix = new Dictionary<string, Dictionary<string, double>>
            {
                { "user_other", new Dictionary<string, double> { { "course1", 5.0 } } }
            };

            string targetUser = "user_new";

            var result = knn.GetRecommendations(targetUser, matrix, k: 5, topN: 5);

            Assert.Empty(result);
        }


        /// <summary>
        /// Перевірка вибору найкращого сусіда.
        /// Вхід: Цільовий користувач, ідеально схожий сусід та поганий сусід (k=1, topN=1).
        /// Вихід: Отримання курсу саме від ідеального сусіда.
        /// </summary>
        [Fact]
        public void Test2()
        {
            string targetUser = "user_target";
            var matrix = new Dictionary<string, Dictionary<string, double>>
            {
                {
                    targetUser,
                    new Dictionary<string, double> { { "course_A", 5.0 }, { "course_B", 4.0 } }
                },
                {
                    "user_perfect_neighbor",
                    new Dictionary<string, double> { { "course_A", 5.0 }, { "course_B", 4.0 }, { "course_C", 5.0 } }
                },
                {
                    "user_bad_neighbor",
                    new Dictionary<string, double> { { "course_A", 1.0 }, { "course_B", 1.0 }, { "course_D", 5.0 } }
                }
            };

            var result = knn.GetRecommendations(targetUser, matrix, k: 1, topN: 1);

            Assert.Single(result);

            Assert.Equal("course_C", result.First());
        }

        /// <summary>
        /// Фільтрація вже пройдених курсів.
        /// Вхід: Користувач та сусід, які мають спільний курс.
        /// Вихід: Рекомендація містить лише НОВИЙ курс і не дублює вже оцінений.
        /// </summary>
        [Fact]
        public void Test3()
        {
            string targetUser = "user_target";
            var matrix = new Dictionary<string, Dictionary<string, double>>
            {
                {
                    targetUser,
                    new Dictionary<string, double> { { "course_A", 5.0 } }
                },
                {
                    "user_neighbor",
                    new Dictionary<string, double> { { "course_A", 5.0 }, { "course_B", 5.0 } }
                }
            };


            var result = knn.GetRecommendations(targetUser, matrix, k: 5, topN: 5);

            Assert.Single(result);
            Assert.DoesNotContain("course_A", result);
            Assert.Contains("course_B", result);
        }
    }
}
