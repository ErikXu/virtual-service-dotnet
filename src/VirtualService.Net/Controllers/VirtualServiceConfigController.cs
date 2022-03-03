using k8s.Models;
using KubeOps.Operator.Controller;
using KubeOps.Operator.Controller.Results;
using KubeOps.Operator.Rbac;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VirtualService.Net.Entities;
using Newtonsoft.Json;

namespace VirtualService.Net.Controllers
{
    [EntityRbac(typeof(VirtualServiceConfig), Verbs = RbacVerb.All)]
    public class VirtualServiceConfigController : IResourceController<VirtualServiceConfig>
    {
        private readonly ILogger<VirtualServiceConfigController> _logger;

        public VirtualServiceConfigController(ILogger<VirtualServiceConfigController> logger)
        {
            _logger = logger;
        }

        public Task<ResourceControllerResult?> ReconcileAsync(VirtualServiceConfig entity)
        {
            _logger.LogInformation($"entity {entity.Name()} called {nameof(ReconcileAsync)}.");

            var json = JsonConvert.SerializeObject(entity);
            _logger.LogInformation($"json {json}");

            return Task.FromResult<ResourceControllerResult?>(null);
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
    }
}
