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

        // 1. Upload Material - GET
        [HttpGet]
        public IActionResult UploadMaterial(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        // 1. Upload Material - POST
        [HttpPost]
        public async Task<IActionResult> UploadMaterial(int courseId, IFormFile file, string? description)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file.");
                return View();
            }

            var instructorId = GetCurrentUserId();
            var course = await context.Courses
                .Where(c => c.CourseId == courseId && c.InstructorId == instructorId)
                .FirstOrDefaultAsync();

            if (course == null)
            {
                return NotFound("Course not found or unauthorized access.");
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var acceptableExtensions = new List<string> { ".pdf", ".mp4", ".webm", ".avi" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!acceptableExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("", "Invalid file type. Only PDF and video files (MP4, WebM, AVI) are allowed.");
                return View();
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileNameWithoutExtension(file.FileName) + fileExtension;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string fileType = fileExtension == ".pdf" ? "PDF" : "Video";

            var material = new MaterialsModel
            {
                CourseId = courseId,
                FilePath = uniqueFileName,
                FileType = fileType,
                Description = description
            };

            context.Materials.Add(material);
            await context.SaveChangesAsync();

            return RedirectToAction("CourseConfig", "Course", new { id = courseId });
        }

        // 2. Edit Material - GET
        [HttpGet]
        public async Task<IActionResult> EditMaterial(int id)
        {
            var material = await context.Materials
                .Include(m => m.Course)
                .Where(m => m.MaterialId == id && m.Course.InstructorId == GetCurrentUserId())
                .FirstOrDefaultAsync();

            if (material == null)
            {
                return NotFound("Material not found or unauthorized access.");
            }

            return View(material);
        }

        // 2. Edit Material - POST
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
            existingMaterial.UpdatedAt = DateTime.UtcNow;

            context.Materials.Update(existingMaterial);
            await context.SaveChangesAsync();

            return RedirectToAction("CourseConfig", "Course", new { id = existingMaterial.CourseId });
        }

        // 3. Delete Material
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
