using Microsoft.AspNetCore.Mvc;
using MVCFileUpload.Models;

namespace MVCFileUpload.Controllers
{
    public class AssignmentController : Controller
    {
        //to allow access into the folder and the file we are going to create
        private readonly IWebHostEnvironment _webHostEnvironment;
        //used to access wwwroot and files

        public AssignmentController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            //tells the system to look at the wwwroot 
        }

        private static List<Assignment> assignments = new List<Assignment>(); //stoe list yo memory
        public IActionResult Index()
        {
            return View(assignments); //returns the list
        }

        [HttpPost]
        public IActionResult Upload(IFormFile file, string UploaderName)
        {
            if (file != null && file.Length > 0)
            {
                //get original file name
                var fileName = Path.GetFileName(file.FileName);

                //create the path to save the file under wwwroot/uploads
                var path = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(stream); //copy the uploaded file into the stream
                }
                assignments.Add(new Assignment // add assignment details to th list 
                {
                    Id = assignments.Count + 1,
                    FileName = fileName,
                    UploaderName = UploaderName,
                    UploadDate = DateTime.Now
                });
            }
            return RedirectToAction("Index"); //show the updated list
        }

        public ActionResult OpenFile(string fileName)
        {
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);
            var fileBytes = System.IO.File.ReadAllBytes(path);

            //return the file to browsers as download
            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}
