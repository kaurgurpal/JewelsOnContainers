﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebMVC.Infrastructure;
using WebMVC.Models;
using WebMVC.Models.CartModels;

namespace WebMVC.Services
{
    public class CartService : ICartService
    {
        private readonly IConfiguration _config;
        private IHttpClient _apiClient;
        private readonly string _remoteServiceBaseUrl;
        private readonly ILogger _logger;
        private IHttpContextAccessor _httpContextAccesor;

        public CartService(IConfiguration config,
            IHttpContextAccessor httpContextAccesor,
    IHttpClient httpClient,
    ILoggerFactory logger)
        {
            _config = config;
            _remoteServiceBaseUrl = $"{_config["CartUrl"]}/api/v1/cart";
            _apiClient = httpClient;
            _logger = logger.CreateLogger<CartService>();
            _httpContextAccesor = httpContextAccesor;

        }

        public async Task AddItemToCartAsync(ApplicationUser user, CartItem product)
        {
            var cart = await GetCart(user);
            _logger.LogDebug("User Name: " + user.Id);
            if (cart == null)
            {
                cart = new Cart()
                {
                    BuyerId = user.Id,
                    Items = new List<CartItem>()
                };
            }
            var basketItem = cart.Items
                .Where(p => p.ProductId == product.ProductId)
                .FirstOrDefault();
            if (basketItem == null)
            {
                cart.Items.Add(product);
            }
            else
            {
                basketItem.Quantity += 1;
            }


            await UpdateCart(cart);
        }

        public async Task ClearCart(ApplicationUser user)
        {
            var token = await GetUserTokenAsync();
            var cleanBasketUri = ApiPaths.Basket.CleanBasket(_remoteServiceBaseUrl, user.Id);
            _logger.LogDebug("Clean Basket uri : " + cleanBasketUri);
            var response = await _apiClient.DeleteAsync(cleanBasketUri);
            _logger.LogDebug("Basket cleaned");
        }

        public async Task<Cart> GetCart(ApplicationUser user)
        {
            var token = await GetUserTokenAsync();
            _logger.LogInformation(" We are in get basket and user id " + user.Email);
            _logger.LogInformation(_remoteServiceBaseUrl);

            var getBasketUri = ApiPaths.Basket.GetBasket(_remoteServiceBaseUrl, user.Email);
            _logger.LogInformation(getBasketUri);
            var dataString = await _apiClient.GetStringAsync(getBasketUri, token);
            _logger.LogInformation(dataString);

            var response = JsonConvert.DeserializeObject<Cart>(dataString.ToString()) ??
               new Cart()
               {
                   BuyerId = user.Email
               };
            return response;
        }

        async Task<string> GetUserTokenAsync()
        {
            var context = _httpContextAccesor.HttpContext;

            return await context.GetTokenAsync("access_token");

        }
        public async Task<Cart> SetQuantitiesAsync(ApplicationUser user, Dictionary<string, int> quantities)
        {
            var basket = await GetCart(user);

            basket.Items.ForEach(x =>
            {
                // Simplify this logic by using the
                // new out variable initializer.
                if (quantities.TryGetValue(x.Id, out var quantity))
                {
                    x.Quantity = quantity;
                }
            });

            return basket;
        }

        public async Task<Cart> UpdateCart(Cart Cart)
        {
            var token = await GetUserTokenAsync();
            _logger.LogDebug("Service url: " + _remoteServiceBaseUrl);
            var updateBasketUri = ApiPaths.Basket.UpdateBasket(_remoteServiceBaseUrl);
            _logger.LogDebug("Update Basket url: " + updateBasketUri);
            var response = await _apiClient.PostAsync(updateBasketUri, Cart, token);
            response.EnsureSuccessStatusCode();

            return Cart;
        }
    }
}
