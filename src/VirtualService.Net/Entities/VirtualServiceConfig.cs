using k8s.Models;
using KubeOps.Operator.Entities;

namespace VirtualService.Net.Entities
{
    [KubernetesEntity(Group = "extension.networking.istio.io", ApiVersion = "v1", Kind = "VirtualServiceConfig")]
    public class VirtualServiceConfig : CustomKubernetesEntity<VirtualServiceConfigSpec, VirtualServiceConfigStatus>
    {

    }

    public class VirtualServiceConfigSpec
    {
        public string Host { get; set; } = string.Empty;
    }

    public class VirtualServiceConfigStatus
    {
        public string Status { get; set; } = string.Empty;
    }
}
