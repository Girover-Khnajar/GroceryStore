using System.Reflection;

namespace CQRS.Infrastructure.Options;

public class CqrsOptions : ICqrsOptions
{
    internal List<Assembly> AssembliesToScan { get; } = [];
    public PipelineOptions PipelineOptions { get; } = new PipelineOptions();

    public void ConfigurePipelineBehaviors(Action<ConfigurePipelineBehaviorsContext> configure)
    {
        var context = new ConfigurePipelineBehaviorsContext(PipelineOptions);
        
        configure(context);
    }

    public void RegisterServicesFromAssembly(Assembly assembly) 
                => AssembliesToScan.Add(assembly);
}
