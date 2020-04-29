using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Mvc;

namespace CheckListManager.WebApplication.Controllers
{
    public class DefaultController : BaseController
    {
        [AllowAnonymous]
        [Route("Settings")]
        public ActionResult Settings()
        {
            return Json(new { API = ConfigurationManager.AppSettings["BaseUrl"] }, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        [Route("Error", Order = 1)]
        public ActionResult Error()
        {
            return ConditionalView();
        }

        [AllowAnonymous]
        public ActionResult Loader(string url)
        {
            //Load Balancer Healt Check
            if ("ping".Equals(url, StringComparison.InvariantCultureIgnoreCase))
                return Ping();
            else if ("qrcode".Equals(url, StringComparison.InvariantCultureIgnoreCase))
                return QRCode();

            return ConditionalView("Loader", url ?? "Dashboard");
        }

        [Route("Dashboard")]
        public ActionResult Dashboard()
        {
            return ConditionalView();
        }

        [HttpPost]
        [Route]
        public ActionResult RedirectToDashboard()
        {
            return RedirectToAction("Dashboard");
        }

        [AllowAnonymous]
        [Route("Ping")]
        public ActionResult Ping()
        {
            return Json(new { ok = true }, JsonRequestBehavior.AllowGet);
        }

        private ActionResult QRCode()
        {
            var Encoder = new Gma.QrCodeNet.Encoding.QrEncoder(Gma.QrCodeNet.Encoding.ErrorCorrectionLevel.M);
            var Code = Encoder.Encode($"checklist://{Request.Url.Authority + Request.ApplicationPath.TrimEnd('/')}");
            const int PIXEL_SIZE = 8;
            var Image = new Bitmap(Code.Matrix.Width * PIXEL_SIZE, Code.Matrix.Height * PIXEL_SIZE);
            for (int X = 0; X < Code.Matrix.Width; ++X)
                for (int Y = 0; Y < Code.Matrix.Height; ++Y)
                    for (int XOffset = 0; XOffset < PIXEL_SIZE; ++XOffset)
                        for (int YOffset = 0; YOffset < PIXEL_SIZE; ++YOffset)
                            Image.SetPixel(X * PIXEL_SIZE + XOffset, Y * PIXEL_SIZE + YOffset, Code.Matrix.InternalArray[X, Y] ? Color.Black : Color.White);

            byte[] Array;
            using (var Stream = new MemoryStream())
            {
                Image.Save(Stream, System.Drawing.Imaging.ImageFormat.Png);
                Array = Stream.ToArray();
            }
            return File(Array, "image/jpeg");
        }
    }
}