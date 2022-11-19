namespace I2R.Endpoints;

public static partial class SyncEndpoint<BaseEndpoint>
{
    public static class Req<TRequest>
    {
        public abstract class Res<TResponse>
        {
            public abstract TResponse Handle(
                TRequest request
            );
        }

        public abstract class NoRes
        {
            public abstract void Handle(
                TRequest request
            );
        }
    }

    public static class NoReq
    {
        public abstract class Res<TResponse>
        {
            public abstract TResponse Handle();
        }

        public abstract class NoRes
        {
            public abstract void Handle();
        }
    }
}