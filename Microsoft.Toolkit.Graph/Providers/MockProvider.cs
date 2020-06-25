// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Extensions;

namespace Microsoft.Toolkit.Graph.Providers
{
    /// <summary>
    /// Provider to connect to the example data set for Microsoft Graph. Useful for prototyping and samples.
    /// </summary>
    public class MockProvider : IProvider
    {
        private ProviderState _state = ProviderState.Loading;

        /// <inheritdoc/>
        public ProviderState State
        {
            get
            {
                return _state;
            }

            private set
            {
                var current = _state;
                _state = value;

                StateChanged?.Invoke(this, new StateChangedEventArgs(current, _state));
            }
        }

        /// <inheritdoc/>
        public GraphServiceClient Graph => new GraphServiceClient(
            "https://proxy.apisandbox.msdn.microsoft.com/svc?url=" + HttpUtility.HtmlEncode("https://graph.microsoft.com/beta/"),
            new DelegateAuthenticationProvider((requestMessage) =>
            {
                //// Temporary Workaround for https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues/59
                //// ------------------------
                var requestUri = requestMessage.RequestUri.ToString();
                var index = requestUri.IndexOf("&");
                if (index >= 0)
                {
                    requestMessage.RequestUri = new Uri(requestUri.Remove(index, 1).Insert(index, "?"));
                }

                //// End Workaround

                return this.AuthenticateRequestAsync(requestMessage);
            }));

        /// <inheritdoc/>
        public event EventHandler<StateChangedEventArgs> StateChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockProvider"/> class.
        /// </summary>
        /// <param name="signedIn">Whether the default state should be signedIn, defaults to true.</param>
        public MockProvider(bool signedIn = true)
        {
            if (signedIn)
            {
                State = ProviderState.SignedIn;
            }
            else
            {
                State = ProviderState.SignedOut;
            }
        }

        /// <inheritdoc/>
        public Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            request.AddSdkVersion();

            request.AddMockProviderToken();

            return Task.FromResult(0);
        }

        /// <inheritdoc/>
        public async Task LoginAsync()
        {
            State = ProviderState.Loading;
            await Task.Delay(3000);
            State = ProviderState.SignedIn;
        }

        /// <inheritdoc/>
        public async Task LogoutAsync()
        {
            State = ProviderState.Loading;
            await Task.Delay(3000);
            State = ProviderState.SignedOut;
        }
    }
}
