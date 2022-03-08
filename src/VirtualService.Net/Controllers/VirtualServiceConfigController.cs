using k8s.Models;
using KubeOps.Operator.Controller;
using KubeOps.Operator.Controller.Results;
using KubeOps.Operator.Rbac;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VirtualService.Net.Entities;
using DotnetKubernetesClient;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using VirtualService.Net.Serialization;
using System.Collections.Generic;
using System.Linq;
using k8s;
using Microsoft.Rest;

namespace VirtualService.Net.Controllers
{
    [EntityRbac(typeof(VirtualServiceConfig), Verbs = RbacVerb.All)]
    public class VirtualServiceConfigController : IResourceController<VirtualServiceConfig>
    {
        private readonly ILogger<VirtualServiceConfigController> _logger;
        private readonly IKubernetesClient _client;

        public VirtualServiceConfigController(ILogger<VirtualServiceConfigController> logger, IKubernetesClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task<ResourceControllerResult?> ReconcileAsync(VirtualServiceConfig entity)
        {
            _logger.LogInformation($"[{entity.Namespace()}/{entity.Name()}] reconciled.");

            var configs = await _client.List<VirtualServiceConfig>(entity.Namespace());
            var preConfig = configs.FirstOrDefault(n => n.Name() == entity.Name());
            if (preConfig != null)
            {
                configs.Remove(preConfig);
            }

            configs.Add(entity);

            var virtualService = new Entities.VirtualService
            {
                ApiVersion = VirtualServiceStatic.ApiVersion,
                Kind = VirtualServiceStatic.Kind,
                Metadata = new Metadata
                {
                    Name = entity.Spec.VirtualServiceName,
                    Namespace = entity.Namespace()
                },
                Spec = new Spec
                {
                    Hosts = new List<string>
                    {
                        entity.Spec.Host
                    },
                    Http = new List<Http>()
                }
            };

            var specHttps = configs.SelectMany(n => n.Spec.Http)
                                   .OrderByDescending(n => n.Order)
                                   .ThenByDescending(n => n.Match.Headers.Count).ToList();

            foreach (var specHttp in specHttps)
            {
                var http = new Http
                {
                    Match = new List<Match>(),
                    Route = new List<Route>()
                };

                var match = new Match
                {
                    Uri = StringMatchToDic(specHttp.Match.Uri)
                };

                if (specHttp.Match.Headers != null && specHttp.Match.Headers.Count > 0)
                {
                    match.Headers = new Dictionary<string, Dictionary<string, string>>();
                    foreach (var header in specHttp.Match.Headers)
                    {
                        match.Headers[header.Key] = StringMatchToDic(header.Value);
                    }
                }

                http.Match.Add(match);

                var route = new Route
                {
                    Destination = new Destination
                    {
                        Host = specHttp.Route.Host,
                        Subset = !string.IsNullOrEmpty(specHttp.Route.Subset) ? specHttp.Route.Subset : null
                    }
                };

                http.Route.Add(route);

                virtualService.Spec.Http.Add(http);
            }

            var k8sConfig = KubernetesClientConfiguration.InClusterConfig();
            IKubernetes client = new Kubernetes(k8sConfig);

            try
            {
                client.PatchNamespacedCustomObject(new V1Patch(virtualService, V1Patch.PatchType.ApplyPatch),
                                   VirtualServiceStatic.Group,
                                   VirtualServiceStatic.Version,
                                   virtualService.Metadata.Namespace,
                                   VirtualServiceStatic.Plural,
                                   virtualService.Metadata.Name,
                                   null,
                                   "application/apply-patch");
            }
            catch (HttpOperationException ex)
            {
                _logger.LogError($"Message:{ex.Message}");
                _logger.LogError($"Response Content:{ex.Response.Content}");

                _logger.LogError($"Request RequestUri:{ex.Request.RequestUri}");
                _logger.LogError("Request Headers:");
                foreach (var h in ex.Request.Headers)
                {
                    _logger.LogError($"{h.Key}:{h.Value.First()}");
                }

                _logger.LogError($"Request Content:{ex.Request.Content}");
            }

            return null;
        }

        public Task StatusModifiedAsync(VirtualServiceConfig entity)
        {
            _logger.LogInformation($"entity {entity.Name()} called {nameof(StatusModifiedAsync)}.");

            return Task.CompletedTask;
        }

        public Task DeletedAsync(VirtualServiceConfig entity)
        {
            _logger.LogInformation($"entity {entity.Name()} called {nameof(DeletedAsync)}.");

            return Task.CompletedTask;
        }

        private string VirtualServiceToYaml(Entities.VirtualService virtualService)
        {
            var serializer = new SerializerBuilder()
                                 .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                 .WithEmissionPhaseObjectGraphVisitor(args => new YamlIEnumerableSkipEmptyObjectGraphVisitor(args.InnerVisitor))
                                 .Build();

            var yaml = serializer.Serialize(virtualService);

            return yaml;
        }

        private Dictionary<string, string> StringMatchToDic(StringMatch match)
        {
            if (!string.IsNullOrEmpty(match.Exact))
            {
                return new Dictionary<string, string> { { "exact", match.Exact } };
            }
            if (!string.IsNullOrEmpty(match.Prefix))
            {
                return new Dictionary<string, string> { { "prefix", match.Prefix } };
            }

            return new Dictionary<string, string> { { "regex", match.Regex } };
        }
    }
}
