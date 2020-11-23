using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BookStore_API.Contracts;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILoggerService _logger;
        private readonly IConfiguration _config;

        public UsersController(SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILoggerService logger,
            IConfiguration config)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _config = config;
        }
        /// <summary>
        /// User Registration Endpoint
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> register([FromBody] UserDTO userDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                var username = userDTO.EmailAddress;
                var password = userDTO.Password;

                _logger.LogInfo($"{location}: Registration attempt from User: {username}");
                var user = new IdentityUser { Email = username, UserName = username };
                var result = await _userManager.CreateAsync(user, password);
                await _userManager.AddToRoleAsync(user, "Customer");

                if (!result.Succeeded)
                {                   
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError($"{location}: {error.Code} {error.Description}");
                    }
                    return InternalError($"{location}: {username} User registration attempt failed.");
                }             
               
                 return Ok(new { result.Succeeded });                
            }
            catch (Exception ex)
            {
                return InternalError($"{ex.Message} - {ex.InnerException}");
            }

        }


        /// <summary>
        /// User Login Endpoint
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        [Route("login")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserDTO userDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                var username = userDTO.EmailAddress;
                var password = userDTO.Password;
                _logger.LogInfo($"{location}: Login attempt from User: {username}");
                var result = await _signInManager.PasswordSignInAsync(username, password, false, false);
                if (result.Succeeded)
                {
                    _logger.LogInfo($"{location}: {username} Successfully authenticated.");
                    var user = await _userManager.FindByEmailAsync(username);
                    var tokenString = await GenerateJSONWebToken(user);
                    return Ok(new { token = tokenString});
                }
                _logger.LogInfo($"{location}: {username} Not authenticated.");
                return Unauthorized(userDTO);
            }
            catch(Exception ex)
            {
                return InternalError($"{ex.Message} - {ex.InnerException}");
            }

                    
        
        }

        //A token stores the credentials and claims that we defined, including the role
        //The token has encoded information about the user.
        private async Task<string> GenerateJSONWebToken (IdentityUser user)
        {
            //Generate the security key
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            //The credentials should include hashing
            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

            //Create claims inside of a token: means, what is your information, who are you claiming to be
            //The token can tell the user all this information in a secure hashed way
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                //JWT Unique identifier, allows the token to be used only once
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };
            var roles = await _userManager.GetRolesAsync(user);

            //Add roles to the list of claims
            claims.AddRange(roles.Select(r => new Claim(ClaimsIdentity.DefaultRoleClaimType, r))) ;

            //Alternative code
            //foreach(var role in roles)
            //{
            //    claims.Add(new Claim(ClaimTypes.Role, role));
            //}

            var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Issuer"], 
                claims,
                null, 
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);            

            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private string GetControllerActionNames()
        {
            
            var controller = ControllerContext.ActionDescriptor.ControllerName;           
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }
        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong. Please, contact the administrator.");
        }
    }   
}
