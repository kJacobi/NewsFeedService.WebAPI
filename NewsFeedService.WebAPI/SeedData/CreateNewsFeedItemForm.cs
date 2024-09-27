using System;
using Newtonsoft.Json;

namespace NewsFeedService.WebAPI.SeedData
{
    public class CreateNewsFeedItemForm
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("authorName")]
        public string AuthorName { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("dateCreated")]
        public DateTime DateCreated { get; set; }

        [JsonProperty("allowComments")]
        public bool AllowComments { get; set; }
    }
}
