using System;
using System.Collections.Generic;

namespace ApartmentManagementSystem.Application.DTOs.CustomerDto
{
    public class MoveChecklistDto
    {
        public int Id { get; set; }
        public int LeaseId { get; set; }
        public string ChecklistType { get; set; }
        public DateTime InspectionDate { get; set; }
        public string InspectorName { get; set; }
        public string OverallCondition { get; set; }
        public string Notes { get; set; }
        public List<ChecklistItemDto> ChecklistItems { get; set; } = new List<ChecklistItemDto>();
    }

    public class ChecklistItemDto
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public string Condition { get; set; }
        public string Notes { get; set; }
        public bool IsDamage { get; set; }
        public List<string> PhotoUrls { get; set; } = new List<string>();
    }
}
