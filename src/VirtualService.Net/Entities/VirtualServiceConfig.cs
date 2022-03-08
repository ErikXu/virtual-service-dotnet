using k8s.Models;
using KubeOps.Operator.Entities;
using System.Collections.Generic;
using KubeOps.Operator.Entities.Annotations;

namespace VirtualService.Net.Entities
{
    [KubernetesEntity(Group = "extension.networking.istio.io", ApiVersion = "v1", Kind = "VirtualServiceConfig")]
    [KubernetesEntityShortNames("vsc")]
    public class VirtualServiceConfig : CustomKubernetesEntity<VirtualServiceConfigSpec, VirtualServiceConfigStatus>
    {

    }

    [Description("This is the VirtualServiceConfigSpec Description")]
    public class VirtualServiceConfigSpec
    {
        [Required]
        public string VirtualServiceName { get; set; } = string.Empty;

        [Required]
        public string Host { get; set; } = string.Empty;

        public List<HttpRoute> Http { get; set; } = new List<HttpRoute>();
    }

    [Description("This is the VirtualServiceConfigStatus Description")]
    public class VirtualServiceConfigStatus
    {
        public string Status { get; set; } = string.Empty;
    }

    public class HttpRoute
    {
        public string Name { get; set; } = string.Empty;

        public int Order { get; set; } = 0;

        public HttpMatchRequest Match { get; set; } = new HttpMatchRequest();

        public HttpRouteDestination Route { get; set; } = new HttpRouteDestination();
    }

    public class HttpMatchRequest
    {
        public string Name { get; set; } = string.Empty;

        public StringMatch Uri { get; set; } = new StringMatch();

        public IDictionary<string, StringMatch> Headers { get; set; } = new Dictionary<string, StringMatch>();
    }

    public class HttpRouteDestination
    {
        public string Host { get; set; } = string.Empty;

        public string Subset { get; set; } = string.Empty;
    }

    public class StringMatch
    {
        public string Exact { get; set; } = string.Empty;

        public string Prefix { get; set; } = string.Empty;

        public string Regex { get; set; } = string.Empty;
    }
}
