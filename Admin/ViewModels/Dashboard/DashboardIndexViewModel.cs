using Application.Dashboard.DTOs;

namespace Admin.ViewModels.Dashboard;

/// <summary>營運總覽頁 ViewModel。</summary>
public sealed class DashboardIndexViewModel
{
    public DashboardDto Data { get; init; } = new();
}
