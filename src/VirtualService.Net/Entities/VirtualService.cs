using System.Collections.Generic;

namespace VirtualService.Net.Entities
{
    public class VirtualService
    {
        public string ApiVersion { get; set; } = "networking.istio.io/v1alpha3";

        public string Kind { get; set; } = "VirtualService";

        public Metadata Metadata { get; set; } = new Metadata();

        public Spec Spec { get; set; } = new Spec();
    }

    public class Metadata
    {
        public string Name { get; set; } = string.Empty;

        public string Namespace { get; set; } = string.Empty;
    }

    public class Spec
    {
        public List<string> Hosts { get; set; } = new List<string>();

        public List<Http> Http { get; set; } = new List<Http>();
    }

    public class Http
    {
        public string? Name { get; set; } = null;

        public List<Match> Match { get; set; } = new List<Match>();

        public List<Route> Route { get; set; } = new List<Route>();
    }

    public class Match
    {
        public string? Name { get; set; } = null;

        public Dictionary<string, string> Uri { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, Dictionary<string, string>>? Headers { get; set; } = null;
    }

    public class Route
    {
        public Destination Destination { get; set; } = new Destination();
    }

    public class Destination
    {
        public string Host { get; set; } = string.Empty;

        public string? Subset { get; set; } = null;
    }
}
