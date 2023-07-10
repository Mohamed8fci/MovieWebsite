using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieCrudOperation.Models;
using MovieCrudOperation.ViewModels;
using NToastNotify;
using NuGet.Packaging.Signing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MovieCrudOperation.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification toastNotification;
        private List<string> allowedExtensions = new List<string> { ".jpg", ".png" };
        private long maxAllowedPosterSize = 1048576;
        public MoviesController(ApplicationDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            this.toastNotification = toastNotification;
        }

        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies.OrderByDescending(m=>m.Rate).ToListAsync();
            return View(movies);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new MovieFormViewModel()
            {
                Genres = await _context.Genres.OrderBy(e => e.Name).ToListAsync()
            };
            return View("MovieForm", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _context.Genres.OrderBy(e => e.Name).ToListAsync();
                return View("MovieForm", model);
            }
                var files = Request.Form.Files;

                if (!files.Any())
                {
                    model.Genres = await _context.Genres.OrderBy(e => e.Name).ToListAsync();
                    ModelState.AddModelError("poster", "Please select a movie poster!");
                    return View("MovieForm", model);
                }

                var poster = files.FirstOrDefault();
                
                var extension = Path.GetExtension(poster.FileName);

                if (!allowedExtensions.Contains(extension.ToLower()))
                {
                    model.Genres = await _context.Genres.OrderBy(e => e.Name).ToListAsync();
                    ModelState.AddModelError("poster", "Only .png and .jpg images are allowed!");
                    return View("MovieForm", model);
                }

                if (poster.Length > maxAllowedPosterSize)
                {
                    model.Genres = await _context.Genres.OrderBy(e => e.Name).ToListAsync();
                    ModelState.AddModelError("poster", "Poster cannot be more than 1 MB!");
                    return View("MovieForm", model);
                }

                using var dataStream = new MemoryStream();
                await poster.CopyToAsync(dataStream);

                var movie = new Movie
                {
                    Title = model.Title,
                    GenreId = model.GenereId,
                    Year = model.Year,
                    Rate = model.Rate,
                    Storeline = model.StoryLine,
                    Poster = dataStream.ToArray(),
                };

                _context.Movies.Add(movie);
                await _context.SaveChangesAsync();

            toastNotification.AddSuccessToastMessage("movie created sucssuflly");

                return RedirectToAction(nameof(Index));
            }

            
        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.FindAsync(id);

            if(movie == null)
                return NotFound();

            var viewModel = new MovieFormViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                GenereId = movie.GenreId,
                Rate = movie.Rate,
                Year = movie.Year,
                StoryLine = movie.Storeline,
                Poster = movie.Poster,
                Genres = await _context.Genres.OrderBy(e => e.Name).ToListAsync()

            };
            return View("MovieForm",viewModel);
    }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _context.Genres.OrderBy(e => e.Name).ToListAsync();
                return View("MovieForm", model);
            }

            var movie = await _context.Movies.FindAsync(model.Id);

            if (movie == null)
                return NotFound();

            var files = Request.Form.Files;

            if (files.Any())
            {
                var poster = files.FirstOrDefault();
                using var dataStream = new MemoryStream();
                await poster.CopyToAsync(dataStream);

                model.Poster = dataStream.ToArray();

                if (!allowedExtensions.Contains(Path.GetExtension(poster.FileName).ToLower()))
                {
                    model.Genres = await _context.Genres.OrderBy(e => e.Name).ToListAsync();
                    ModelState.AddModelError("poster", "Only .png and .jpg images are allowed!");
                    return View("MovieForm", model);
                }

                if (poster.Length > maxAllowedPosterSize)
                {
                    model.Genres = await _context.Genres.OrderBy(e => e.Name).ToListAsync();
                    ModelState.AddModelError("poster", "Poster cannot be more than 1 MB!");
                    return View("MovieForm", model);
                }

                movie.Poster = model.Poster;
            }

            movie.Title = model.Title;
            movie.GenreId = model.GenereId;
            movie.Rate = model.Rate;
            movie.Year = model.Year;
            movie.Storeline = model.StoryLine;

            _context.SaveChanges();

            toastNotification.AddSuccessToastMessage("movie edit succsuffly");
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.Include(m=>m.Genre).SingleOrDefaultAsync(m => m.Id == id);

            if(movie == null)
                return NotFound();

            return View(movie);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
                return NotFound();

            _context.Movies.Remove(movie);
            _context.SaveChanges();
            return Ok();
        }
    }
}