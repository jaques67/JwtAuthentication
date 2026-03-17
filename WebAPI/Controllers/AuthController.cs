using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Dto;
using WebAPI.Infrastructure;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataAccess _dataAccess;

        public AuthController(DataAccess dataAccess)
        {
            _dataAccess = dataAccess;
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
    }
}
