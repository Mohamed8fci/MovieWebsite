using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MovieCrudOperation.Models;

namespace MovieCrudOperation.ViewModels
{
    public class MovieFormViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(250)]
        public string Title { get; set; }
        public int Year { get; set; }
        [Range(1,10)]
        public double Rate { get; set; }
        [Required, StringLength(2500)]
        public string? StoryLine { get; set; }
        [Display(Name ="Select Poster ...")]
        public byte[] Poster { get; set; } = new byte[0];

        [Display(Name ="Genre")]
        public byte GenereId { get; set; }

        public IEnumerable<Genre>? Genres { get; set; }
    }
}
