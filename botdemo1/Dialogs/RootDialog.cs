using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace botdemo1.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private class QnAMakerResult
        {
            /// <summary>
            /// The top answer found in the QnA Service.
            /// </summary>
            [JsonProperty(PropertyName = "answer")]
            public string Answer { get; set; }

            /// <summary>
            /// The score in range [0, 100] corresponding to the top answer found in the QnA    Service.
            /// </summary>
            [JsonProperty(PropertyName = "score")]
            public double Score { get; set; }
        }

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // calculate something for us to return
            // int length = (activity.Text ?? string.Empty).Length;

            // return our reply to the user

            var response = PostRequest(activity);
            QnAMakerResult QnAResult;
            GetResult(response, out QnAResult);
            if (QnAResult.Score < 1) await context.PostAsync($"喔,不知道该怎么回答你.");
            else await context.PostAsync(QnAResult.Answer);

            //await context.PostAsync($"You sent {activity.Text} which was {length} characters");

            context.Wait(MessageReceivedAsync);
        }

        private string PostRequest(Activity MyQuestion)
        {
            string responseString = string.Empty;

            var query = MyQuestion.Text; //User Query
            var knowledgebaseId = "7608c808-26c7-49a7-ab86-ddf843f80224"; // Use knowledge base id created.
            var qnamakerSubscriptionKey = "b9da13d6adff4befa52ade2993e6b3a0"; //Use subscription key assigned to you.

            //Build the URI
            Uri qnamakerUriBase = new Uri("https://westus.api.cognitive.microsoft.com/qnamaker/v1.0");
            var builder = new UriBuilder($"{qnamakerUriBase}/knowledgebases/{knowledgebaseId}/generateAnswer");

            //Add the question as part of the body
            var postBody = $"{{\"question\": \"{query}\"}}";

            //Send the POST request
            using (WebClient client = new WebClient())
            {
                //Set the encoding to UTF8
                client.Encoding = System.Text.Encoding.UTF8;

                //Add the subscription key header
                client.Headers.Add("Ocp-Apim-Subscription-Key", qnamakerSubscriptionKey);
                client.Headers.Add("Content-Type", "application/json");
                responseString = client.UploadString(builder.Uri, postBody);

                return responseString;
            }
        }

        private void GetResult(string responseString, out QnAMakerResult result)
        {
            try
            {
                result = JsonConvert.DeserializeObject<QnAMakerResult>(responseString);
            }
            catch
            {
                throw new Exception("Unable to deserialize QnA Maker response string.");
            }
        }
    }
}