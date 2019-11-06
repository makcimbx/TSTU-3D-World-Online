using SimpleJSON;

namespace TSTU.Core.Configuration
{
    public interface IConfigProvider
    {
        string ConfigKey { get; }
        void InitializeConfig(JSONNode config);
    }
}