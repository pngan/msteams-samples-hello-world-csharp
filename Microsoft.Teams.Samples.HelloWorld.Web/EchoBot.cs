using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    public class EchoBot
    {
        public async Task EchoMessage(ConnectorClient connector, Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                //var reply = activity.CreateReply("You said: " + activity.GetTextWithoutMentions());
                //await connector.Conversations.ReplyToActivityWithRetriesAsync(reply);
                Activity reply =
                    activity.CreateReply($"You sent {activity.GetTextWithoutMentions()} which was {activity.GetTextWithoutMentions().Length} characters");
                var msgToUpdate = await connector.Conversations.ReplyToActivityAsync(reply);
                var serviceUrl = activity.ServiceUrl;
                Observable
                    .Interval(TimeSpan.FromSeconds(1))
                    .Take(5)
                    .Subscribe(async x =>
                    {
                        using (var conn = new ConnectorClient(new Uri(serviceUrl)))
                        {
                            Activity updatedReply = activity.CreateReply($"Count = {x}");
                            await conn.Conversations.UpdateActivityAsync(reply.Conversation.Id, msgToUpdate.Id, updatedReply);
                        }
                    });
            }
        }
    }
    public class ProcessMessageBot
    {
        private const string IsLoggedInProperty = "IsLoggedIn";

        public async Task ProcessMessage(ConnectorClient connector, Activity activity)
        {
            StateClient stateClient = activity.GetStateClient();
            BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
            bool isLoggedIn = userData.GetProperty<bool>(IsLoggedInProperty);
            if (userData.Data == null)
            {
                userData.SetProperty(IsLoggedInProperty, false);
            }

            if (activity.Type == ActivityTypes.Message)
            {
                var replyMessage = string.Empty;

                if (string.Compare(activity.GetTextWithoutMentions(), "login", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (isLoggedIn)
                    {
                        replyMessage = "You are already logged in; no action needed.";
                    }
                    else
                    {
                        replyMessage = "You are now logged in.";
                        userData.SetProperty(IsLoggedInProperty, true);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    }
                }
                if (string.Compare(activity.GetTextWithoutMentions(), "logout", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (isLoggedIn)
                    {
                        replyMessage = "You have been logged out.";
                        userData.SetProperty(IsLoggedInProperty, false);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    }
                    else
                    {
                        replyMessage = "You are already logged out; no action needed.";
                    }
                }

                if (!string.IsNullOrEmpty(replyMessage))
                {
                    var reply = activity.CreateReply(replyMessage);
                    await connector.Conversations.ReplyToActivityWithRetriesAsync(reply);
                }
            }
        }
    }
}
