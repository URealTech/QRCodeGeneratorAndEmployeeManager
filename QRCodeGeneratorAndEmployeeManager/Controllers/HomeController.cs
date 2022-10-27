using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QRCodeGeneratorAndEmployeeManager.Models;
using System.Diagnostics;
using IronBarCode;
using System.Drawing;
using System.Security.Cryptography;

namespace QRCodeGeneratorAndEmployeeManager.Controllers
{
    public class HomeController : Controller
    {
        private static byte[] IV = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public string qrCodeTextString;
        public string EmployeeData;
        public string EmployeeDataNon;
        public string symetricKey = "ePPhSSJIguIlaLys";

        private readonly IWebHostEnvironment _environment;
        private AppDbContext db = null;

        public HomeController(AppDbContext _db, IWebHostEnvironment environment)
        {
            this.db = _db;
            _environment = environment;
        }

        private void FillUsers()
        {
            List<SelectListItem> employees = (from c in db.EmployeeQRTable
                                          orderby c.EmployeeName ascending
                                          select new SelectListItem
                                          {
                                              Text = c.EmployeeName,
                                              Value = c.EmployeeName
                                          }).ToList();
            ViewBag.EmployeeQRTable = employees;
        }

        [HttpGet]
        public IActionResult List()
        {
            List<Employee> model = (from e in db.EmployeeQRTable 
                                    orderby e.EmployeeID 
                                    select e).ToList();
            return View(model);
        }

        public IActionResult Insert()
        {
            FillUsers();
            return View();
        }

        [HttpPost]
        public IActionResult Insert(Employee model)
        {
            FillUsers();
            if (ModelState.IsValid)
            {
                db.EmployeeQRTable.Add(model);

                EmployeeData += model.EmployeeName + " " + model.EmployeeLastName + " " + model.Phone;
                model.encryptedData = Encrypt(EmployeeData, symetricKey, IV);
                model.nonEncryptedData = EmployeeData;

        db.SaveChanges();
                ViewBag.Message = "Employee Inserted";
                ViewBag.QRCodeUri = IronBarcodeQR(model.nonEncryptedData);
            }
            return View(model);
        }   

        public IActionResult Update(int id)
        {
            FillUsers();
            Employee model = db.EmployeeQRTable.Find(id);
            return View(model);
        }
        [HttpPost]
        public IActionResult Update(Employee model)
        {
            FillUsers();
            if (ModelState.IsValid)
            {
                db.EmployeeQRTable.Update(model);

                EmployeeData += model.EmployeeName + " " + model.EmployeeLastName + " " + model.Phone;
                model.encryptedData = Encrypt(EmployeeData, symetricKey, IV);
                model.nonEncryptedData = EmployeeData;
                db.SaveChanges();
                ViewBag.Message = "Employee updated succesfully";
            }
            return View(model);
        }

        [ActionName("Delete")]
        public IActionResult ConfirmDelete(int id)
        {
            Employee model = db.EmployeeQRTable.Find(id);
            return View(model);
        }
        [HttpPost]
        public IActionResult Delete(int employeeID)
        {
            Employee model = db.EmployeeQRTable.Find(employeeID);
            db.EmployeeQRTable.Remove(model);
            db.SaveChanges();
            TempData["Message"] = "Employee Deleted";
            return RedirectToAction("List");
        }

        [HttpGet]
        public IActionResult CreateQRCode(int id)
        {
            FillUsers();
            Employee model = db.EmployeeQRTable.Find(id);
            return View(model);
        }

        [HttpPost]
        public IActionResult CreateQRCode(Employee model)
        {
            FillUsers();
            if (ModelState.IsValid)
            {
                EmployeeData += model.EmployeeName + " " + model.EmployeeLastName + " " + model.Phone;
                model.encryptedData = Encrypt(EmployeeData, symetricKey, IV);
                model.nonEncryptedData = EmployeeData;
                ViewBag.url = IronBarcodeQR(model.nonEncryptedData);
            }
            return View(model);
        }
        private string Encrypt(string plainText, string Password, byte[] IV)
        {
            byte[] Key = System.Text.Encoding.UTF8.GetBytes(Password);

            AesManaged aes = new AesManaged();
            aes.Key = Key;
            aes.IV = IV;

            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

            byte[] InputBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            cryptoStream.Write(InputBytes, 0, InputBytes.Length);
            cryptoStream.FlushFinalBlock();

            byte[] Encrypted = memoryStream.ToArray();
            return Convert.ToBase64String(Encrypted);
        }

        private string IronBarcodeQR(string dataText)
        {

            GeneratedBarcode barcode = QRCodeWriter.CreateQrCodeWithLogo(dataText, "wwwroot/Logo/github.png", 300);
            barcode.SetMargins(10);
            barcode.ChangeBarCodeColor(Color.Goldenrod);
            string path = Path.Combine(_environment.WebRootPath, "GeneratedQRCode");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filePath = Path.Combine(_environment.WebRootPath, "GeneratedQRCode/qrcodewithlogo.png");
            barcode.SaveAsPng(filePath);
            string fileName = Path.GetFileName(filePath);
            string imageUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}" + "/GeneratedQRCode/" + fileName;
            ViewBag.QRCodeUri = imageUrl;

            return imageUrl;
        }
    }
}