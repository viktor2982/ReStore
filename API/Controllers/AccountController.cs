using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<User> _userManager;
        private readonly TokenService _tokenService;
        private readonly StoreContext _context;
        public AccountController(UserManager<User> userManager, TokenService tokenService, StoreContext context)
        {
            _context = context;
            _tokenService = tokenService;
            _userManager = userManager;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO LoginDTO)
        {
            var user = await _userManager.FindByNameAsync(LoginDTO.Username);

            if (user == null || !await _userManager.CheckPasswordAsync(user, LoginDTO.Password))
                return Unauthorized();

            var userBasket = await RetrieveBasket(LoginDTO.Username);
            var anonymousBasket = await RetrieveBasket(Request.Cookies["buyerId"]);

            if (anonymousBasket != null)
            {
                if (userBasket != null) _context.Baskets.Remove(userBasket);
                anonymousBasket.BuyerId = user.UserName;
                Response.Cookies.Delete("buyerId");
                await _context.SaveChangesAsync();
            }

            return new UserDTO()
            {
                Email = user.Email,
                Token = await _tokenService.GenerateToken(user),
                Basket = anonymousBasket != null ? anonymousBasket.MapBasketToDTO() : userBasket.MapBasketToDTO()
            };
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDTO RegisterDTO)
        {
            var user = new User()
            {
                UserName = RegisterDTO.Username,
                Email = RegisterDTO.Email
            };

            var result = await _userManager.CreateAsync(user, RegisterDTO.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

                return ValidationProblem();
            }

            await _userManager.AddToRoleAsync(user, "Member");

            return StatusCode(201);
        }

        [Authorize]
        [HttpGet("currentUser")]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var userBasket = await RetrieveBasket(User.Identity.Name);

            return new UserDTO()
            {
                Email = user.Email,
                Token = await _tokenService.GenerateToken(user),
                Basket = userBasket.MapBasketToDTO()
            };
        }


        private async Task<Basket> RetrieveBasket(string buyerId)
        {
            if (string.IsNullOrEmpty(buyerId))
            {
                Response.Cookies.Delete("buyerId");
                return null;
            }

            return await _context.Baskets
                .Include(i => i.Items)
                    .ThenInclude(p => p.Product)
                .FirstOrDefaultAsync(x => x.BuyerId == buyerId);
        }
    }
}