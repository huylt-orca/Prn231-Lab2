using Grpc.Core;
using GrpcServiceDemo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using GrpcServiceDemo.Models;
using System.Linq;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace GrpcServiceDemo.Services
{
    public class BookService : BookCRUD.BookCRUDBase
    {
        private readonly ILogger<BookService> _logger;
        private MyDBContext db;
        public BookService(ILogger<BookService> logger, MyDBContext db)
        {
            _logger = logger;
            this.db = db;
        }

        [Authorize(Roles = "admin")]
        public override Task<Empty> Delete(BookFilter request, ServerCallContext context)
        {
            var book = db.Books.FirstOrDefault(c => c.Id == request.Id);
            db.Books.Remove(book!);
            db.SaveChanges(true);
            return Task.FromResult(new Empty());
        }

        [Authorize]
        public override Task<Presses> GetAllPress(Empty request, ServerCallContext context)
        {
            Presses presses = new Presses();
            presses.Items.AddRange(db.Presss.ToArray().Select(o => new Press()
            {
               Id = o.Id,
                Name = o.Name
            }));
            return Task.FromResult(presses);
        }

        [Authorize(Roles = "admin")]
        public override Task<Empty> Insert(Book request, ServerCallContext context)
        {
            Address address = new Address()
            {
                Street = request.Street,
                City = request.City
            };
            db.Books.Add(new Models.Book()
            {
                Title = request.Title,
                Author = request.Author,
                ISBN = request.ISBN,
                Location = address,
                Price = (decimal)request.Price,
                PressId = request.PressId
            });
            db.SaveChanges();
            return Task.FromResult(new Empty());
        }

        [Authorize]
        public override Task<Books> SelectAll(BookFilterString request, ServerCallContext context)
        {
            request.PageSize = request.PageSize == 0 ? 3 : request.PageSize;
            request.Page = request.Page == 0 ? 1 : request.Page;

            Books books = new Books();
            books.Items.AddRange(db.Books.Where(b =>
            b.ISBN.Contains(request.Value) || b.Title.Contains(request.Value) || b.Author.Contains(request.Value)
                        )
                        .OrderByDescending(m => m.Id)
                        .Include(b => b.Press)
                        .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
                        .ToArray().Select(o => new Book()
            {
                Id= o.Id ?? 0,
                Title = o.Title,
                Author= o.Author,
                PressId= o.PressId,
                City= o.Location!.City,
                Street = o.Location!.Street,
                Price = (double)o.Price,
                ISBN = o.ISBN,
                PressName = o.Press!.Name
            }));
            return Task.FromResult(books);
        }

        [Authorize]
        public override Task<Book> SelectByID(BookFilter request, ServerCallContext context)
        {
            var data = db.Books.Include(b => b.Press)
                .Select(o => new Book()
                {
                    Id = o.Id ?? 0,
                    Title = o.Title,
                    Author = o.Author,
                    PressId = o.PressId,
                    City = o.Location!.City,
                    Street = o.Location!.Street,
                    ISBN = o.ISBN,
                    Price = (double)o.Price,
                    PressName = o.Press!.Name
                })
                .FirstOrDefault(c => c.Id == request.Id);

            return Task.FromResult(data!);
        }

        [Authorize(Roles = "admin")]
        public override Task<Empty> Update(Book request, ServerCallContext context)
        {
            Address address = new Address()
            {
                City = request.City,
                Street = request.Street
            };

            db.Books.Update(new Models.Book()
            {
                Id = request.Id,
                Title = request.Title,
                Author = request.Author,
                PressId = request.PressId,
                Location = address,
                ISBN = request.ISBN,
                Price = (decimal)request.Price
            });
            db.SaveChanges();
            return Task.FromResult(new Empty());
        }

        [Authorize]
        public override Task<TotalBook> GetTotalBook(BookFilterString request, ServerCallContext context)
        {
            int count = db.Books.Where(b =>
                        b.ISBN.Contains(request.Value) || b.Title.Contains(request.Value) || b.Author.Contains(request.Value)
                        ).Count();
            TotalBook totalBook = new TotalBook()
            {
                Total = count,
            };

            return Task.FromResult(totalBook);
        }
    }
}