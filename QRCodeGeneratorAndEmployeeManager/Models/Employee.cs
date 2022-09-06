using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRCodeGeneratorAndEmployeeManager.Models
{
    [Table("EmployeeQRTable")]
    public class Employee
    {
        [Column("EmployeeID")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required(ErrorMessage = "Employee ID is required")]
        [Display(Name = "Employee ID")]
        public int EmployeeID { get; set; }

        [Column("EmployeeName")]
        [Display(Name = "Employee Name")]
        [Required(ErrorMessage = "Employee Name is required")]
        [StringLength(15, ErrorMessage = "Employee Name must be less than 10 characters")]
        public string? EmployeeName { get; set; }

        [Column("EmployeeLastName")]
        [Display(Name = "Employee Last Name")]
        [Required(ErrorMessage = "Employee Last Name is required")]
        [StringLength(10, ErrorMessage = "Employee Last Name must be less than 10 characters")]
        public string? EmployeeLastName { get; set; }

        [Column("Phone")]
        [Display(Name = "Phone")]
        [Required(ErrorMessage = "Phone is required")]
        [StringLength(10, ErrorMessage = "Phone must be less than 10 characters")]
        public string? Phone { get; set; }

        [Column("encryptedData")]
        [Display(Name = "encryptedData")]
        public string? encryptedData { get; set; }

        [Column("nonEncryptedData")]
        [Display(Name = "nonEncryptedData")]
        public string? nonEncryptedData { get; set; }
    }
}
