using PurchaseApp.Data;
using PurchaseApp.Models;

namespace PurchaseApp.Services;

public sealed class PurchaseRequestWorkflowService
{
    private readonly AppDbContext _context;

    public PurchaseRequestWorkflowService(AppDbContext context)
    {
        _context = context;
    }

    public void LogAction(int? requestId, string action, string performedBy, string details)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            PurchaseRequestId = requestId,
            Action = action,
            PerformedBy = performedBy,
            Details = details,
            Timestamp = DateTime.UtcNow
        });
    }

    public bool UpdateApprovalStatus(PurchaseRequest request)
    {
        bool approvalsComplete = request.IsDepartmentHeadApproved
            && (request.Amount <= 500 || request.IsFinanceApproved)
            && (request.Amount <= 3000 || request.IsDirectorApproved);

        if (!approvalsComplete)
        {
            request.Status = "Pending Approval";
            return false;
        }

        request.Status = "Processing";
        LogAction(request.Id, "Processing", "System", "All required approvals obtained. Processing API in background.");
        return true;
    }
}
