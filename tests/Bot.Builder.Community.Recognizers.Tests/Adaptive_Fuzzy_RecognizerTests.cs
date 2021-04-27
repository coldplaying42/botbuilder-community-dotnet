using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Recognizers.Fuzzy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bot.Builder.Community.Recognizers.Tests
{
    [TestClass]
    public class Adaptive_Fuzzy_RecognizerTests
    {
        [TestMethod]
        [TestCategory("Recognizers")]
        public async Task AdaptiveFuzzyRecognizer_TestScores_DefaultOptions()
        {
            var adaptivFuzzyRecognizer = GetRecognizer();

            var dc = CreateContext("Gary Prety");

            var result = await adaptivFuzzyRecognizer.RecognizeAsync(dc, dc.Context.Activity, CancellationToken.None);

            Assert.IsNotNull(result, "Recognizer result should not be null");
            dynamic entities = result.Entities;
            Assert.IsNotNull(entities["$instance"]["Gary Pretty"]);
        }

        public static DialogContext CreateContext(string text)
        {
            var activity = Activity.CreateMessageActivity();
            activity.Text = text;
            return new DialogContext(new DialogSet(), new TurnContext(new TestAdapter(), (Activity)activity), new DialogState());
        }

        public static AdaptiveFuzzyRecognizer GetRecognizer() => new AdaptiveFuzzyRecognizer
        {
            Choices = new List<string>()
            {
                "Phil Coulson",
                "Peggy Carter",
                "Gary Pretty",
                "Peter Parker",
                "Tony Stark",
                "Bruce Banner",
                "Garry Pritti"
            },
            Threshold = 0.7,
            IgnoreCase = true,
            IgnoreNonAlphanumeric = true,
        };
    }
}