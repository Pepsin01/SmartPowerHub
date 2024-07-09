using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Serilog; // Ensure Serilog is configured in your project for logging

namespace SmartPowerHub.Data
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        // Path where the uploaded files will be stored
        private static readonly string StoragePath = Path.Combine(Directory.GetCurrentDirectory(), "IoTControllers");

        public FileUploadController()
        {
            // Ensure the directory exists
            if (!Directory.Exists(StoragePath))
            {
                Directory.CreateDirectory(StoragePath);
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(List<IFormFile> files)
        {
            var filePaths = new List<string>();
            foreach (var file in files)
            {
                // Security check: Confirm the file is a .dll
                if (Path.GetExtension(file.FileName).Equals(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    var filePath = Path.Combine(StoragePath, file.FileName);
                    try
                    {
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        filePaths.Add(filePath);
                        Log.Information("File uploaded successfully: {FileName}", file.FileName);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("An error occurred while uploading the file {FileName}: {ErrorMessage}", file.FileName, ex.Message);
                        return StatusCode(StatusCodes.Status500InternalServerError, $"Error uploading file {file.FileName}");
                    }
                }
                else
                {
                    Log.Warning("Attempt to upload non-DLL file: {FileName}", file.FileName);
                    return BadRequest("Only .DLL files are allowed.");
                }
            }

            return Ok(new { Message = "Files uploaded successfully!", Files = filePaths });
        }
    }
}
