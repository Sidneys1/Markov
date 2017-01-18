using System;
using System.Collections.Generic;

namespace Markov {
    public static class ExtensionMethods {
        /// Returns random index in weights list with probability based on its weight value.
        /// 
        /// 
        /// List of weights.
        /// 
        /// Throws an exception if weights is null.
        /// 
        /// Throws an exception if weights count is zero.
        /// Throws an exception if any weight is less than zero.
        /// 
        /// 
        /// Returned values are within range of zero and weights.Count (exclusive).
        /// Chance of returned value to be i is weights[i]/weights.Sum().
        /// Any weight can be equal to zero. Such index is never selected.
        /// < code>
        /// var weights=new List&lt;int&gt;(new int[]{2,3,5,0});
        /// int v=new Random().WeightedRandom(weights);
        /// 
        /// 20% chance for v==0
        /// 30% chance for v==1
        /// 50% chance for v==2
        /// 0% chance for v==3
        /// </code>
        public static int WeightedRandom(this Random rnd, IList<int> weights) {
            if (weights == null) throw new ArgumentNullException(nameof(weights));
            if (weights.Count == 0) throw new ArgumentOutOfRangeException(nameof(weights));
            var totalWeights = new List<int>();
            foreach (var t in weights) {
                if (t < 0)
                    throw new ArgumentOutOfRangeException(nameof(weights));
                var last = totalWeights.Count > 0 ? totalWeights[totalWeights.Count - 1] : 0;
                var w = checked(last + t);
                totalWeights.Add(w);
            }
            var totalRandom = rnd.Next(totalWeights[totalWeights.Count - 1]);
            for (var i = 0; i < weights.Count; i++) {
                if (weights[i] > totalRandom) return i;
                totalRandom -= weights[i];
            }
            throw new Exception();
        }
    }
}