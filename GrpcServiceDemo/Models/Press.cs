using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrpcServiceDemo.Models
{
    public enum Category
    {
        Book,
        Magazine,
        EBook
    }
    public class Press
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Name { get; set; }
        public Category? Category { get; set; }

        [InverseProperty("Press")]
        public ICollection<Book>? Books { get; set; }
    }
}
