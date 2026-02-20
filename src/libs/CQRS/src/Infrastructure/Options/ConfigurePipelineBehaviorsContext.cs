namespace CQRS.Infrastructure.Options;

public sealed class ConfigurePipelineBehaviorsContext(PipelineOptions pipelineOptions)
{

    public void UseUnhandledException(bool use = true, int order = 0)
    {
        pipelineOptions.UseUnhandledException = use;
        pipelineOptions.OrderUnhandledException = order;
    }

    public void UseLogging(bool use = true, int order = 1000)
    {
        pipelineOptions.UseLogging = use;
        pipelineOptions.OrderLogging = order;
    }

    public void UsePerformance(bool use = true, int order = 2000)
    {
        pipelineOptions.UsePerformance = use;
        pipelineOptions.OrderPerformance = order;
    }

    public void UseAuthorization(bool use = true, int order = 3000)
    {
        pipelineOptions.UseAuthorization = use;
        pipelineOptions.OrderAuthorization = order;
    }

    public void UseValidation(bool use = true, int order = 4000)
    {
        pipelineOptions.UseValidation = use;
        pipelineOptions.OrderValidation = order;
    }
}
