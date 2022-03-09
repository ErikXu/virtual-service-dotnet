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

            await GenerateVirtualService(entity.Spec.VirtualServiceName, entity.Namespace(), entity.Spec.Host, configs);

            return null;
        }

        public Task StatusModifiedAsync(VirtualServiceConfig entity)
        {
            _logger.LogInformation($"[{entity.Namespace()}/{entity.Name()}] status modified.");

            return Task.CompletedTask;
        }

        public async Task DeletedAsync(VirtualServiceConfig entity)
        {
            _logger.LogInformation($"[{entity.Namespace()}/{entity.Name()}] deleted.");

            var configs = await _client.List<VirtualServiceConfig>(entity.Namespace());
            var preConfig = configs.FirstOrDefault(n => n.Name() == entity.Name());
            if (preConfig != null)
            {
                configs.Remove(preConfig);
            }

            if (configs.Count > 0)
            {
                await GenerateVirtualService(entity.Spec.VirtualServiceName, entity.Namespace(), entity.Spec.Host, configs);
            }
            else
            {
                await DeleteVirtualService(entity.Namespace(), entity.Spec.VirtualServiceName);
            }
        }

        private async Task GenerateVirtualService(string virtualServiceName, string @namespace, string host, IList<VirtualServiceConfig> configs)
        {
            var virtualService = ConfigsToVirtualService(virtualServiceName, @namespace, host, configs);

            var yaml = VirtualServiceToYaml(virtualService);
            _logger.LogInformation("Generated yaml:");
            _logger.LogInformation(yaml);

            await ApplyVirtualService(virtualService);
        }

        private Entities.VirtualService ConfigsToVirtualService(string virtualServiceName, string @namespace, string host, IList<VirtualServiceConfig> configs)
        {
            var virtualService = new Entities.VirtualService
            {
                ApiVersion = VirtualServiceStatic.ApiVersion,
                Kind = VirtualServiceStatic.Kind,
                Metadata = new Metadata
                {
                    Name = virtualServiceName,
                    Namespace = @namespace
                },
                Spec = new Spec
                {
                    Hosts = new List<string>
                    {
                        host
                    },
                    Http = new List<Http>()
                }
            };

            var specHttps = configs.SelectMany(n => n.Spec.Http)
                                   .OrderByDescending(n => n.Order)
                                   .ThenByDescending(n => StringMatchToUri(n.Match.Uri).Length)
                                   .ThenByDescending(n => n.Match.Headers.Count).ToList();

            foreach (var specHttp in specHttps)
            {
                var http = new Http
                {
                    Name = specHttp.Name,
                    Match = new List<Match>(),
                    Route = new List<Route>()
                };

                var match = new Match
                {
                    Name = specHttp.Match.Name,
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

            return virtualService;
        }

        private async Task ApplyVirtualService(Entities.VirtualService virtualService)
        {
            var config = KubernetesClientConfiguration.InClusterConfig();
            IKubernetes client = new Kubernetes(config);

            try
            {
                await client.PatchNamespacedCustomObjectAsync(new V1Patch(virtualService, V1Patch.PatchType.ApplyPatch),
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
                _logger.LogError($"Request Method:{ex.Request.Method}");
                _logger.LogError("Request Headers:");
                foreach (var h in ex.Request.Headers)
                {
                    _logger.LogError($"{h.Key}:{h.Value.First()}");
                }

                _logger.LogError($"Request Content:{ex.Request.Content}");
            }
        }

        private async Task DeleteVirtualService(string @namespace, string name)
        {
            var config = KubernetesClientConfiguration.InClusterConfig();
            IKubernetes client = new Kubernetes(config);

            try
            {
                await client.DeleteNamespacedCustomObjectAsync(VirtualServiceStatic.Group,
                                                               VirtualServiceStatic.Version,
                                                               @namespace,
                                                               VirtualServiceStatic.Plural,
                                                               name);
            }
            catch (HttpOperationException ex)
            {
                _logger.LogError($"Message:{ex.Message}");
                _logger.LogError($"Response Content:{ex.Response.Content}");

                _logger.LogError($"Request RequestUri:{ex.Request.RequestUri}");
                _logger.LogError($"Request Method:{ex.Request.Method}");
                _logger.LogError("Request Headers:");
                foreach (var h in ex.Request.Headers)
                {
                    _logger.LogError($"{h.Key}:{h.Value.First()}");
                }

                _logger.LogError($"Request Content:{ex.Request.Content}");
            }
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

        private string StringMatchToUri(StringMatch match)
        {
            if (!string.IsNullOrEmpty(match.Exact))
            {
                return match.Exact;
            }
            if (!string.IsNullOrEmpty(match.Prefix))
            {
                return match.Prefix;
            }

            return match.Regex;
        }
    }
}
