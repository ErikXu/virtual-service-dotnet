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
            configs.Add(entity);

            var virtualService = new Entities.VirtualService
            {
                ApiVersion = "networking.istio.io/v1alpha3",
                Kind = "VirtualService",
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

            var yaml = VirtualServiceToYaml(virtualService);
            _logger.LogInformation(yaml);
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
    }
}
