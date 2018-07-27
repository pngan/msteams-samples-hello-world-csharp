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
        private bool m_isLoggedIn;
        public async Task ProcessMessage(ConnectorClient connector, Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                Activity reply = null;

                if (string.Compare(activity.GetTextWithoutMentions(), "login", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (m_isLoggedIn)
                    {
                        reply = activity.CreateReply($"You are already logged in; no action needed.");
                    }
                    else
                    {
                        reply = activity.CreateReply($"You are now logged in.");
                        m_isLoggedIn = true;
                    }
                }
                if (string.Compare(activity.GetTextWithoutMentions(), "logout", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (m_isLoggedIn)
                    {
                        reply = activity.CreateReply($"You have been logged out.");
                    }
                    else
                    {
                        reply = activity.CreateReply($"You are already logged out; no action needed.");
                        m_isLoggedIn = true;
                    }
                }
                if (reply != null)
                    await connector.Conversations.ReplyToActivityWithRetriesAsync(reply);
            }
        }
    }
}
