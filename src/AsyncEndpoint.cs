namespace I2R.Endpoints;

public static partial class AsyncEndpoint<BaseEndpoint>
{
    public static class Req<TRequest>
    {
        public abstract class Res<TResponse>
        {
            public abstract Task<TResponse> HandleAsync(
                TRequest request,
                CancellationToken cancellationToken = default
            );
        }

        public abstract class NoRes
        {
            public abstract Task HandleAsync(
                TRequest request,
                CancellationToken cancellationToken = default
            );
        }
    }

    public static class NoReq
    {
        public abstract class Res<TResponse>
        {
            public abstract Task<TResponse> HandleAsync(
                CancellationToken cancellationToken = default
            );
        }

        public abstract class NoRes
        {
            public abstract Task HandleAsync(
                CancellationToken cancellationToken = default
            );
        }
    }
}