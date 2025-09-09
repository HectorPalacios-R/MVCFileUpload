using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using MVCFileUpload.Models;
using System.Security.Cryptography;

namespace MVCFileUpload.Controllers
{
    public class AssignmentController : Controller
    {
        private readonly IWebHostEnvironment _env;

        private const string Base64Key = "iKwxciCzDX6mWEll+c2Ko2xFUaT2TlwbmMfWLY+kX/4=";

        private static readonly List<Assignment> assignments = new(); 
        public AssignmentController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index()
        {
            return View(assignments);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, string uploaderName)
        {
            if (file == null || file.Length == 0)
                return RedirectToAction(nameof(Index));

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var originalName = Path.GetFileName(file.FileName);
            var encryptedPath = Path.Combine(uploadsFolder, originalName + ".enc");

            // Read file into memory
            byte[] plainBytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                plainBytes = ms.ToArray();
            }

            // Encrypt and save: format = [IV(16)][ciphertext(...)]
            var key = Convert.FromBase64String(Base64Key);
            var iv = RandomNumberGenerator.GetBytes(16);

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var encryptor = aes.CreateEncryptor();
                var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                await System.IO.File.WriteAllBytesAsync(encryptedPath, iv.Concat(cipherBytes).ToArray());
            }

            assignments.Add(new Assignment
            {
                Id = assignments.Count + 1,
                FileName = originalName,     // keep the friendly name
                UploaderName = uploaderName,
                UploadDate = DateTime.Now
            });

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> OpenFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return NotFound();

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            var encryptedPath = Path.Combine(uploadsFolder, fileName + ".enc");
            if (!System.IO.File.Exists(encryptedPath))
                return NotFound();

            // Read [IV|cipher]
            var data = await System.IO.File.ReadAllBytesAsync(encryptedPath);
            if (data.Length < 16) return BadRequest("File is corrupted.");

            var iv = new byte[16];
            Buffer.BlockCopy(data, 0, iv, 0, 16);
            var cipher = new byte[data.Length - 16];
            Buffer.BlockCopy(data, 16, cipher, 0, cipher.Length);

            // Decrypt
            var key = Convert.FromBase64String(Base64Key);
            byte[] plain;
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            }

            // Guess content type from original extension
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
                contentType = "application/octet-stream";

            return File(plain, contentType, fileName);
        }
    }
}
