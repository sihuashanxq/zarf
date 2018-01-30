using AspNetCoreApp.Contexts;
using AspNetCoreApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreApp.Controllers
{
    public class HomeController : Controller
    {
        public SqlServerDemoDbContext SqlServerDbContext { get; }

        public HomeController(SqlServerDemoDbContext sqlServerDbContext)
        {
            SqlServerDbContext = sqlServerDbContext;
        }

        public IActionResult Index()
        {
            var users = SqlServerDbContext
                  .Query<User>()
                  .Where(i => i.Id < 10)
                  .ToList();

            return View(users);
        }
    }
}
