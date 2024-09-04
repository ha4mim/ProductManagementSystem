using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Reporting.NETCore;
using System.Data;
using System.IO;

namespace ProductSellingApp.Controllers
{
    public class ReportController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IConfiguration configuration, ILogger<ReportController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public IActionResult PrintReport(int id)
        {
            try
            {
                var reportPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "InvoiceReport.rdlc");
                if (!System.IO.File.Exists(reportPath))
                {
                    _logger.LogError("Report file not found: " + reportPath);
                    return StatusCode(500, "Report file not found");
                }

                var localReport = new LocalReport
                {
                    ReportPath = reportPath
                };

                
                DataTable reportData = GetSaleData(id);
                if (reportData == null || reportData.Rows.Count == 0)
                {
                    _logger.LogError("No data found for Sale ID: " + id);
                    return NotFound("No data found for the specified Sale ID");
                }

                
                localReport.DataSources.Add(new ReportDataSource("DataSet1", reportData));

                
                var pdfBytes = localReport.Render("PDF");

                // Return the PDF file
                return File(pdfBytes, "application/pdf", "InvoiceReport.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error generating report: " + ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        private DataTable GetSaleData(int saleId)
        {
            DataTable dt = new DataTable();
            string connectionString = _configuration.GetConnectionString("SqlConnectionString");
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("GetSaleDetails", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SaleId", saleId);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }
    }
}
