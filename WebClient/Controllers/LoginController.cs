using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;

namespace WebClient.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly UserCRUD.UserCRUDClient client;
        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
            string url = "http://localhost:5104";

            var channel = GrpcChannel.ForAddress(url);
            client = new UserCRUD.UserCRUDClient(channel);
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginModel? acc)
        {
            try
            {
                TokenJwt jwt = client.Login(acc);

                if (jwt.Token == "")  {
                    ViewBag.Message = "Username or Password is wrong.";
                    return View();
                }

                HttpContext.Session.SetString("token", jwt.Token);
                var headers = new Metadata
                {
                    { "Authorization", $"Bearer {jwt.Token}" }
                };

                User user = client.GetInfor(new Empty(), headers);
                HttpContext.Session.SetString("role", user.Role);

                return RedirectToAction("Index", "Home");
            }
            catch
            {
                return View();
            }
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Index));
        }

    }
}
