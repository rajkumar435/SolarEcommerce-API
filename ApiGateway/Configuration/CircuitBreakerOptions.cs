namespace ApiGateway.Configuration
{
    public class CircuitBreakerOptions
    {
        public int FailureThreshold { get; set; } = 3;

        public int BreakDurationSeconds { get; set; } = 30;
    }
}