using System;
using System.Linq;
using System.Threading.Tasks;
using Client.Models;
using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Extensions.Configuration;

namespace Client.Workers
{
    public interface IBotConnector
    {
        Task<Chat> TalkToBot(string message);
    }

    public class BotConnector: IBotConnector
    {
        private readonly string botId;
        private readonly string appServiceKey;

        private string directlineUrl
               = @"https://directline.botframework.com";

        public BotConnector(IConfiguration configuration)
        {
            botId = configuration.GetSection(
                "BotId")?.Value;

            appServiceKey = configuration.GetSection(
                "AppServiceKey")?.Value;
        }

        public async Task<Chat> TalkToBot(string message)
        {
            var client = new DirectLineClient(appServiceKey);
            
            Conversation conversation =
                Current.Session["conversation"] as Conversation;

            // Try to get an existing watermark 
            // the watermark marks the last message we received
            string watermark =
                Current.Session["watermark"] as string;

            if (conversation == null)
            {
                // There is no existing conversation
                // start a new one
                conversation = await client.Conversations.StartConversationAsync();
            }

            // Use the text passed to the method (by the user)
            // to create a new message
            Activity userMessage = new Activity
            {
                From = new ChannelAccount(User.Identity.Name),
                Text = message,
                Type = ActivityTypes.Message
            };

            // Post the message to Bot
            await client.Conversations.PostActivityAsync(
                conversation.ConversationId, userMessage);

            // Get the response as a Chat object
            var chat =
                await ReadBotMessagesAsync(
                    client,
                    conversation.ConversationId,
                    watermark);

            // Save values
            Current.Session["conversation"] = conversation;

            Current.Session["watermark"] = chat.Watermark;

            
            return chat;
        }

        private async Task<Chat> ReadBotMessagesAsync(
            DirectLineClient client, string conversationId, string watermark)
        {
            var chat = new Chat();

            bool waitTillmessageReceived = false;

            while (!waitTillmessageReceived)
            {
                // Retrieve the activity set from the bot.
                var activitySet = await client.Conversations.GetActivitiesAsync(
                    conversationId, watermark);

                // Set the watermark to the message received
                watermark = activitySet?.Watermark;

                // Extract the activies sent from our bot.
                var activities = (from Activity in activitySet.Activities
                                  where Activity.From.Id == botId
                                  select Activity).ToList();

                // Analyze each activity in the activity set.
                foreach (Activity activity in activities)
                {
                    // Set the text response
                    // to the message text
                    chat.ChatResponse
                        += " "
                        + activity.Text.Replace("\n\n", "<br />");

                    // Are there any attachments?
                    if (activity.Attachments != null)
                    {
                        // Extract each attachment from the activity.
                        foreach (Attachment attachment in activity.Attachments)
                        {
                            switch (attachment.ContentType)
                            {
                                case "image/png":
                                    // Set the text response as an HTML link
                                    // to the image
                                    chat.ChatResponse
                                        += " "
                                        + attachment.ContentUrl;
                                    break;
                            }
                        }
                    }
                }

                // Mark messageReceived so we can break 
                // out of the loop
                waitTillmessageReceived = true;
            }

            // Set watermark on the Chat object that will be 
            // returned
            chat.Watermark = watermark;

            return chat;
        }
    }
}