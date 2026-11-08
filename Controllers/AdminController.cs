using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PurchaseApp.Data;
using PurchaseApp.Models;
using PurchaseApp.Services;

namespace PurchaseApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PurchaseRequestWorkflowService _workflow;
        private readonly PurchaseRequestApiQueue _apiQueue;

        public AdminController(
            AppDbContext context,
            PurchaseRequestWorkflowService workflow,
            PurchaseRequestApiQueue apiQueue)
        {
            _context = context;
            _workflow = workflow;
            _apiQueue = apiQueue;
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _context.PurchaseRequests
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        public async Task<IActionResult> AuditLogs()
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
            return View(logs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RetryApi(int id)
        {
            var request = await _context.PurchaseRequests.FindAsync(id);
            if (request != null && request.Status == "Failed")
            {
                request.Status = "Processing";
                _workflow.LogAction(request.Id, "Admin_Manual_Retry", "Administrator", "Administrator manually triggered API retry.");
                await _context.SaveChangesAsync();
                await _apiQueue.EnqueueAsync(request.Id);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _context.PurchaseRequests.FindAsync(id);
            if (request != null)
            {
                _context.PurchaseRequests.Remove(request);

                _workflow.LogAction(id, "Admin_Deleted", "Administrator", $"Request '{request.Title}' deleted by administrator.");

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllAuditRecords()
        {
            var allLogs = await _context.AuditLogs.ToListAsync();
            _context.AuditLogs.RemoveRange(allLogs);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AuditLogs));
        }
    }
}
