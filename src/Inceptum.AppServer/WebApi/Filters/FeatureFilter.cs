using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Inceptum.AppServer.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Inceptum.AppServer.WebApi.Filters
{
    internal class FeatureFilter : ActionFilterAttribute
    {
        private readonly IConfigurationProviderFactory providerFactory;        

        public FeatureFilter(IConfigurationProviderFactory providerFactory)
        {
            this.providerFactory = providerFactory;                       
        }
        
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var jFeatures = JObject.Parse(providerFactory.Create().GetBundle("AppServer", "server.host"))["features"];

            var permissions = new Dictionary<Resource, string[]>()
            {
                { new Resource("Configurations", "Delete"), new [] { "full-access" }  },
                { new Resource("Configurations", "Create"), new [] { "full-access" }  },
                { new Resource("Configurations", "UpdateBundle"), new [] { "full-access" }  },
                { new Resource("Configurations", "CreateBundle"), new [] { "full-access" }  },
                { new Resource("Configurations", "DeleteBundle"), new [] { "full-access" }  },

                { new Resource("Instances", "Create"), new [] { "full-access", "service-management" }  },
                { new Resource("Instances", "Update"), new [] { "full-access", "service-management" }  },
                { new Resource("Instances", "Delete"), new [] { "full-access", "service-management" }  },
                { new Resource("Instances", "Start"), new [] { "full-access", "service-management" }  },
                { new Resource("Instances", "Debug"), new [] { "full-access", "service-management" }  },
                { new Resource("Instances", "Kill"), new [] { "full-access", "service-management" }  },
                { new Resource("Instances", "Stop"), new [] { "full-access", "service-management" }  },
                { new Resource("Instances", "Restart"), new [] { "full-access", "service-management" }  },
                { new Resource("Instances", "Command"), new [] { "full-access", "service-management" }  },
                { new Resource("Instances", "UpdateVersion"), new [] { "full-access", "service-management" }  },
            };

            if (jFeatures != null)
            {
                var features = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(jFeatures.ToString());
                var controller = actionContext.RequestContext.RouteData.Values["controller"].ToString();
                var action = actionContext.RequestContext.RouteData.Values["action"].ToString();

                var resource = new Resource(controller, action);

                var allowedFeatures = permissions.ContainsKey(resource) ? permissions[resource] : new string[0];
                var rolesForFeatures = allowedFeatures.SelectMany(f => features[f]).ToArray();

                if (rolesForFeatures.Any() && !rolesForFeatures.Any(Thread.CurrentPrincipal.IsInRole))
                {
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(new { message = "Недостаточно прав для совершения операции." }), Encoding.UTF8, "application/json")
                    });
                }                
            }                        
        }        
    }

    public class Resource
    {
        public string Controller { get; }
        public string Action { get; }

        public Resource(string controller, string action)
        {
            Controller = controller;
            Action = action;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            
            var r = obj as Resource;
            if (r == null)
            {
                return false;
            }

            return Controller.Equals(r.Controller, StringComparison.OrdinalIgnoreCase) && Action.Equals(r.Action, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hash = 17;                
                hash = hash * 23 + Controller.ToLower().GetHashCode();
                hash = hash * 23 + Action.ToLower().GetHashCode();                
                return hash;
            }
        }
    }
}