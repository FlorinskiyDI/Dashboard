using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ResourceOwnerClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
            Console.ReadKey();
        }

      
        private static async Task MainAsync(string[] args)
        {
            
            // IdentityServer4 host
            var authority = "http://localhost:5100";
            // client data            
            var clientId = "ro.client";
            var clientSecret = "secret";
            // user data
            var userName = "SuperAdmin";
            var password = "SuperAdmin";
            var scope = "api1";

            // discover endpoints from metadata
            var disco = await DiscoveryClient.GetAsync(authority);
            // request token
            var tokenClient = new TokenClient(disco.TokenEndpoint, clientId, clientSecret);
            var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync(userName, password, scope);

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }
            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            // call api
            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);
            var response = await client.GetAsync("http://localhost:5200/api/Identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("User claims \n" + JArray.Parse(content));
            }
        }
    }
}
