using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace KockpitPortal.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private IHostingEnvironment _env;
        public UploadController(IHostingEnvironment env)
        {
            _env = env;
        }

        class UploadResult
        {
            public int StatusCode { get; set; }
            public string Message { get; set; }
        }

        [HttpPost("base64")]
        public IActionResult base64()
        {
            try
            {
                string data = HttpContext.Request.Form["data"].ToString();
                string sPath = "";
                sPath = Path.Combine(_env.WebRootPath, "locker");

                if (!Directory.Exists(sPath))
                {
                    Directory.CreateDirectory(sPath);
                }

                string filePath = Guid.NewGuid().ToString() + ".png";
                sPath = Path.Combine(sPath, filePath);
                System.IO.File.WriteAllBytes(sPath, Convert.FromBase64String(data));
                var s = new Uri($"{Request.Scheme}://{Request.Host}/locker/{filePath}").ToString();
                var response = new UploadResult { 
                    StatusCode = 1,
                    Message = s
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return NotFound(new UploadResult() { StatusCode = 1, Message = $"Error : {ex.Message}" });
            }
        }

        //[HttpPost("base64")]
        //public IActionResult base64()
        //{
        //    try
        //    {
        //        string data = HttpContext.Request.Form["data"].ToString();
        //        string sPath = "";
        //        sPath = Path.Combine(_env.WebRootPath, "locker");

        //        if (!Directory.Exists(sPath))
        //        {
        //            Directory.CreateDirectory(sPath);
        //        }

        //        string filePath = Guid.NewGuid().ToString() + ".png";
        //        sPath = Path.Combine(sPath, filePath);
        //        System.IO.File.WriteAllBytes(sPath, Convert.FromBase64String(data));
        //        var s = new Uri($"{Request.Scheme}://{Request.Host}/locker/{filePath}").ToString();

        //        //var s = $"{HttpContext.Request}{System.Uri.SchemeDelimiter}{HttpContext.Current.Request.Url.Authority}{"/sub/locker/"}{filePath}";
        //        return Ok(new UploadResult() { StatusCode = 1, Message = s });
        //    }
        //    catch (Exception ex)
        //    {
        //        return NotFound(new UploadResult() { StatusCode = 0, Message = ex.Message });
        //    }

        //}
    }

}
