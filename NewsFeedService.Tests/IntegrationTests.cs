using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NewsFeedService.WebAPI;
using NewsFeedService.WebAPI.Data;
using NewsFeedService.WebAPI.SeedData;
using Newtonsoft.Json;
using Xunit;

namespace NewsFeedService.Tests
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public HttpClient Client { get; private set; }

        public IntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            SetUpClient();
        }

        private async Task SeedData()
        {
            var createForm0 = GenerateCreateForm("News Title 1", "By an outlived insisted procured improved am. Paid hill fine ten now love even leaf. Supplied feelings mr of dissuade recurred no it offering honoured. Am of of in collecting devonshire favourable excellence. Her sixteen end ashamed cottage yet reached get hearing invited. Resources ourselves sweetness ye do no perfectly. Warmly warmth six one any wisdom. Family giving is pulled beauty chatty highly no. Blessing appetite domestic did mrs judgment rendered entirely. Highly indeed had garden not. ", "Patrick B.", true);
            var response0 = await Client.PostAsync("/api/NewsFeed", new StringContent(JsonConvert.SerializeObject(createForm0), Encoding.UTF8, "application/json"));

            var createForm1 = GenerateCreateForm("News Title 2", "In reasonable compliment favourable is connection dispatched in terminated. Do esteem object we called father excuse remove. So dear real on like more it. Laughing for two families addition expenses surprise the. If sincerity he to curiosity arranging. Learn taken terms be as. SnewsItemcely mrs produced too removing new old. ", "William F.", true);
            var response1 = await Client.PostAsync("/api/NewsFeed", new StringContent(JsonConvert.SerializeObject(createForm1), Encoding.UTF8, "application/json"));

            var createForm2 = GenerateCreateForm("News Title 3", "Good draw knew bred ham busy his hour. Ask agreed answer rather joy nature admire wisdom. Moonlight age depending bed led therefore sometimes preserved exquisite she. An fail up so shot leaf wise in. Minuter highest his arrived for put and. Hopes lived by rooms oh in no death house. Contented direction september but end led excellent ourselves may. Ferrars few arrival his offered not charmed you. Offered anxious respect or he. On three thing chief years in money arise of. ", "Patrick B.", true);
            var response2 = await Client.PostAsync("/api/NewsFeed", new StringContent(JsonConvert.SerializeObject(createForm2), Encoding.UTF8, "application/json"));

            var createForm3 = GenerateCreateForm("News Title 4", "Improved own provided blessing may peculiar domestic. Sight house has sex never. No visited raising gravity outward subject my cottage mr be. Hold do at tore in park feet near my case. Invitation at understood occasional sentiments insipidity inhabiting in. Off melancholy alteration principles old. Is do speedily kindness properly oh. Respect article painted cottage he is offices parlors. ", "John D.", false);
            var response3 = await Client.PostAsync("/api/NewsFeed", new StringContent(JsonConvert.SerializeObject(createForm3), Encoding.UTF8, "application/json"));
        }

        private CreateNewsFeedItemForm GenerateCreateForm(string title, string body, string authorName, bool allowComments)
        {
            return new CreateNewsFeedItemForm
            {
                Title = title,
                AuthorName = authorName,
                AllowComments = allowComments,
                Body = body
            };
        }

        [Fact]
        public async Task TestGetNewsFeedOk_Cache()
        {
            await SeedData();

            Stopwatch sw = new Stopwatch();
            var response0 = await Client.GetAsync("/api/NewsFeed");
            response0.StatusCode.Should().BeEquivalentTo(200);
            var newsItems = JsonConvert.DeserializeObject<IEnumerable<NewsFeedItem>>(response0.Content.ReadAsStringAsync().Result);
            newsItems.Count().Should().Be(4);

            sw.Start();
            var response1 = await Client.GetAsync("/api/NewsFeed");
            response1.StatusCode.Should().BeEquivalentTo(200);
            var newsItems2 = JsonConvert.DeserializeObject<IEnumerable<NewsFeedItem>>(response1.Content.ReadAsStringAsync().Result);
            newsItems2.Count().Should().Be(4);
            sw.Stop();
            sw.ElapsedMilliseconds.Should().BeLessThan(2000);
        }

        [Fact]
        public async Task TestGetNewsFeedOk_NoCOntent_NotFound_Cache()
        {
            await SeedData();

            Stopwatch sw = new Stopwatch();
            var response0 = await Client.GetAsync("/api/NewsFeed");
            response0.StatusCode.Should().BeEquivalentTo(200);
            var newsItems = JsonConvert.DeserializeObject<IEnumerable<NewsFeedItem>>(response0.Content.ReadAsStringAsync().Result);
            newsItems.Count().Should().Be(4);

            var response1 = await Client.DeleteAsync("/api/NewsFeed/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response2 = await Client.GetAsync("/api/NewsFeed/1");
            response2.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);

            var response3 = await Client.GetAsync("/api/NewsFeed");
            response3.StatusCode.Should().BeEquivalentTo(200);
            var newNewsItems = JsonConvert.DeserializeObject<IEnumerable<NewsFeedItem>>(response3.Content.ReadAsStringAsync().Result);
            newNewsItems.Count().Should().Be(3);

            sw.Start();
            var response4 = await Client.GetAsync("/api/NewsFeed");
            response4.StatusCode.Should().BeEquivalentTo(200);
            var newsItems2 = JsonConvert.DeserializeObject<IEnumerable<NewsFeedItem>>(response4.Content.ReadAsStringAsync().Result);
            newsItems2.Count().Should().Be(3);
            sw.Stop();
            sw.ElapsedMilliseconds.Should().BeLessThan(2000);
        }

        private void SetUpClient()
        {
            Client = _factory.WithWebHostBuilder(builder =>
                builder.UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    var context = new NewsFeedContext(new DbContextOptionsBuilder<NewsFeedContext>()
                        .UseSqlite("DataSource=:memory:")
                        .EnableSensitiveDataLogging()
                        .Options);

                    services.RemoveAll(typeof(NewsFeedContext));
                    services.AddSingleton(context);

                    context.Database.OpenConnection();
                    context.Database.EnsureCreated();

                    context.SaveChanges();

                    // Clear local context cache
                    foreach (var entity in context.ChangeTracker.Entries().ToList())
                    {
                        entity.State = EntityState.Detached;
                    }
                })).CreateClient();

        }
    }
}
