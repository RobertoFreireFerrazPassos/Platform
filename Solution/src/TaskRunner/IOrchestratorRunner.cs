namespace TaskRunner
{
    public interface IOrchestratorRunner
    {
        public object Run(HttpOrchestratorRunnerParams parameters);
    }
}
