using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Store bot state in Azure table
            var store = new TableBotDataStore(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            Conversation.UpdateContainer(
                builder =>
                {
                    builder.Register(c => store)
                        .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                        .AsSelf()
                        .SingleInstance();

                    builder.Register(c => new CachingBotDataStore(store,
                            CachingBotDataStoreConsistencyPolicy
                                .ETagBasedConsistency))
                        .As<IBotDataStore<BotData>>()
                        .AsSelf()
                        .InstancePerLifetimeScope();

                    builder.RegisterType<AgentStateSubscription>()
                        .As<IAgentStateSubscription>()
                        .As<IStartable>()
                        .SingleInstance();
                });
        }
    }


    public class AgentStateSubscription : IAgentStateSubscription, IStartable
    {
        public void Start()
        {
            var namespaceClient = 
                NamespaceManager.CreateFromConnectionString(ConfigurationManager.AppSettings["ServiceBusConnection"]);
            if (namespaceClient.SubscriptionExists(ConfigurationManager.AppSettings["ServiceBusTopic"], ConfigurationManager.AppSettings["BotSubscription"]) == false)
                namespaceClient.CreateSubscription(ConfigurationManager.AppSettings["ServiceBusTopic"], ConfigurationManager.AppSettings["BotSubscription"]);
            var subscriptionClient = SubscriptionClient.CreateFromConnectionString(
                ConfigurationManager.AppSettings["ServiceBusConnection"],
                ConfigurationManager.AppSettings["ServiceBusTopic"],
                ConfigurationManager.AppSettings["BotSubscription"]);
            subscriptionClient.OnMessageAsync(StateMessageHandler);

        }

        private async Task StateMessageHandler(BrokeredMessage message)
        {
            try
            {
                var agentStateChangeMessage = message.GetBody<AgentStateChange>();
                await AgentStateChangeMessageHandler(agentStateChangeMessage);
            }
            catch (Exception e)
            {
                //m_logger.Exception(e, $"Failed to handle agent state service bus pub message <{JsonConvert.SerializeObject(message)}>");
            }
        }

        private async Task AgentStateChangeMessageHandler(AgentStateChange message)
        {
            if (message == null || string.IsNullOrEmpty(message.Activity) || string.IsNullOrEmpty(message.StateChangeMessage))
                return;
            try
            {
                var activity = JsonConvert.DeserializeObject<Activity>(message.Activity);

                var reply = activity.CreateReply(message.StateChangeMessage);
                using (var connector = new ConnectorClient(new Uri(activity.ServiceUrl)))
                {
                    await connector.Conversations.ReplyToActivityWithRetriesAsync(reply);
                }
            }
            catch (Exception e)
            {
                // m_logger.Exception(e, $"Failed to extract tenantId from agent state service bus pub message <{JsonConvert.SerializeObject(message)}>");
            }
        }
    }

    public interface IAgentStateSubscription
    {
    }

    public class AgentStateChange
    {
        public string StateChangeMessage { get; set; }
        public string Activity { get; set; }
    }
}
