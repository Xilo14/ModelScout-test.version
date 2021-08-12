using System;

namespace ModelScoutAPI {
    public class ModelScoutAPIOptions {
        public const string ModelScout = "ModelScout";

        public string DbConnectionString { get; set; } = null;
        public String CptchApiKey { get; set; } = null;
        public String CptchSoftId { get; set; } = null;
    }
}
