using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using ZSpitz.Util;

namespace Periscope {
    public class ConfigContractResolver : DefaultContractResolver {
        private readonly bool keyedConfig;
        public ConfigContractResolver(bool keyedConfig = false) => this.keyedConfig = keyedConfig;
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) =>
            keyedConfig != member.HasAttribute<KeyedConfigPropertyAttribute>() ? 
                null! : 
                base.CreateProperty(member, memberSerialization);
    }
}
