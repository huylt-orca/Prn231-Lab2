using Grpc.Core;
using GrpcServiceDemo.Models;
using GrpcServiceDemo.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GrpcServiceDemo.Services
{
    public class UserService : UserCRUD.UserCRUDBase
    {
        private readonly JwtService _jwtService;
        private readonly ILogger<UserService> _logger;
        private readonly MyDBContext db;
        public UserService(ILogger<UserService> logger, MyDBContext db, JwtService jwtService)
        {
            this._jwtService = jwtService;
            _logger = logger;
            this.db = db;
        }

        [Authorize]
        public override Task<User> GetInfor(Empty request, ServerCallContext context)
        {

            ClaimsPrincipal currentUser = context.GetHttpContext()?.User as ClaimsPrincipal;
            int userId = int.Parse(currentUser.FindFirstValue("Id"));

            var data = db.Users
                .Select(o => new User()
                {
                    Id = o.Id,
                    Name = o.Name,
                    Role = o.Role == Role.ADMIN ? "Admin" : "User",
                    Username = o.Username
                })
                .FirstOrDefault(c => c.Id == userId);

            return Task.FromResult(data!);
        }

        public override Task<TokenJwt> Login(LoginModel request, ServerCallContext context)
        {
            var user = db.Users.FirstOrDefaultAsync(e => e.Username == (string)request.Username).Result;
            if (user == null)
            {
                return Task.FromResult(new TokenJwt() { Token = ""}); 
            }
            bool isVerify = PasswordService.Verify(request.Password, user.Password);

            if (isVerify)
            {
                string token = _jwtService.GenerateJwtToken(user.Username, user.Role == Role.ADMIN ? "admin" : "user", Convert.ToString(user.Id));
                TokenJwt tokenJwt = new TokenJwt()
                {
                    Token = token
                };
                return Task.FromResult(tokenJwt);
            }
            return Task.FromResult(new TokenJwt() { Token = "" });

        }

        public override Task<MessageResponse> Register(User request, ServerCallContext context)
        {
            try
            {
                var user = db.Users.FirstOrDefaultAsync(e => e.Username == (string)request.Username).Result;
                if (user != null)
                {
                    return Task.FromResult(new MessageResponse()
                    {
                        Status = 1,
                        Message = "Username duplicated"
                    });
                }

                string hashPassword = PasswordService.Hash(request.Password);
                db.Users.AddAsync(new Models.User()
                {
                    Username = request.Username,
                    Password = hashPassword,
                    Name = request.Name,
                    Role = request.Role == "Admin" ? Role.ADMIN : Role.USER
                });
                db.SaveChangesAsync();
                return Task.FromResult(new MessageResponse()
                {
                    Status = 0,
                    Message = "Register Successful"
                });
            }
            catch
            {
                return Task.FromResult(new MessageResponse() { 
                    Status = 1,
                    Message = "Register Failed" 
                });
            }
        }
    }
}
