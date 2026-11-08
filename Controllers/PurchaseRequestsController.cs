using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PurchaseApp.Data;
using PurchaseApp.Models;
using PurchaseApp.Services;

namespace PurchaseApp.Controllers
{
    public class PurchaseRequestsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PurchaseRequestWorkflowService _workflow;
        private readonly PurchaseRequestApiQueue _apiQueue;

        public PurchaseRequestsController(
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

        public IActionResult Error()
        {
            return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Amount,Department,ProposedSupplier,DesiredDeliveryDate,ApplicantName")] PurchaseRequest purchaseRequest)
        {
            if (ModelState.IsValid)
            {
                purchaseRequest.DesiredDeliveryDate = DateTime.SpecifyKind(purchaseRequest.DesiredDeliveryDate, DateTimeKind.Utc);
                purchaseRequest.Status = "Draft";
                purchaseRequest.CreatedAt = DateTime.UtcNow;

                _context.Add(purchaseRequest);
                await _context.SaveChangesAsync();

                _workflow.LogAction(purchaseRequest.Id, "Created", purchaseRequest.ApplicantName, $"Request created with amount {purchaseRequest.Amount}€.");
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(purchaseRequest);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var purchaseRequest = await _context.PurchaseRequests.FindAsync(id);
            if (purchaseRequest == null) return NotFound();
            if (purchaseRequest.Status != "Draft") return RedirectToAction(nameof(Index));
            return View(purchaseRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Amount,Department,ProposedSupplier,DesiredDeliveryDate,ApplicantName")] PurchaseRequest purchaseRequest)
        {
            if (id != purchaseRequest.Id) return NotFound();
            var existingRequest = await _context.PurchaseRequests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (existingRequest == null) return NotFound();
            if (existingRequest.Status != "Draft") return RedirectToAction(nameof(Index));

            if (ModelState.IsValid)
            {
                try
                {
                    purchaseRequest.Status = existingRequest.Status;
                    purchaseRequest.CreatedAt = DateTime.SpecifyKind(existingRequest.CreatedAt, DateTimeKind.Utc);
                    purchaseRequest.DesiredDeliveryDate = DateTime.SpecifyKind(purchaseRequest.DesiredDeliveryDate, DateTimeKind.Utc);

                    _context.Update(purchaseRequest);
                    _workflow.LogAction(purchaseRequest.Id, "Edited", purchaseRequest.ApplicantName, "Request draft edited.");
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseRequestExists(purchaseRequest.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(purchaseRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            var request = await _context.PurchaseRequests.FindAsync(id);
            if (request == null) return NotFound();

            if (request.Status == "Draft")
            {
                request.Status = "Pending Approval";
                request.RejectionReason = null;
                _context.Update(request);

                _workflow.LogAction(request.Id, "Submitted", request.ApplicantName, "Request submitted for approval.");
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Review()
        {
            var requests = await _context.PurchaseRequests
                .Where(r => r.Status != "Draft" && !r.IsHidden)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HideAllApproved()
        {
            var approvedRequests = await _context.PurchaseRequests
                .Where(r => r.Status == "Approved")
                .ToListAsync();

            foreach (var req in approvedRequests)
            {
                req.IsHidden = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Review));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveDepartmentHead(int id)
        {
            var request = await _context.PurchaseRequests.FindAsync(id);
            if (request != null && request.Status == "Pending Approval")
            {
                request.IsDepartmentHeadApproved = true;
                _workflow.LogAction(request.Id, "Approved_DeptHead", "Department Head", "Department head approval granted.");
                var shouldProcess = _workflow.UpdateApprovalStatus(request);
                await _context.SaveChangesAsync();
                if (shouldProcess) await _apiQueue.EnqueueAsync(request.Id);
            }
            return RedirectToAction(nameof(Review));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveFinance(int id)
        {
            var request = await _context.PurchaseRequests.FindAsync(id);
            if (request != null && request.Status == "Pending Approval" && request.Amount > 500)
            {
                request.IsFinanceApproved = true;
                _workflow.LogAction(request.Id, "Approved_Finance", "Finance", "Finance approval granted.");
                var shouldProcess = _workflow.UpdateApprovalStatus(request);
                await _context.SaveChangesAsync();
                if (shouldProcess) await _apiQueue.EnqueueAsync(request.Id);
            }
            return RedirectToAction(nameof(Review));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveDirector(int id)
        {
            var request = await _context.PurchaseRequests.FindAsync(id);
            if (request != null && request.Status == "Pending Approval" && request.Amount > 3000)
            {
                request.IsDirectorApproved = true;
                _workflow.LogAction(request.Id, "Approved_Director", "Director", "Director approval granted.");
                var shouldProcess = _workflow.UpdateApprovalStatus(request);
                await _context.SaveChangesAsync();
                if (shouldProcess) await _apiQueue.EnqueueAsync(request.Id);
            }
            return RedirectToAction(nameof(Review));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRequest(int id, string rejectionReason, string rejectedBy)
        {
            var request = await _context.PurchaseRequests.FindAsync(id);
            if (request != null && request.Status == "Pending Approval")
            {
                request.Status = "Rejected";
                request.RejectionReason = rejectionReason;
                _workflow.LogAction(request.Id, "Rejected", string.IsNullOrWhiteSpace(rejectedBy) ? "Approver" : rejectedBy, $"Request rejected. Reason: {rejectionReason}");
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Review));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevertToPending(int id, string performedBy)
        {
            var request = await _context.PurchaseRequests.FindAsync(id);
            if (request != null && request.Status == "Rejected")
            {
                request.Status = "Pending Approval";
                request.RejectionReason = null;
                _workflow.LogAction(request.Id, "Reverted_To_Pending", string.IsNullOrWhiteSpace(performedBy) ? "Approver" : performedBy, "Request status reverted from Rejected to Pending Approval.");
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Review));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceApprove(int id, string performedBy)
        {
            var request = await _context.PurchaseRequests.FindAsync(id);
            if (request != null && request.Status == "Rejected")
            {
                request.IsDepartmentHeadApproved = true;
                if (request.Amount > 500) request.IsFinanceApproved = true;
                if (request.Amount > 3000) request.IsDirectorApproved = true;

                request.RejectionReason = null;
                _workflow.LogAction(request.Id, "Force_Approved", string.IsNullOrWhiteSpace(performedBy) ? "Approver" : performedBy, "Request forcefully approved after previous rejection.");
                var shouldProcess = _workflow.UpdateApprovalStatus(request);
                await _context.SaveChangesAsync();
                if (shouldProcess) await _apiQueue.EnqueueAsync(request.Id);
            }
            return RedirectToAction(nameof(Review));
        }

        private bool PurchaseRequestExists(int id)
        {
            return _context.PurchaseRequests.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _context.PurchaseRequests.FindAsync(id);
            if (request != null && request.Status == "Draft")
            {
                _context.PurchaseRequests.Remove(request);
                _workflow.LogAction(request.Id, "Deleted", request.ApplicantName, "Draft request deleted by applicant.");
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetAllRequests()
        {
            var requests = await _context.PurchaseRequests.ToListAsync();

            foreach (var req in requests)
            {
                req.IsDepartmentHeadApproved = false;
                req.IsFinanceApproved = false;
                req.IsDirectorApproved = false;
                req.Status = "Draft";
                req.PurchaseOrderNumber = null;
                req.RejectionReason = null;
                req.ApiStatusMessage = null;
            }

            _workflow.LogAction(null, "Reset_All", "Developer", "All purchase requests reset.");
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Review));
        }
    }
}

