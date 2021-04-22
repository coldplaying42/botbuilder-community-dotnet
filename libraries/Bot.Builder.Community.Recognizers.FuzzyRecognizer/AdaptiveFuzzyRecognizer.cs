using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;
using System.Threading;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.TraceExtensions;

namespace Bot.Builder.Community.Recognizers.Fuzzy
{
    class AdaptiveFuzzyRecognizer : Recognizer
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Community.FuzzyRecognizer";

        /// <summary>
        /// Gets or sets the  to Threshold
        /// </summary>
        /// <value>
        /// Threshold
        /// </value>
        [JsonProperty("Threshold")]
        public NumberExpression Threshold { get; set; }

        /// <summary>
        /// Gets or sets the  to IgnoreNonAlphanumeric
        /// </summary>
        /// <value>
        /// IgnoreNonAlphanumeric
        /// </value>
        [JsonProperty("IgnoreNonAlphanumeric")]
        public BoolExpression IgnoreNonAlphanumeric { get; set; }

        /// <summary>
        /// Gets or sets the  to IgnoreCase
        /// </summary>
        /// <value>
        /// IgnoreCase
        /// </value>
        [JsonProperty("IgnoreCase")]
        public BoolExpression IgnoreCase { get; set; } = true;

        /// <summary>
        /// Gets or sets the  to choices
        /// </summary>
        /// <value>
        /// choices
        /// </value>
        [JsonProperty("choices")]
        public ArrayExpression<string> choices { get; set; } 

        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            // Identify matched intents
            var text = activity.Text ?? string.Empty;
            var locale = activity.Locale ?? "en-us";

            var recognizerResult = new RecognizerResult()
            {
                Text = text,
                Entities = new JObject()
            };

            if (string.IsNullOrWhiteSpace(text))
            {
                // nothing to recognize, return empty recognizerResult
                return recognizerResult;
            }

            dynamic entities = recognizerResult.Entities;

            var choices = this.choices.GetValue(dialogContext.State);
            var IgnoreNonAlphanumeric = this.IgnoreNonAlphanumeric.GetValue(dialogContext.State);
            var IgnoreCase = this.IgnoreCase.GetValue(dialogContext.State);
            var Threshold = this.Threshold.GetValue(dialogContext.State);

            if (choices == null)
                throw new ArgumentNullException(nameof(choices));

            var options = new FuzzyRecognizerOptions(Threshold, IgnoreCase, IgnoreNonAlphanumeric);
            var recognizer = new FuzzyRecognizer(options);
            var result = await recognizer.Recognize(choices, activity.Text);

            if (result != null)
            {

                entities["$instance"] = new JObject();
 
                foreach (var match in result.Matches)
                {
                    // add all ids with exact same start/end

                    dynamic instance = new JObject();
                    instance.startIndex = 0;
                    instance.endIndex = 0;
                    instance.score = match.Score;
                    instance.text = match.Choice;
                    entities["$instance"][match.Choice].Add(instance);
                }
            }

            // if no match return None intent
            recognizerResult.Intents.Add("None", new IntentScore() { Score = 1.0 });

            await dialogContext.Context.TraceActivityAsync(nameof(FuzzyRecognizer), JObject.FromObject(recognizerResult), "RecognizerResult", "FuzzyRecognizerResult", cancellationToken).ConfigureAwait(false);

            this.TrackRecognizerResult(dialogContext, "FuzzyRecognizerResult", this.FillRecognizerResultTelemetryProperties(recognizerResult, telemetryProperties), telemetryMetrics);

            return recognizerResult;
        }

    }
}
