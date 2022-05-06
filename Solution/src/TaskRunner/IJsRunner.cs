namespace TaskRunner
{
    public interface IJsRunner
    {
        public Task<object?> RunAsync(JsRunnerParams parameters);
    }
}
