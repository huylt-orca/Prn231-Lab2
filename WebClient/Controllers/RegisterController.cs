using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace WebClient.Controllers
{
    public class RegisterController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly UserCRUD.UserCRUDClient client;
        public RegisterController(ILogger<LoginController> logger)
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
        public async Task<IActionResult> Index(User user)
        {
            try
            {
                MessageResponse response = client.Register(user);
                if (response.Status == 1)
                {
                    ViewBag.Message = response.Message;
                    return View();
                }
                return RedirectToAction("Index", "Login");
            }
            catch
            {
                return View();
            }
        }
    }
}
