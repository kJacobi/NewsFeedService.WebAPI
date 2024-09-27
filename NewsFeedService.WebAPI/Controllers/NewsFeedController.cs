using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NewsFeedService.WebAPI.Data;
using NewsFeedService.WebAPI.Services;

namespace NewsFeedService.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsFeedController : ControllerBase
    {
        private readonly INewsFeedService _newsFeedService; 
        private readonly IMemoryCacheService _memoryCacheService;

        public NewsFeedController(INewsFeedService newsFeedService, IMemoryCacheService memoryCacheService)
        {
            _newsFeedService = newsFeedService;
            _memoryCacheService = memoryCacheService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var newsItem = _memoryCacheService.GetFromCache<NewsFeedItem>(id);

            if (newsItem == null)
            {
                newsItem = (await _newsFeedService.Get(new[] { id }, null))?.FirstOrDefault();
            }

            AddNewsFeedItemToCache(id, newsItem);
            return newsItem == null ? NotFound() : Ok(newsItem);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Filters filters)
        {
            var newsItems = new List<NewsFeedItem>();

            // Get all keys from cache removing any Id's to exclude
            var keys = _memoryCacheService.GetAllKeys().Except(filters?.ExcludeIds ?? Array.Empty<int>());
            
            if (keys.Any())
            {
                // get all from cached results then filter per incoming filters.
                foreach (var id in keys)
                {
                    newsItems.Add(_memoryCacheService.GetFromCache<NewsFeedItem>(id));
                }
                var queryable = newsItems.AsQueryable();
                newsItems = _newsFeedService.ApplyFilterToQueryable(queryable, null, filters).ToList();
            }

            // Combine the requested Id's to exclude from db lookup as well as anything already found in cache.
            var mergedExcludeIds = (keys.Union(filters.ExcludeIds ?? Enumerable.Empty<int>())).ToArray();
            filters.ExcludeIds = mergedExcludeIds;

            // Get anything from db w/ the above exclusions (ie. cache) to improve GetAll performance.
            var dbResult = await _newsFeedService.Get(null, filters);
            if (dbResult != null && dbResult.Any())
            {
                dbResult.ToList().ForEach(e => AddNewsFeedItemToCache(e.Id, e));        // add db results to cache
                newsItems.AddRange(dbResult);
            }

            return Ok(newsItems);
        }

        [HttpPost]
        public async Task<IActionResult> Add(NewsFeedItem newsFeedItem)
        {
            await _newsFeedService.Add(newsFeedItem);
            _memoryCacheService.AddToCache(newsFeedItem.Id, newsFeedItem);
            return Ok(newsFeedItem);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var newsFeedItem = (await _newsFeedService.Get(new[] { id }, null)).FirstOrDefault();
            if (newsFeedItem == null)
                return NotFound();

            _memoryCacheService.RemoveFromCache(newsFeedItem.Id);
            await _newsFeedService.Delete(newsFeedItem);
            return NoContent();
        }

        private void AddNewsFeedItemToCache(int key, NewsFeedItem newsItem)
        {
            if (newsItem != null)
            {
                _memoryCacheService.AddToCache(key, newsItem);
            }
        }
    }
}
