using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.ProjectOxford.Common
{
    public class Emotion
    {
        /// <summary>
        /// 
        /// </summary>
        public float Anger { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Contempt { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Disgust { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Fear { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Happiness { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Neutral { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Sadness { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Surprise { get; set; }

        /// <summary>
        /// Create a sorted key-value pair of emotions and the corresponding scores, sorted from highest score on down.
        /// To make the ordering stable, the score is the primary key, and the name is the secondary key.
        /// </summary>
        public IEnumerable<KeyValuePair<string, float>> ToRankedList()
        {
            return new Dictionary<string, float>()
            {
                { "Anger", Anger },
                { "Contempt", Contempt },
                { "Disgust", Disgust },
                { "Fear", Fear },
                { "Happiness", Happiness },
                { "Neutral", Neutral },
                { "Sadness", Sadness },
                { "Surprise", Surprise }
            }
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key)
            .ToList();
        }

        #region overrides
        public override bool Equals(object o)
        {
            if (o == null) return false;

            var other = o as Emotion;
            if (other == null) return false;

            return this.Anger == other.Anger &&
                this.Disgust == other.Disgust &&
                this.Fear == other.Fear &&
                this.Happiness == other.Happiness &&
                this.Neutral == other.Neutral &&
                this.Sadness == other.Sadness &&
                this.Surprise == other.Surprise;
        }

        public override int GetHashCode()
        {
            return Anger.GetHashCode() ^
                Disgust.GetHashCode() ^
                Fear.GetHashCode() ^
                Happiness.GetHashCode() ^
                Neutral.GetHashCode() ^
                Sadness.GetHashCode() ^
                Surprise.GetHashCode();
        }
        #endregion;
    }
}
