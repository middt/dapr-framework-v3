using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Dapr.Framework.Domain.Caching;

namespace Products.Api.Controllers
{
    /// <summary>
    /// Controller demonstrating distributed caching operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CacheController : ControllerBase
    {
        private readonly IDistributedCacheService _cacheService;

        public CacheController(IDistributedCacheService cacheService)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        /// <summary>
        /// Set a simple string value in the cache
        /// </summary>
        [HttpPost("simple")]
        public async Task<IActionResult> SetSimpleValue(
            [FromQuery] string key, 
            [FromQuery] string value, 
            [FromQuery] int? expirySeconds = null)
        {
            var options = expirySeconds.HasValue 
                ? DistributedCacheEntryOptions.WithSlidingExpiration(TimeSpan.FromSeconds(expirySeconds.Value))
                : null;

            await _cacheService.SetAsync(key, value, options);
            return Ok();
        }

        /// <summary>
        /// Get a simple string value from the cache
        /// </summary>
        [HttpGet("simple")]
        public async Task<IActionResult> GetSimpleValue([FromQuery] string key)
        {
            var value = await _cacheService.GetAsync<string>(key);
            
            return value != null 
                ? Ok(value) 
                : NotFound();
        }

        /// <summary>
        /// Set a complex object in the cache
        /// </summary>
        [HttpPost("complex")]
        public async Task<IActionResult> SetComplexObject(
            [FromQuery] string key, 
            [FromBody] ComplexCacheObject value, 
            [FromQuery] int? expirySeconds = null)
        {
            var options = expirySeconds.HasValue 
                ? DistributedCacheEntryOptions.WithSlidingExpiration(TimeSpan.FromSeconds(expirySeconds.Value))
                : DistributedCacheEntryOptions.WithSlidingExpiration(TimeSpan.FromMinutes(30));

            await _cacheService.SetAsync(key, value, options);
            return Ok();
        }

        /// <summary>
        /// Get a complex object from the cache
        /// </summary>
        [HttpGet("complex")]
        public async Task<IActionResult> GetComplexObject([FromQuery] string key)
        {
            var value = await _cacheService.GetAsync<ComplexCacheObject>(key);
            
            return value != null 
                ? Ok(value) 
                : NotFound();
        }

        /// <summary>
        /// Remove an item from the cache
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> RemoveCacheItem([FromQuery] string key)
        {
            await _cacheService.RemoveAsync(key);
            return Ok();
        }

        /// <summary>
        /// Refresh a cache item's sliding expiration
        /// </summary>
        [HttpPut("refresh")]
        public async Task<IActionResult> RefreshCacheItem([FromQuery] string key)
        {
            await _cacheService.RefreshAsync(key);
            return Ok();
        }

        /// <summary>
        /// Demonstrate GetOrSetAsync with a simple string value
        /// </summary>
        [HttpGet("getset/simple")]
        public async Task<IActionResult> GetOrSetSimpleValue(
            [FromQuery] string key, 
            [FromQuery] string? defaultValue = null)
        {
            var value = await _cacheService.GetOrSetAsync(
                key, 
                () => Task.FromResult(defaultValue ?? $"Generated value for {key} at {DateTime.UtcNow}")
            );

            return Ok(value);
        }

        /// <summary>
        /// Demonstrate GetOrSetAsync with a complex object
        /// </summary>
        [HttpGet("getset/complex")]
        public async Task<IActionResult> GetOrSetComplexObject(
            [FromQuery] string key, 
            [FromQuery] string? name = null)
        {
            var value = await _cacheService.GetOrSetAsync(
                key, 
                () => Task.FromResult(new ComplexCacheObject
                {
                    Name = name ?? $"Generated Object {key}",
                    Tags = new[] { "demo", "getset" },
                    Metadata = new Dictionary<string, string>
                    {
                        ["created"] = DateTime.UtcNow.ToString("o"),
                        ["source"] = "GetOrSetAsync demo"
                    }
                }),
                DistributedCacheEntryOptions.WithSlidingExpiration(TimeSpan.FromMinutes(30))
            );

            return Ok(value);
        }

        /// <summary>
        /// Demonstrate GetOrSetAsync with a custom key generation
        /// </summary>
        [HttpGet("getset/custom-key")]
        public async Task<IActionResult> GetOrSetWithCustomKey(
            [FromQuery] int userId)
        {
            var value = await _cacheService.GetOrSetAsync(
                userId, 
                id => Task.FromResult(new ComplexCacheObject
                {
                    Name = $"User {id}",
                    Tags = new[] { "user", $"id-{id}" },
                    Metadata = new Dictionary<string, string>
                    {
                        ["userId"] = id.ToString(),
                        ["generatedAt"] = DateTime.UtcNow.ToString("o")
                    }
                }),
                id => $"user:profile:{id}", // Custom key generation
                DistributedCacheEntryOptions.WithAbsoluteExpiration(DateTime.UtcNow.AddHours(1))
            );

            return Ok(value);
        }
    }

    /// <summary>
    /// Sample complex object for caching demonstration
    /// </summary>
    public class ComplexCacheObject
    {
        /// <summary>
        /// Name of the object
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Tags associated with the object
        /// </summary>
        public string[]? Tags { get; set; }

        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
