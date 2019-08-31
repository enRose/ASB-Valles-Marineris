using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        // GET api/values
        private readonly MicrosoftAppCredentials appCredentials;

        public MessageController(IConfiguration configuration)
        {
            var appId = configuration.GetSection(
                MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;

            var pw = configuration.GetSection(
                MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value;

            appCredentials = new MicrosoftAppCredentials(appId, pw);
        }

        // POST api/values
        [HttpPost, Authorize(Roles = "Bot")]
        public virtual async Task<OkResult> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                // calculate something for us to return
                int length = (activity.Text ?? string.Empty).Length;

                await ReplyMessage(activity, $"You sent {activity.Text} which was {length} characters");
            }
            else
            {
                await HandleSystemMessage(activity);
            }

            return Ok();
        }

        private async Task<Activity> HandleSystemMessage(Activity activity)
        {
            switch (activity.Type)
            {
                case ActivityTypes.DeleteUserData:
                    // Implement user deletion here
                    // If we handle user deletion, return a real message
                    break;

                case ActivityTypes.ConversationUpdate:
                    // Handle conversation state changes, like members being added and removed
                    // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                    // Not available in all channels
                    break;

                case ActivityTypes.ContactRelationUpdate:
                    // Handle add/remove from contact lists
                    // Activity.From + Activity.Action represent what happened
                    break;

                case ActivityTypes.Typing:
                    // Handle knowing that the user is typing
                    break;

           

                default:
                    break;
            }

            return null;
        }

        /// <summary>
        /// Replies the message.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="message">The message.</param>
        private async Task ReplyMessage(Activity activity, string message)
        {
            var serviceEndpointUri = new Uri(activity.ServiceUrl);

            using (var connector = new ConnectorClient(
                serviceEndpointUri, appCredentials))
            {
                var reply = activity.CreateReply(message);

                await connector.Conversations.ReplyToActivityAsync(reply);
            }
        }
    }
}
