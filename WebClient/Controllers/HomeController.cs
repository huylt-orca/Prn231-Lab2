using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using WebClient.Models;

namespace WebClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BookCRUD.BookCRUDClient client;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            string url = "http://localhost:5104";

            var channel = GrpcChannel.ForAddress(url);
            client = new BookCRUD.BookCRUDClient(channel);
        }

        public async Task<IActionResult> Index(string searchKeyword = "", int page = 1, int pageSize = 3)
        {
            string token = HttpContext.Session.GetString("token");
            if (token == null)
            {
                return RedirectToAction("Index", "Login");
            }

            searchKeyword = searchKeyword == null ? "" : searchKeyword;

            BookFilterString request = new BookFilterString()
            {
                Value = searchKeyword,
                Page = page,
                PageSize = pageSize
            };

            var headers = new Metadata
                {
                    { "Authorization", $"Bearer {token}" }
                };

            Books books = client.SelectAll(request,headers);

            TotalBook totalBook = client.GetTotalBook(request, headers);

            ViewData["total"] = (int)Math.Ceiling((decimal)totalBook.Total / pageSize);
            ViewData["currentPage"] = page;
            ViewData["searchValue"] = searchKeyword;

            return View(books);
        }

        public async Task<IActionResult> Create()
        {
            string token = HttpContext.Session.GetString("token");
            if (token == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var headers = new Metadata
                {
                    { "Authorization", $"Bearer {token}" }
                };

            Presses presses = client.GetAllPress(new Empty(),headers);
            TempData["PressList"] = presses.Items.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            string token = HttpContext.Session.GetString("token");
            var headers = new Metadata
                {
                    { "Authorization", $"Bearer {token}" }
                };
            client.Insert(book, headers);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            string token = HttpContext.Session.GetString("token");
            if (token == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var headers = new Metadata
                {
                    { "Authorization", $"Bearer {token}" }
                };

            Presses presses = client.GetAllPress(new Empty(), headers);
            TempData["PressList"] = presses.Items.ToList();

            Book book = client.SelectByID(new BookFilter()
            {
                Id = id
            }, headers);
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            string token = HttpContext.Session.GetString("token");
            var headers = new Metadata
                {
                    { "Authorization", $"Bearer {token}" }
                };
            client.Update(book, headers);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            string token = HttpContext.Session.GetString("token");
            if (token == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var headers = new Metadata
                {
                    { "Authorization", $"Bearer {token}" }
                };
            Book book = client.SelectByID(new BookFilter()
            {
                Id = id
            }, headers);
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, Book book)
        {
            string token = HttpContext.Session.GetString("token");
            var headers = new Metadata
                {
                    { "Authorization", $"Bearer {token}" }
                };

            client.Delete(new BookFilter()
            {
                Id = id
            }, headers);
            return RedirectToAction(nameof(Index));
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}