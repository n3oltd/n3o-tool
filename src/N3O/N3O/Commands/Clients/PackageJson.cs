using Newtonsoft.Json;
using System.Collections.Generic;

namespace N3O.Commands.Clients {
    public class PackageJson {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public Dictionary<string, string> Dependencies { get; set; }
        public string License { get; set; }
        public string Main { get; set; }
        public string Module { get; set; }
        public string Types { get; set; }
        public Dictionary<string, string> DevDependencies { get; set; }
        public Repository Repository { get; set; }
        public IEnumerable<string> Files { get; set; }
        public Dictionary<string, string> Scripts { get; set; }
        public bool SideEffects { get; set; }
    }

    public class Repository {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
