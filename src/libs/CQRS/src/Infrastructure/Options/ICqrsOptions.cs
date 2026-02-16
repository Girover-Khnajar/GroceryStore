using System.Reflection;

namespace CQRS.Infrastructure.Options;

public interface ICqrsOptions
{
    void RegisterServicesFromAssembly(Assembly assembly);
    void ConfigurePipelineBehaviors(Action<ConfigurePipelineBehaviorsContext> configure);
}
