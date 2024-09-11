using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DinarkTaskOne.Data;
using DinarkTaskOne.Models.ManageCourse;
using System.IO;
using System.Threading.Tasks;
using System.Security.Claims;

namespace DinarkTaskOne.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class UploadController(ApplicationDbContext context) : Controller
    {

        private int GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("User ID claim is missing.");
            return int.Parse(userIdValue);
        }

        // The view that returns the upload form when the page loads
        [HttpGet]
        public IActionResult UploadMaterial(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadMaterial(int courseId, IFormFile file, string? description)
        {
            // If no file is selected, return the form with validation errors
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file.");
                return View(); // Optionally pass ViewBag.CourseId to keep courseId in the view
            }

            try
            {
                // Ensure the course belongs to the logged-in instructor
                var instructorId = GetCurrentUserId();
                var course = await context.Courses
                    .Where(c => c.CourseId == courseId && c.InstructorId == instructorId)
                    .FirstOrDefaultAsync();

                if (course == null)
                {
                    return NotFound("Course not found or unauthorized access.");
                }

                // Define the folder for uploads
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");

                // Ensure the folder exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Check for acceptable file extensions (PDF and video formats)
                var acceptableExtensions = new List<string> { ".pdf", ".mp4", ".webm", ".avi" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!acceptableExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("", "Invalid file type. Only PDF and video files (MP4, WebM, AVI) are allowed.");
                    return View();
                }

                // Generate a unique file name to prevent conflicts
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileNameWithoutExtension(file.FileName) + fileExtension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file to the uploads folder
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Determine the file type based on the extension
                string fileType = fileExtension == ".pdf" ? "PDF" : "Video";

                // Create a new material record
                var material = new MaterialsModel
                {
                    CourseId = courseId,
                    FilePath = uniqueFileName,
                    FileType = fileType,
                    Description = description
                };

                // Add the material to the database and save changes
                context.Materials.Add(material);
                await context.SaveChangesAsync();

                // Redirect back to course configuration after a successful upload
                return RedirectToAction("CourseConfig", "Course", new { id = courseId });
            }
            catch (Exception ex)
            {
                // Log the error and return an error response
                Console.WriteLine(ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // 2. Edit Upload
        [HttpGet]
        public async Task<IActionResult> EditMaterial(int id)
        {
            var material = await context.Materials.FindAsync(id);
            if (material == null)
            {
                return NotFound("Material not found.");
            }

            return View(material);
        }

        [HttpPost]
        public async Task<IActionResult> EditMaterial(MaterialsModel material)
        {
            var existingMaterial = await context.Materials
                .Include(m => m.Course)
                .Where(m => m.MaterialId == material.MaterialId && m.Course.InstructorId == GetCurrentUserId())
                .FirstOrDefaultAsync();

            if (existingMaterial == null)
            {
                return NotFound("Material not found or unauthorized access.");
            }

            existingMaterial.FileType = material.FileType;
            existingMaterial.Description = material.Description;
            context.Materials.Update(existingMaterial);
            await context.SaveChangesAsync();

            return RedirectToAction("CourseConfig", "Course", new { id = existingMaterial.CourseId });
        }

        // 3. Delete Upload
        [HttpPost]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            var material = await context.Materials
                .Include(m => m.Course)
                .Where(m => m.MaterialId == id && m.Course.InstructorId == GetCurrentUserId())
                .FirstOrDefaultAsync();

            if (material == null)
            {
                return NotFound("Material not found or unauthorized access.");
            }

            context.Materials.Remove(material);
            await context.SaveChangesAsync();

            return RedirectToAction("CourseConfig", "Course", new { id = material.CourseId });
        }
    }
}
