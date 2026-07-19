using Microsoft.AspNetCore.Razor.TagHelpers;
using UniDocs.Models.Enums;

namespace UniDocs.TagHelpers
{
    // <status-text>
    [HtmlTargetElement("status-text")]
    public class StatusTextTagHelper : TagHelper
    {
        public ReportStatusEnum? Status { get; set; }
        public DocumentStatusEnum? DocStatus { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            output.TagMode = TagMode.StartTagAndEndTag;

            if (Status != null)
            {
                // Report
                if (Status == ReportStatusEnum.Pending)
                {
                    output.Attributes.SetAttribute("class", "text-warning fw-bold");
                    output.Content.SetContent("Pending");
                }
                else if (Status == ReportStatusEnum.Finished)
                {
                    output.Attributes.SetAttribute("class", "text-success fw-bold");
                    output.Content.SetContent("Finished");
                }
            }

            if (DocStatus != null)
            {
                // Docs
                if (DocStatus == DocumentStatusEnum.Pending)
                {
                    output.Attributes.SetAttribute("class", "text-warning fw-bold");
                    output.Content.SetContent("Pending");
                }
                else if (DocStatus == DocumentStatusEnum.Deleted)
                {
                    output.Attributes.SetAttribute("class", "text-danger fw-bold");
                    output.Content.SetContent("Deleted");
                }
            }

        }

    }
}
