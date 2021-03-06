﻿using System.Collections.Generic;

namespace Gac.Logistics.Aes.Api.Model.SubClasses
{
    public class SubmissionResponse
    {
        public SubmissionResponse()
        {
            this.CustomsResponseList = new List<CustomsResponse>();
        }
        public string Status { get; set; }
        public string Description { get; set; }
        public string ItnNumber { get; set; }
        public List<CustomsResponse> CustomsResponseList { get; set; }
    }

    public class CustomsResponse
    {
        public string ResponseCode { get; set; }
        public string SeverityIndicator { get; set; }
        public string NarrativeText { get; set; }
        public string ReasonCode { get; set; }
        public string FinalDestinationIndicator { get; set; }
    }
}