using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERS.Core.Algorithms
{
    public class KNN
    {
        public List<string> GetRecommendations(
            string target,
            Dictionary<string, Dictionary<string, double>> allUsersRatings,
            int k,
            int topN)
        {

            if (!allUsersRatings.TryGetValue(target, out var targetСhar) || targetСhar.Count == 0)
            {
                return new List<string>();
            }

            var dis = new List<(string UserId, double Distance)>();

            foreach (var otherUser in allUsersRatings)
            {
                if (otherUser.Key == target) continue;

                double distance = Similarities.EuclidDistance(targetСhar, otherUser.Value);

                if (distance != double.MaxValue)
                {
                    dis.Add((otherUser.Key, distance));
                }
            }

            var topKNeighbors = dis
                .OrderBy(d => d.Distance)
                .Take(k)
                .ToList();

            if (topKNeighbors.Count == 0) return new List<string>();

            var candidates = new Dictionary<string, (double TotalRating, int Count)>();

            foreach (var neighbor in topKNeighbors)
            {
                var neighbors = allUsersRatings[neighbor.UserId];

                foreach (var courseRating in neighbors)
                {
                    string objId = courseRating.Key;
                    double rating = courseRating.Value;

                    if (!targetСhar.ContainsKey(objId))
                    {
                        if (!candidates.ContainsKey(objId))
                            candidates[objId] = (0, 0);

                        var cur = candidates[objId];
                        candidates[objId] = (cur.TotalRating + rating, cur.Count + 1);
                    }
                }
            }

            var rec = candidates
                .Select(c => new
                {
                    CourseId = c.Key,
                    PredictedRating = c.Value.TotalRating / c.Value.Count
                })
                .OrderByDescending(c => c.PredictedRating)
                .Take(topN)
                .Select(c => c.CourseId)
                .ToList();

            return rec;
        }
    }
}