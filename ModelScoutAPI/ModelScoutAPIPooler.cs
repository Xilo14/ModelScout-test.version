using System;
using System.Collections.Generic;

using System.Threading.Tasks;

namespace ModelScoutAPI {
    public static class ModelScoutAPIPooler {
        public static ModelScoutAPIOptions DefaultOptions { get; set; } = new ModelScoutAPIOptions() {
            CptchApiKey = "",
            CptchSoftId = "",
            DbConnectionString = "",
        };
        private static Dictionary<long, ModelScoutAPI> _pool = new Dictionary<long, ModelScoutAPI>();

        public static async Task<ModelScoutAPI> GetOrCreateApi(long ChatId, ModelScoutAPIOptions Options) {
            ModelScoutAPI api;
            lock (_pool) {
                if (!_pool.TryGetValue(ChatId, out api)) {

                    api = new ModelScoutAPI(Options, (int)ChatId);
                    _pool.Add(ChatId, api);

                }
            }
            return api;
        }
        public static async Task<ModelScoutAPI> GetOrCreateApi(long ChatId)
            => await GetOrCreateApi(ChatId, ModelScoutAPIPooler.DefaultOptions);

    }
}
