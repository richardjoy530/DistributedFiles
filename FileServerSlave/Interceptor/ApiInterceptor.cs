using Castle.DynamicProxy;

namespace FileServerSlave.Interceptor
{
    public class ApiInterceptor : IInterceptor
    {
        private static readonly HttpClient HttpClient = new();

        private readonly Uri _baseUri;

        private ApiInterceptor(HostString host, bool https)
        {
            _baseUri = new UriBuilder()
            {
                Scheme = https ? Uri.UriSchemeHttps : Uri.UriSchemeHttp,
                Host = host.Host,
                Port = host.Port ?? default,
            }.Uri;
        }

        public void Intercept(IInvocation invocation)
        {
            var requestMessage = HttpUtils.BuildRequest(_baseUri, invocation.Method, invocation.Arguments);

            var responseMessage = HttpClient.SendAsync(requestMessage).Result;

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new InvalidDataException(responseMessage.ReasonPhrase);
            }

            var returnValue = HttpUtils.Deserialize(invocation.Method.ReturnType, responseMessage);

            invocation.ReturnValue = returnValue;
        }

        public static T GetController<T>(HostString hostString, bool https)
        {
            var proxyGen = new ProxyGenerator();
            return (T)proxyGen.CreateInterfaceProxyWithoutTarget(typeof(T), new ApiInterceptor(hostString, https));
        }
    }
}
