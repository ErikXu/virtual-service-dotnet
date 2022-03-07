namespace VirtualService.Net.Entities
{
    public class VirtualServiceStatic
    {
        public static string ApiVersion { get; set; } = "networking.istio.io/v1alpha3";

        public static string Kind { get; set; } = "VirtualService";

        public static string Group { get; set; } = "networking.istio.io";

        public static string Version { get; set; } = "v1alpha3";

        public static string Plural { get; set; } = "virtualservices";
    }
}
