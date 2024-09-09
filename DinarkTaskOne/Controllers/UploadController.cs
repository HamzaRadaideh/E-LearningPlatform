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

        // 1. Upload Course Material
        [HttpGet]
        public IActionResult UploadMaterial(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadMaterial(int courseId, IFormFile file, string fileType, string? description)
        {
            var course = await context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return NotFound("Course not found.");
            }

            if (file != null && file.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = file.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var material = new MaterialsModel
                {
                    CourseId = courseId,
                    FilePath = uniqueFileName,
                    FileType = fileType,
                    Description = description
                };

                context.Materials.Add(material);
                await context.SaveChangesAsync();
            }

            return RedirectToAction("CourseConfig", "Course", new { id = courseId });
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
