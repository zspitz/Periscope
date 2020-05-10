using Microsoft.VisualStudio.DebuggerVisualizers;
using System.IO;

namespace Periscope.Debuggee {
    public abstract class ObjectSourceBase<TConfig> : VisualizerObjectSource where TConfig : ConfigBase<TConfig> {
        public virtual string ConfigKey => "";
        public abstract object GetSerializationModel(TConfig config);

        public override void GetData(object target, Stream outgoingData) =>
            Serialize(outgoingData, ConfigKey);

        public override void TransferData(object target, Stream incomingData, Stream outgoingData) {
            var config = (TConfig)Deserialize(incomingData);
            var serializationModel = GetSerializationModel(config);
            Serialize(outgoingData, serializationModel);
        }
    }
}
