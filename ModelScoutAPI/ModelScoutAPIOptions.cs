using System;

namespace ModelScoutAPI {
    public class ModelScoutAPIOptions {
        public String DbConnectionString { get; set; } = null;
        public String CptchApiKey { get; set; } = null;
        public String CptchSoftId { get; set; } = null;
    }
}