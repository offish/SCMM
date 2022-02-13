﻿using SCMM.Market.Client;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SCMM.Market.Buff.Client
{
    public class BuffWebClient : AgentWebClient
    {
        private const string BaseUri = "https://buff.163.com/api/";

        public const int MaxPageLimit = 80;

        public BuffWebClient()
        {
            Cookies.Add(new Uri(BaseUri), new Cookie("session", "1-5FmUUBO4DK7Tf6Mc4veLwoRUqgwEZHariXisqhTsdRga2037021906"));
        }

        public async Task<BuffMarketGoodsResponse> GetMarketGoodsAsync(string appName, int page = 1, int pageSize = MaxPageLimit)
        {
            using (var client = GetHttpClient())
            {
                var url = $"{BaseUri}market/goods?game={appName.ToLower()}&page_num={page}&page_size={pageSize}&sort_by=price.desc&trigger=undefined_trigger&_={Random.Shared.NextInt64(1000000000000L, 9999999999999L)}";
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var textJson = await response.Content.ReadAsStringAsync();
                var responseJson = JsonSerializer.Deserialize<BuffResponse<BuffMarketGoodsResponse>>(textJson);
                return responseJson?.Data;
            }
        }
    }
}
