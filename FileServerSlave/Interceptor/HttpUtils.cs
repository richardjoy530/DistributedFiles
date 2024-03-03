using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Formatting;
using System.Reflection;

namespace FileServerSlave.Interceptor
{
    public static class HttpUtils
    {
        private enum ParameterBinding
        {
            FromUriPath,
            FromBody
        }

        private static readonly JsonSerializerSettings JsonSerializationSettings = new()
        {
            Converters = { new StringEnumConverter() }
        };

        public static HttpRequestMessage BuildRequest(Uri baseUri, MethodInfo methodInfo, object[] args)
        {
            var path = methodInfo.GetRequestPath();

            var parameters = methodInfo
                .GetParameters()
                .Select((p, i) => (Binding: GetParameterBinding(p, path), Type: p.ParameterType, p.Name, Value: args[i]))
                .ToList();

            var pathParameters = parameters
                .Where(x => x.Binding == ParameterBinding.FromUriPath)
                .ToList();

            var uriBuilder = new UriBuilder(new Uri(baseUri, GetUriPath(path, pathParameters!)));

            var httpRequestMessage = new HttpRequestMessage(GetHttpMethod(methodInfo), uriBuilder.Uri);

            var bodyParameters = parameters
                .Where(x => x.Binding == ParameterBinding.FromBody)
                .ToList();

            if (bodyParameters.Count == 0)
            {
                return httpRequestMessage;
            }

            var bodyParameter = bodyParameters.Single();
            var formatter = new JsonMediaTypeFormatter
            {
                SerializerSettings = JsonSerializationSettings
            };
            httpRequestMessage.Content = new ObjectContent(bodyParameter.Type, bodyParameter.Value, formatter);

            return httpRequestMessage;
        }

        public static object? Deserialize(Type type, HttpResponseMessage httpResponseMessage)
        {
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }

            if (type == typeof(void))
            {
                return null;
            }

            if (type == typeof(HttpResponseMessage))
            {
                return httpResponseMessage;
            }

            return JsonConvert.DeserializeObject(GetResponseContent(), type, JsonSerializationSettings);

            string GetResponseContent() => httpResponseMessage.Content.ReadAsStringAsync().Result;
        }

        private static string GetRequestPath(this MemberInfo methodInfo)
        {
            var route = methodInfo?.GetCustomAttribute<RouteAttribute>()?.Template;

            if (!string.IsNullOrWhiteSpace(route))
            {
                return route;
            }

            return string.Empty;
        }

        private static ParameterBinding GetParameterBinding(ParameterInfo parameterInfo, string actionRoute)
        {
            if (actionRoute.Contains($"{{{parameterInfo.Name}}}"))
            {
                return ParameterBinding.FromUriPath;
            }

            return ParameterBinding.FromBody;
        }

        private static string GetUriPath(string actionRoute, IEnumerable<(ParameterBinding binding, Type type, string Name, object Value)> uriPathParameters)
        {
            foreach (var (_, type, name, value) in uriPathParameters)
            {
                if (value == null)
                {
                    throw new ArgumentNullException(name);
                }
                actionRoute = actionRoute.Replace($"{{{name}}}", WebUtility.UrlEncode(WebUtility.UrlEncode(value.ToString())));
            }

            return actionRoute;
        }

        private static HttpMethod GetHttpMethod(MemberInfo methodInfo)
        {
            if (methodInfo.GetCustomAttribute<HttpPostAttribute>() != null)
            {
                return HttpMethod.Post;
            }

            if (methodInfo.GetCustomAttribute<HttpPutAttribute>() != null)
            {
                return HttpMethod.Put;
            }

            if (methodInfo.GetCustomAttribute<HttpDeleteAttribute>() != null)
            {
                return HttpMethod.Delete;
            }

            return HttpMethod.Get;
        }
    }
}