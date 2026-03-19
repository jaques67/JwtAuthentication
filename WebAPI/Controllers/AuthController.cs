using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Dto;
using WebAPI.Infrastructure;
using RegisterRequest = WebAPI.Dto.RegisterRequest;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataAccess _dataAccess;
        private readonly TokenProvider _tokenProvider;

        public AuthController(DataAccess dataAccess, TokenProvider tokenProvider)
        {
            _dataAccess = dataAccess;
            _tokenProvider = tokenProvider;
        }

        [HttpPost("register")]
        public ActionResult Register(RegisterRequest request)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            
            var result = _dataAccess.RegisterUser(request.Email, hashedPassword, request.Role);
            
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpPost("login")]
        public ActionResult<AuthResponse> Login(AuthRequest request)
        {
            AuthResponse response = new AuthResponse();
            
            var user = _dataAccess.FindUserByEmail(request.Email);
            if (user == null)
            {
                return BadRequest("Username is not found");
            }
            
            var verifyPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
            if (!verifyPassword)
            {
                return BadRequest("Password is incorrect");
            }
            
            // Generate Access token
            var token = _tokenProvider.GenerateToken(user);
            response.AccessToken = token.AccessToken;
            
            
            // Generate Refresh token
            response.RefreshToken = token.RefreshToken.Token;
            
            _dataAccess.DisableUserTokenByEmail(request.Email);
            _dataAccess.InsertRefreshToken(token.RefreshToken, request.Email);
            
            return Ok(response);
        }

        [HttpPost("refresh")]
        public ActionResult<AuthResponse> Refresh(RefreshRequest request)
        {
            var response = new AuthResponse();
            var refreshToken = Request.Cookies["refreshtoken"];
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest();
            
            var isValid = _dataAccess.IsRefreshTokenValid(refreshToken);
            if (!isValid)
                return BadRequest();
            
            var currentUser = _dataAccess.FindUserByToken(refreshToken);
            if (currentUser == null)
                return BadRequest();
            
            var token = _tokenProvider.GenerateToken(currentUser);
            response.AccessToken = token.AccessToken;
            response.RefreshToken = token.RefreshToken.Token;
            
            _dataAccess.DisableUserToken(refreshToken);
            _dataAccess.InsertRefreshToken(token.RefreshToken, currentUser.Email);
            
            return Ok(response);
        }

        [HttpPost("logout")]
        public ActionResult Logout()
        {
            var refreshToken = Request.Cookies["refreshtoken"];
            if (refreshToken != null)
            {
                _dataAccess.DisableUserToken(refreshToken);
            }
            return Ok();
        }
    }
    
}
