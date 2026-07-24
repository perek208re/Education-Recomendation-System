using System.Collections.Generic;
using Xunit;
using ERS.Core;
using ERS.Core.Algorithms;

namespace ERS.Core.Tests
{
    public class SimilaritiesTests
    {
        /// <summary>
        /// Косинусна схожість: ідентичні вектори.
        /// Вхід: Два однакові вектори з ідентичними ключами та значеннями.
        /// Вихід: Схожість рівна 1.0 (максимальна колінеарність).
        /// </summary>
        [Fact]
        public void CosSimilarity1()
        {
            var vA = new Dictionary<int, double> { { 1, 0.5 }, { 2, 0.8 } };
            var vB = new Dictionary<int, double> { { 1, 0.5 }, { 2, 0.8 } };

            double result = Similarities.CosSimilarity(vA, vB);
            Assert.Equal(1.0, result, 4);
        }

        /// <summary>
        /// Косинусна схожість: пусті вектори.
        /// Вхід: Два абсолютно пусті словники.
        /// Вихід: Схожість рівна 0.0.
        /// </summary>
        [Fact]
        public void CosSimilarity2()
        {
            var vA = new Dictionary<int, double>();
            var vB = new Dictionary<int, double>();

            double result = Similarities.CosSimilarity(vA, vB);
            Assert.Equal(0.0, result, 4);
        }

        /// <summary>
        /// Евклідова відстань: немає спільних курсів.
        /// Вхід: Користувачі з повністю різними списками оцінених курсів.
        /// Вихід: Максимально можливе значення (double.MaxValue), що означає відсутність схожості.
        /// </summary>
        [Fact]
        public void EuclidDistance1()
        {
            var userA = new Dictionary<string, double> { { "course1", 5.0 } };
            var userB = new Dictionary<string, double> { { "course2", 4.0 } };

            double result = Similarities.EuclidDistance(userA, userB);

            Assert.Equal(double.MaxValue, result);
        }

        /// <summary>
        /// Евклідова відстань: повний збіг за спільними курсами.
        /// Вхід: Користувачі мають однакові оцінки на спільних курсах, але у одного є додатковий курс.
        /// Вихід: Відстань рівна 0.0, оскільки розрахунок йде лише за перетином елементами.
        /// </summary>
        [Fact]
        public void EuclidDistance2()
        {
            var userA = new Dictionary<string, double> { { "course1", 5.0 }, { "course2", 4.0 } };
            var userB = new Dictionary<string, double> { { "course1", 5.0 }, { "course2", 4.0 }, { "course3", 1.0 } };

            double result = Similarities.EuclidDistance(userA, userB);

            Assert.Equal(0.0, result, 4);
        }

        /// <summary>
        /// Евклідова відстань: різниця в оцінках на одному курсі.
        /// Вхід: Один спільний курс з оцінками 5.0 та 2.0.
        /// Вихід: Відстань рівна 3.0 (модуль різниці оцінок: |5.0 - 2.0|).
        /// </summary>
        [Fact]
        public void EuclidDistance3()
        {

            var userA = new Dictionary<string, double> { { "course1", 5.0 } };
            var userB = new Dictionary<string, double> { { "course1", 2.0 } };

            double result = Similarities.EuclidDistance(userA, userB);

            Assert.Equal(3.0, result, 4);
        }
    }
}