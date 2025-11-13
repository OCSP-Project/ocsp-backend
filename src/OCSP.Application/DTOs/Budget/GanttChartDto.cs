using System.Text.Json.Serialization;

namespace OCSP.Application.DTOs.Budget
{
    public class GanttChartDataDto
    {
        [JsonPropertyName("projectId")]
        public Guid ProjectId { get; set; }

        [JsonPropertyName("projectName")]
        public string ProjectName { get; set; } = string.Empty;

        [JsonPropertyName("projectStartDate")]
        public DateTime ProjectStartDate { get; set; }

        [JsonPropertyName("projectEndDate")]
        public DateTime ProjectEndDate { get; set; }

        [JsonPropertyName("items")]
        public List<GanttTaskDto> Items { get; set; } = new();

        [JsonPropertyName("timeline")]
        public GanttTimelineDto Timeline { get; set; } = new();
    }

    public class GanttTaskDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("orderNumber")]
        public string OrderNumber { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("parentId")]
        public Guid? ParentId { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("order")]
        public int Order { get; set; }

        [JsonPropertyName("hasChildren")]
        public bool HasChildren { get; set; }

        [JsonPropertyName("isExpanded")]
        public bool IsExpanded { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime? StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("progress")]
        public decimal Progress { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("statusColor")]
        public string StatusColor { get; set; } = string.Empty;

        [JsonPropertyName("assignees")]
        public List<AssigneeDto> Assignees { get; set; } = new();

        [JsonPropertyName("dependencies")]
        public List<Guid> Dependencies { get; set; } = new();

        [JsonPropertyName("children")]
        public List<GanttTaskDto> Children { get; set; } = new();

        // For Gantt positioning
        [JsonPropertyName("dayOffset")]
        public int DayOffset { get; set; }                               // Days from project start

        [JsonPropertyName("barWidth")]
        public int BarWidth { get; set; }                                // Width in pixels
    }

    public class GanttTimelineDto
    {
        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("totalDays")]
        public int TotalDays { get; set; }

        [JsonPropertyName("totalWeeks")]
        public int TotalWeeks { get; set; }

        [JsonPropertyName("months")]
        public List<GanttMonthDto> Months { get; set; } = new();

        [JsonPropertyName("weeks")]
        public List<GanttWeekDto> Weeks { get; set; } = new();
    }

    public class GanttMonthDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Label { get; set; } = string.Empty;                // "April 2025"
        public int WeekCount { get; set; }
        public int PixelWidth { get; set; }
    }

    public class GanttWeekDto
    {
        [JsonPropertyName("weekNumber")]
        public int WeekNumber { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;                // "Week 16"

        [JsonPropertyName("pixelWidth")]
        public int PixelWidth { get; set; } = 60;                        // Fixed 60px per week
    }
}
