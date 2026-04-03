using Microsoft.Extensions.Http.Resilience;

namespace Pulse.ApiService.Extensions
{
    public static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// Remove already configured resilience handlers
        /// </summary>
        /// <param name="builder">The builder instance.</param>
        /// <returns>The value of <paramref name="builder" />.</returns>
        public static IHttpClientBuilder StopPollyTimeouts(
            this IHttpClientBuilder builder)
        {
            builder.ConfigureAdditionalHttpMessageHandlers(static (handlers, _) =>
            {
                for (int i = 0; i < handlers.Count;)
                {
                    if (handlers[i] is ResilienceHandler)
                    {
                        handlers.RemoveAt(i);
                        continue;
                    }
                    i++;
                }
            });
            return builder;
        }
    }
}
