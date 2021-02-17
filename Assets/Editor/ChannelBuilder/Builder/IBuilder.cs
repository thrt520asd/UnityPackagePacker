using UnityEditor;

namespace ChannelBuilder.Builder
{
    public interface IBuilder
    {
        string Name { get; }

        void Prepare(ChannelConfig config, BuildTarget target, BuildCfg cfg);

        void Build(ChannelConfig config, BuildTarget target, BuildCfg cfg);
        void OnEndBuild(BuildTarget target, string pathToBuiltProject);

        void Simulate(ChannelConfig config, BuildTarget target, BuildCfg cfg);

        void RevertSimulate(ChannelConfig config, BuildTarget target, BuildCfg cfg);

        void OnFailed();
    }
}