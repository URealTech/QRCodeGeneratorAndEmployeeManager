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
        public string SymetricKey = "ePPhSSJIguIlaLys";
        public string qrCodeTextString;
        public string EmployeeData;
        public string EmployeeDataNon;

        private readonly IWebHostEnvironment _environment;
        private AppDbContext db = null;

        public HomeController(AppDbContext _db, IWebHostEnvironment environment)
        {
            this.db = _db;
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
            List<Employee> model = (from e in db.EmployeeQRTable orderby e.EmployeeID select e).ToList();
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
                model.encryptedData = Encrypt(EmployeeData, SymetricKey, IV);
                model.nonEncryptedData = EmployeeData;

        db.SaveChanges();
                ViewBag.Message = "Employee Inserted";
                #region IronBarcode
                try
                {
                    GeneratedBarcode barcode = QRCodeWriter.CreateQrCodeWithLogo(model.nonEncryptedData, "github.png", 300);
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
                }
                catch (Exception)
                {
                    throw;
                }
                #endregion
            }
            return View(model);
        }

        public IActionResult InsertEncrypted()
        {
            FillUsers();
            return View();
        }

        [HttpPost]
        public IActionResult InsertEncrypted(Employee model)
        {
            FillUsers();
            if (ModelState.IsValid)
            {
                db.EmployeeQRTable.Add(model);

                EmployeeData += model.EmployeeName + " " + model.EmployeeLastName + " " + model.Phone;
                model.encryptedData = Encrypt(EmployeeData, SymetricKey, IV);
                model.nonEncryptedData = EmployeeData;

                db.SaveChanges();
                ViewBag.Message = "Employee Inserted";
                #region IronBarcode
                try
                {
                    GeneratedBarcode barcode = QRCodeWriter.CreateQrCodeWithLogo(model.encryptedData, "github.png", 300);
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
                }
                catch (Exception)
                {
                    throw;
                }
                #endregion
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
                model.encryptedData = Encrypt(EmployeeData, SymetricKey, IV);
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
                model.encryptedData = Encrypt(EmployeeData, SymetricKey, IV);
                model.nonEncryptedData = EmployeeData;
                #region IronBarcode
                try
                {
                    GeneratedBarcode barcode = QRCodeWriter.CreateQrCodeWithLogo(model.nonEncryptedData, "github.png", 300);
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
                }
                catch (Exception)
                {
                    throw;
                }
                #endregion
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateQRCodeEncrypted(int id)
        {
            FillUsers();
            Employee model = db.EmployeeQRTable.Find(id);
            return View(model);
        }

        [HttpPost]
        public IActionResult CreateQRCodeEncrypted(Employee model)
        {
            FillUsers();
            if (ModelState.IsValid)
            {
                EmployeeData += model.EmployeeName + " " + model.EmployeeLastName + " " + model.Phone;
                model.encryptedData = Encrypt(EmployeeData, SymetricKey, IV);
                model.nonEncryptedData = EmployeeData;
                #region IronBarcode
                try
                {
                    GeneratedBarcode barcode1 = QRCodeWriter.CreateQrCodeWithLogo(model.encryptedData, "github.png", 300);
                    barcode1.SetMargins(10);
                    barcode1.ChangeBarCodeColor(Color.Goldenrod);
                    string path1 = Path.Combine(_environment.WebRootPath, "GeneratedQRCode");
                    if (!Directory.Exists(path1))
                    {
                        Directory.CreateDirectory(path1);
                    }
                    string filePath1 = Path.Combine(_environment.WebRootPath, "GeneratedQRCode/qrcodewithlogo.png");
                    barcode1.SaveAsPng(filePath1);
                    string fileName1 = Path.GetFileName(filePath1);
                    string imageUrl1 = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}" + "/GeneratedQRCode/" + fileName1;
                    ViewBag.QRCodeUri1 = imageUrl1;
                }
                catch (Exception)
                {
                    throw;
                }
                #endregion
            }
            return View(model);
        }
    }
}