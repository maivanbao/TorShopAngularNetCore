using AngularNetCoreIMS.WebApi.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace AngularNetCoreIMS.WebApi.Controllers
{
	[Route("api/[controller]")]
	public class TokenAuthController : Controller
	{
		[HttpPut("Login")]
		public IActionResult Login([FromBody]User user)
		{
			if (user != null)
			{
				User existUser = UserStorage.Users.FirstOrDefault(u => u.Username == user.Username && u.Password == user.Password);

				if (existUser != null)
				{
					var requestAt = DateTime.Now;
					var expiresIn = requestAt + TokenAuthOption.ExpiresSpan;
					var token = GenerateToken(existUser, expiresIn);

					return Json(new RequestResult
					{
						State = RequestState.Success,
						Data = new
						{
							requertAt = requestAt,
							expiresIn = TokenAuthOption.ExpiresSpan.TotalSeconds,
							tokeyType = TokenAuthOption.TokenType,
							accessToken = token
						}
					});
				}
				else
				{
					return Json(new RequestResult
					{
						State = RequestState.Failed,
						Msg = "Username or password is invalid"
					});
				}
			}
			else
			{
				return Json(new RequestResult
				{
					State = RequestState.Failed,
					Msg = "Username or password is invalid"
				});
			}
		}

		private string GenerateToken(User user, DateTime expires)
		{
			var handler = new JwtSecurityTokenHandler();

			ClaimsIdentity identity = new ClaimsIdentity(
				new GenericIdentity(user.Username, "TokenAuth"),
				new[] { new Claim("ID", user.ID.ToString()) }
			);

			var securityToken = handler.CreateToken(new SecurityTokenDescriptor
			{
				Issuer = TokenAuthOption.Issuer,
				Audience = TokenAuthOption.Audience,
				SigningCredentials = TokenAuthOption.SigningCredentials,
				Subject = identity,
				Expires = expires
			});
			return handler.WriteToken(securityToken);
		}

		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[HttpGet]
		public IActionResult GetUserInfo()
		{
			var claimsIdentity = User.Identity as ClaimsIdentity;
			return Json(new RequestResult
			{
				State = RequestState.Success,
				Data = new { UserName = claimsIdentity.Name }
			});
		}
	}
}