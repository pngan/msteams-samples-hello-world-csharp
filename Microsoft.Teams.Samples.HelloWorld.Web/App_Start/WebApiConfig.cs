using System.Configuration;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Json settings
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };

            // Web API configuration and services
            ConfigurationManager.AppSettings["MicrosoftAppId"] = "debbac0a-653b-45fc-94c9-2bc97b478695";
            ConfigurationManager.AppSettings["MicrosoftAppPassword"] = "pdnEKF3642;mitmPBGH2]^}";

            ConfigurationManager.AppSettings["ServiceBusConnection"] = "Endpoint=sb://pn-servicebusexpress.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=rUi6Uxzu9ONY3j1SLuNWvKERHi0Nmt2ktXeRXV5D4bc=";
            ConfigurationManager.AppSettings["ServiceBusTopic"] = "agentstatetopic";
            ConfigurationManager.AppSettings["BotSubscription"] = "agentstatebotsubscription";

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
